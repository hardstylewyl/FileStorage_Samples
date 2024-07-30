namespace FileStorage.NativeLocal;

public sealed class NativeLocalOptions
{
	//    配置文件夹路径 用于记录文件的分片上传情况
	public string ConfigDirectoryPath { get; set; } = string.Empty;

	//   缓存文件夹路径
	public string TempDirectoryPath { get; set; } = string.Empty;

	/// <summary>
	///     片大小 默认5MB
	///     服务端和客户端需保持一致
	/// </summary>
	public long ChunkSize { get; set; } = 5242880L;
}
