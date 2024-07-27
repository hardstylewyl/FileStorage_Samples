namespace FileStorage.TusLocal.UrlStorage;

public sealed class PreviousUpload
{
	public string CreationTime { get; set; } = string.Empty;
	public object Metadata { get; set; } = string.Empty;
	public long Size { get; set; } = 0;
	public string UploadUrl { get; set; } = string.Empty;
	public string? UrlStorageKey { get; set; } = string.Empty;
}
