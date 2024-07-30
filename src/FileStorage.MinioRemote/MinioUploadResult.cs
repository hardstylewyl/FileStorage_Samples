namespace FileStorage.MinioRemote;

public sealed class MinioUploadResult
{
	//文件访问url
	public string FileUrl { get; set; } = string.Empty;

	//minio返回的标识
	public string Etag { get; set; } = string.Empty;

	//minio桶名称
	public string BucketName { get; set; } = string.Empty;

	//minio对象名
	public string ObjectName { get; set; } = string.Empty;

	public bool IsSuccess { get; set; } = false;
	public Exception? Exception { get; set; }
}
