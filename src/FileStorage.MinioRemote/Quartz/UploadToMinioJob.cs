using FileStorage.EntityFramework;
using Microsoft.Extensions.Logging;
using Polly;
using Quartz;

namespace FileStorage.MinioRemote.Quartz;

public sealed class UploadToMinioJob(
	ILogger<UploadToMinioJob> logger,
	IFileMetadataManager metaManager,
	MinioFileService minioFileService,
	MinioSyncStateCache minioSyncStateCache)
	: IJob
{
	private static readonly AsyncPolicy UploadPolicy = Policy.
		Handle<Exception>()
		.WaitAndRetryAsync(3, at => TimeSpan.FromSeconds(at * 5));

	public async Task Execute(IJobExecutionContext context)
	{

		if (context.MergedJobDataMap.TryGetValue(UploadToMinioJobArgs.Key, out var a)
			&& a is UploadToMinioJobArgs args)
		{
			try
			{
				//开启同步状态
				await minioSyncStateCache.StartSyncStateAsync(args.FileId);
				//进行上传
				await UploadPolicy
					.ExecuteAsync(ct => DoExecuteAsync(args, ct),
					context.CancellationToken);
			}
			catch (Exception ex)
			{
				//出错了写状态
				await minioSyncStateCache.SyncErrorAsync(args.FileId, ex.Message, ex.StackTrace!);
				throw;
			}

		}

		logger.LogError("Exec UploadToMinio Task Not Args {Time}", DateTimeOffset.Now);
	}


	private async Task DoExecuteAsync(UploadToMinioJobArgs args, CancellationToken ct)
	{
		var fileId = args.FileId;
		var extension = args.Extension;
		var fileInfo = new FileInfo(args.FilePath);

		if (!fileInfo.Exists)
		{
			logger.LogWarning("FileId [{id}] In [{path}] Not Found ", fileId, args.FilePath);
			return;
		}

		var meta = await metaManager.FindAsync(fileId);
		if (meta != null)
		{
			logger.LogWarning("FileId [{id}] Already Uploaded ", fileId);
			return;
		}

		//上传
		var result = await minioFileService.UploadAsync(fileInfo.OpenRead(), fileId, extension, ct);
		if (!result.IsSuccess)
		{
			throw result.Exception!;
		}

		//存储
		await metaManager.CreateAsync(new FileMetadataEntity(
			args.FileName,
			args.UserId,
			result.BucketName,
			result.ObjectName,
			result.FileUrl,
			args.Size,
			args.Extension,
			args.Md5));

		//上传成功执行回调
		await args.OnSuccessFunc(ct);
	}
}
