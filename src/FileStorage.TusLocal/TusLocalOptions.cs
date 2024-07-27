namespace FileStorage.TusLocal;

public sealed class TusLocalOptions
{
	//     tus文件存储目录
	public string DirectoryPath { get; set; } = string.Empty;

	//     tus文件元数据存储目录
	public string MetaDirectoryPath { get; set; } = string.Empty;
}
