namespace FileStorage.Api.Contracts.Requests;

public sealed class CheckFileRequest
{
	public bool IsTus { get; set; }
	public string FileId { get; set; } = string.Empty;
}
