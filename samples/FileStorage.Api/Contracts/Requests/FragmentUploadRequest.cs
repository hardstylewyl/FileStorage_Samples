namespace FileStorage.Api.Contracts.Requests;

public sealed class FragmentUploadRequest
{
	public string FileId { get; set; } = string.Empty;
	public int ChunkSeq { get; set; } = 0;
	public string Md5 { get; set; } = string.Empty;
	public IFormFile File { get; set; } = null!;
}
