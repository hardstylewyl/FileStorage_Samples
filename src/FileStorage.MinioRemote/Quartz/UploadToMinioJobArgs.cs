namespace FileStorage.MinioRemote.Quartz;

public sealed class UploadToMinioJobArgs(
	string userId, string fileId, string filename,
	string extension, string filePath, string md5,
	long size)
{
	public const string Key = nameof(UploadToMinioJobArgs);
	public string UserId { get; set; } = userId;
	public string FileId { get; set; } = fileId;
	public string FileName { get; set; } = filename;
	public string Extension { get; set; } = extension;
	public string FilePath { get; set; } = filePath;
	public string Md5 { get; set; } = md5;
	public long Size { get; set; } = size;

	public Func<CancellationToken, Task> OnSuccessFunc { get; set; }
		= ct => Task.CompletedTask;
	public Func<CancellationToken, Task> OnFailedFunc { get; set; }
		= ct => Task.CompletedTask;

	public UploadToMinioJobArgs WithSuccessCallback(Func<CancellationToken, Task> func)
	{
		OnSuccessFunc = func;
		return this;
	}
	public UploadToMinioJobArgs WithFailedCallback(Func<CancellationToken, Task> func)
	{
		OnFailedFunc = func;
		return this;
	}
}
