namespace FileStorage.MinioRemote;

public sealed class MinioSyncState
{
	public string FileId { get; set; } = string.Empty;
	public int Progress { get; set; } = 0;
	public bool SyncError { get; set; } = false;
	public string ErrorMessage { get; set; } = string.Empty;
	public string ErrorTrace { get; set; } = string.Empty;
}
