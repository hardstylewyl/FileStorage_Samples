using Microsoft.AspNetCore.Http;

namespace FileStorage.NativeLocal.Models;

public sealed class FragmentUploadContext
{
	public string FileId { get; set; } = string.Empty;
	public IFormFile FormFile { get; set; } = null!;
	public int ChunkSeq { get; set; } = 0;
	public bool IsCheckMd5 { get; set; } = false;
	public string Md5 { get; set; } = string.Empty;
	public Func<NativeFileInfo, Task>? OnCompletedCallbackFunc { get; set; }
}
