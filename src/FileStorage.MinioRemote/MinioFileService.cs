using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;

namespace FileStorage.MinioRemote;

public class UploadToMinioProgressReport(ILogger logger, string fileId) : IProgress<ProgressReport>
{
	public void Report(ProgressReport value)
	{
		var progres = value.Percentage * 1.0 / value.TotalBytesTransferred;
		logger.LogInformation("FileId [{id}] Upload Progress [{pro}]", fileId, progres);
	}
}

public sealed class MinioFileService(
	ILogger<MinioFileService> logger,
	IOptions<MinioRemoteOptions> options)
	: MinioClient
{
	private MinioRemoteOptions Options = options.Value;
	private readonly string BucketName = options.Value.BucketName;

	public async Task<MinioUploadResult> UploadAsync(Stream stream, string fileId, string extension, CancellationToken cancellationToken)
	{
		var noice = Guid.NewGuid().ToString("N");
		var objectName = noice + "." + extension;


		var args = new PutObjectArgs()
			.WithBucket(BucketName)
			.WithObject(objectName)
			.WithStreamData(stream)
			.WithProgress(new UploadToMinioProgressReport(logger, fileId))
			.WithObjectSize(stream.Length);

		try
		{
			var resp = await PutObjectAsync(args, cancellationToken);
			return new MinioUploadResult()
			{
				BucketName = BucketName,
				ObjectName = objectName,
				Etag = resp.Etag,
				FileUrl = $"{Options.Endpoint}/{BucketName}/{objectName}",
				IsSuccess = true
			};
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "FileId [{Id}] Upload To Minio Error ", fileId);
			return new MinioUploadResult()
			{
				IsSuccess = false,
				Exception = ex
			};
		}
		finally
		{
			await stream.DisposeAsync();
		}
	}

}
