namespace FileStorage.MinioRemote;

public sealed class MinioRemoteOptions
{
	public string Endpoint { get; set; } = string.Empty;
	public string AccessKey { get; set; } = string.Empty;
	public string SecretKey { get; set; } = string.Empty;
	//启用SSL
	public bool EnableSSL { get; set; } = false;


	//存储桶名称
	public string BucketName { get; set; } = string.Empty;

	//访问文件的基本路径 host/存储桶名称/对象名称
	//public string AccessBaseUrl { get; set; } = string.Empty;

}
