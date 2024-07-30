namespace FileStorage.Api.Contracts.Requests;

public sealed class DirectUploadRequest
{
	// fileId: {md5}_{size}
	public string FileId { get; set; }

	public string Filename { get; set; }
	public string Extension { get; set; }
	public string Md5 { get; set; }
	public IFormFile File { get; set; }
}
