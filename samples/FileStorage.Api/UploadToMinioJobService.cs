using FileStorage.Caching;
using FileStorage.MinioRemote;
using FileStorage.MinioRemote.Quartz;
using FileStorage.NativeLocal;
using FileStorage.NativeLocal.Models;
using FileStorage.TusLocal.UrlStorage;
using Quartz;
using SolidTUS.Handlers;
using SolidTUS.Models;

namespace FileStorage.Api;

public sealed class UploadToMinioJobService(
	ILogger<UploadToMinioJobService> logger,
	IRedisCache redisCache,
	IUploadStorageHandler uploadStorageHandler,
	IUploadMetaHandler uploadMetaHandler,
	NativeFileStore nativeFileStore,
	NativeFileConfigStore nativeFileConfigStore,
	ISchedulerFactory schedulerFactory,
	MinioSyncStateCache syncStateCache)
{
	public async Task StartScheduleJob(UploadFileInfo info)
	{
		var userId = "uid0";
		var fileId = info.FileId;
		var filePath = Path.Combine(info.OnDiskDirectoryPath ?? "/", info.OnDiskFilename);
		var filename = info.Metadata!.TryGetValue("filename", out var f) ? f : string.Empty;
		var extension = filename.Split('.').Last();
		var fileIdSplitor = fileId.Split('_');
		var md5 = fileIdSplitor[0];
		var size = long.TryParse(fileIdSplitor[1], out var s) ? s : 0;

		//当同步任务已经被开启了，不要继续开启
		if (await syncStateCache.HasSyncAsync(fileId))
		{
			logger.LogWarning("FileId [{id}] Upload To Minio Task Exists", fileId);
			return;
		}

		//构建任务参数
		var args = new UploadToMinioJobArgs(userId, fileId, filename,
			extension, filePath, md5, size);

		//上传成功回调
		args.WithSuccessCallback(async ct =>
		{
			//删除文件上传信息缓存
			await TusUrlStorageApi.RemoveUpload(redisCache, fileId);
			//删除文件缓存
			await uploadStorageHandler.DeleteFileAsync(info, default);
			//删除配置信息
			await uploadMetaHandler.DeleteUploadFileInfoAsync(info, default);
			//删除同步状态信息缓存
			await syncStateCache.RemoveAsync(fileId);
		});

		//调度开始
		await StartScheduleJobCore(args);
	}

	public async Task StartScheduleJob(NativeFileInfo info)
	{
		var userId = "uid1";
		var fileId = info.FileId;
		var filePath = Path.Combine(info.OnDiskDirectoryPath ?? "/", info.OnDiskFilename);
		var filename = info.Filename;
		var extension = filename.Split('.').Last();
		var fileIdSplitor = fileId.Split('_');
		var md5 = fileIdSplitor[0];
		var size = long.TryParse(fileIdSplitor[1], out var s) ? s : 0;

		//当同步任务已经被开启了，不要继续开启
		if (await syncStateCache.HasSyncAsync(fileId))
		{
			logger.LogWarning("FileId [{id}] Upload To Minio Task Exists", fileId);
			return;
		}

		//构建任务参数
		var args = new UploadToMinioJobArgs(userId, fileId, filename,
			extension, filePath, md5, size);

		//上传成功回调
		args.WithSuccessCallback(async ct =>
		{
			//删除文件缓存
			await nativeFileStore.RemoveTempAsync(info);
			//删除配置信息
			await nativeFileConfigStore.RemoveInfoAsync(info);
			//删除同步状态信息缓存
			await syncStateCache.RemoveAsync(fileId);
		});

		//调度开始
		await StartScheduleJobCore(args);
	}

	private async Task StartScheduleJobCore(UploadToMinioJobArgs args)
	{
		//开启一个调度
		var scheduler = await schedulerFactory.GetScheduler();
		if (!scheduler.IsStarted)
			await scheduler.Start();

		var jobKey = new JobKey(Guid.NewGuid().ToString("N"), nameof(UploadToMinioJob));
		var triggerKey = new TriggerKey(Guid.NewGuid().ToString("N"), nameof(UploadToMinioJob));

		//定义Job
		var jobDetail = JobBuilder.Create<UploadToMinioJob>()
			.WithIdentity(jobKey)
			.Build();

		//填充job执行所需参数
		jobDetail.JobDataMap.Put(UploadToMinioJobArgs.Key, args);

		//定义触发器
		var trigger = TriggerBuilder.Create()
			.WithIdentity(triggerKey)
			.StartNow()
			.Build();

		//调度任务
		await scheduler.ScheduleJob(jobDetail, trigger);
	}
}
