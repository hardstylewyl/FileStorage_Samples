using Microsoft.AspNetCore.Http;

namespace FileStorage.NativeLocal.Model;

public sealed class FragmentUploadContext
{
	public string FileId { get; set; } = string.Empty;
	public IFormFile FormFile { get; set; } = null!;
	public int ChunkSeq { get; set; } = 0;
	public bool IsCheckMd5 { get; set; } = false;
	public string Md5 { get; set; } = string.Empty;
	public Func<NativeFileInfo, Task>? OnCompletedCallbackFunc { get; set; }
}


public sealed class FragmentUploadContextBuilder(string fileId, IFormFile formFile, int chunkSeq)
{
	private string FileId { get; } = fileId;
	private IFormFile FormFile { get; } = formFile;
	private int ChunkSeq { get; set; } = chunkSeq;
	private bool IsCheckMd5 { get; set; } = false;
	private string Md5 { get; set; } = string.Empty;
	private Func<NativeFileInfo, Task>? OnCompletedCallbackFunc { get; set; }

	public FragmentUploadContextBuilder OnFragmentUploadCompleted(Func<NativeFileInfo, Task> func)
	{
		OnCompletedCallbackFunc = func;
		return this;
	}

	public FragmentUploadContextBuilder EnableCheckMd5(string md5)
	{
		IsCheckMd5 = true;
		Md5 = md5;
		return this;
	}

	public FragmentUploadContext Build()
	{
		return new FragmentUploadContext
		{
			FileId = FileId,
			FormFile = FormFile,
			ChunkSeq = ChunkSeq,
			IsCheckMd5 = IsCheckMd5,
			Md5 = Md5,
			OnCompletedCallbackFunc = OnCompletedCallbackFunc
		};
	}
}
