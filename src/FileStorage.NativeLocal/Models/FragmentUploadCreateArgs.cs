namespace FileStorage.NativeLocal.Models;

public sealed class FragmentUploadCreateArgs
{
	/// fileId: {md5}_{size}
	public string FileId { get; set; } = string.Empty;
	public string Filename { get; set; } = string.Empty;
	public string Extension { get; set; } = string.Empty;
	public int ChunkCount { get; set; } = 0;
	public long ChunkSize { get; set; } = 0;
}
