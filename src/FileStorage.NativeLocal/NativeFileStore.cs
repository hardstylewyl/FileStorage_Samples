using FileStorage.NativeLocal.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileStorage.NativeLocal;

public sealed class NativeFileStore(ILogger<NativeFileStore> logger, IOptions<NativeLocalOptions> options)
{
	private const int BufferSize = 4096;
	private readonly string TempDirectoryPath = options.Value.TempDirectoryPath;

	//写入分片数据
	public async Task<bool> WriteFragmentDataAsync(NativeFileInfo info, long chunkSeq, Stream stream)
	{
		var fileId = info.FileId;
		var chunkSize = info.ChunkSize;
		try
		{
			var filePath = TempFilePath(info);

			//注意设置文件流参数
			await using var tempFileStream = new FileStream(filePath,
				FileMode.OpenOrCreate,
				FileAccess.Write,
				FileShare.Write,
				bufferSize: BufferSize,
				useAsync: true);

			//将文件指针位置偏移到指定位置 offset = 片号 * 片大小(字节)
			var offset = chunkSeq * chunkSize;
			tempFileStream.Seek(offset, SeekOrigin.Begin);

			//构建缓存区
			var buffer = new byte[BufferSize];
			var readCount = 0;
			//循环读取请求流并写入本地文件流
			while ((readCount = await stream.ReadAsync(buffer.AsMemory(0, BufferSize))) > 0)
			{
				await tempFileStream.WriteAsync(buffer.AsMemory(0, readCount));
			}

			//一次性开启大缓存区
			// var buffer = new byte[stream.Length];
			// var readCount = await stream.ReadAsync(buffer);
			// await tempFileStream.WriteAsync(buffer.AsMemory(0, readCount));

			//写入分片成功记录成功位置
			info.WriteSuccessByte(chunkSeq);
			return true;

		}
		catch (Exception ex)
		{
			logger.LogError(ex, "FileId [{FileId}] Temp ChunkSize [{}] ChunkSeq [{}] Write Error", fileId, chunkSize, chunkSeq);
			return false;
		}
	}

	//移除缓存
	public Task<bool> RemoveTempAsync(NativeFileInfo info)
	{
		var fileId = info.FileId;
		try
		{
			var tempPath = TempFilePath(info);
			if (!File.Exists(tempPath))
				return Task.FromResult(true);

			File.Delete(tempPath);
			return Task.FromResult(!File.Exists(tempPath));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "FileId [{FileId}] Temp Remove Error", fileId);
			return Task.FromResult(false);
		}
	}

	//构建缓存路径
	private string TempFilePath(NativeFileInfo info)
	{
		//info的目录优先级大于配置
		var dirPath = info.OnDiskDirectoryPath ?? TempDirectoryPath;

		//当info没有path则使用配置值
		info.OnDiskDirectoryPath ??= dirPath;

		//使用磁盘上的文件名称，而不是源文件名称
		var filename = info.OnDiskFilename;

		return Path.Combine(TempDirectoryPath, filename);
	}

}
