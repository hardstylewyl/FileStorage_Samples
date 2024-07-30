namespace FileStorage.NativeLocal.Models;

public sealed class NativeFileInfo(
	string fileId,
	string filename,
	int chunkCount,
	long chunkSize,
	string onDiskFilename,
	string? onDiskDirectoryPath = null)
{
	private const byte SuccessByteFlag = 127;

	/// fileId: {md5}_{size}
	public string FileId { get; set; } = fileId;

	//原文件名
	public string Filename { get; set; } = filename;

	//分片总数
	public int ChunkCount { get; set; } = chunkCount;

	//分片大小（字节）
	public long ChunkSize { get; set; } = chunkSize;

	//分片状态列表 0:未上传 127:上传成功
	public byte[] ChunksStatus { get; set; } = new byte[chunkCount];

	//在磁盘上的文件名
	public string OnDiskFilename { get; set; } = onDiskFilename;

	//在磁盘上的文件夹
	public string? OnDiskDirectoryPath { get; set; } = onDiskDirectoryPath;

	//最后一次写入日期
	public DateTime LastWriteDate { get; set; }

	//写入成功字节
	public void WriteSuccessByte(long chunkSeq)
	{
		if (chunkSeq < 0 || chunkSeq >= ChunksStatus.Length)
			throw new ArgumentOutOfRangeException(nameof(chunkSeq));

		ChunksStatus[chunkSeq] = SuccessByteFlag;
		LastWriteDate = DateTime.Now;
	}

	//获取未完成的分片
	public IEnumerable<int> GetMissingChunks()
		=> ChunksStatus
		.Select((b, i) => b == 0 ? i : -1)
		.Where(i => i >= 0);

	//文件是否上传完整
	public bool IsCompleted()
		=> ChunksStatus
		.All(b => b == SuccessByteFlag);
}
