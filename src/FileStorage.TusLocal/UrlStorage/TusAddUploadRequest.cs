namespace FileStorage.TusLocal.UrlStorage;

public sealed class TusAddUploadRequest
{
	public string FileId { get; set; } = string.Empty;
	public PreviousUpload Value { get; set; } = default!;
}
