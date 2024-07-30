namespace FileStorage.Api.Contracts.Responses;

public enum FileUploadStatus
{
	//   文件不存在
	NotExist = 1,

	///     处于上传中，
	///     如果是本地上传还没上传完成需要返回未上传的分片号
	InProgress = 2,

	///     处于minio同步中
	InSynchronizing = 4,

	///     上传完成，直接返回url
	Completed = 8,

	///     同步失败
	SyncFailed = 16
}

public class CheckFileResponse
{
	///     指示文件上传状态
	public FileUploadStatus Status { get; set; }

	///     如果文件没有完全上传完成则返回未上传的分片号
	public List<int>? MissChunkList { get; set; }

	///     如果文件已经上传完成则返回文件的url
	public string? FileUrl { get; set; }
}
