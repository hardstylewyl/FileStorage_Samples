using System.Collections.Concurrent;
using System.Text.Json;
using FileStorage.NativeLocal.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileStorage.NativeLocal;

public sealed class NativeFileConfigStore(
	ILogger<NativeFileConfigStore> logger,
	IOptions<NativeLocalOptions> options)
{
	private readonly string ConfigDirectoryPath = options.Value.ConfigDirectoryPath;
	private const string ConfigFileSuffix = "config.json";

	//信号量集合 key:fileId 控制最多一个线程来访问(读/写)FileInfo
	private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoreSlims = new();

	public Task<bool> CreateOrUpdateInfoAsync(NativeFileInfo info)
	{
		return WriteInfoAsync(info);
	}

	public async Task<NativeFileInfo?> GetInfoAsync(string fileId)
	{
		var slim = _semaphoreSlims.GetOrAdd(fileId, _ => new SemaphoreSlim(1, 1));
		await slim.WaitAsync();

		try
		{
			var infoPath = ConfigFilePath(fileId);
			var infoExists = File.Exists(infoPath);

			if (!infoExists)
			{
				logger.LogWarning("FileId [{Id}] FileInfo Not Exists", fileId);
				return null;
			}

			//读出配置Json
			var infoJson = await File.ReadAllTextAsync(infoPath);
			return JsonSerializer.Deserialize(infoJson, NativeLocalSerializerContext.Default.NativeFileInfo);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "FileId [{FileId}] FileInfo Read Error", fileId);
			return null;
		}
		finally
		{
			slim.Release();
		}
	}

	private async Task<bool> WriteInfoAsync(NativeFileInfo info)
	{
		var fileId = info.FileId;

		var slim = _semaphoreSlims.GetOrAdd(fileId, _ => new SemaphoreSlim(1, 1));
		await slim.WaitAsync();

		try
		{
			var infoPath = ConfigFilePath(fileId);
			var infoJson = JsonSerializer.Serialize(info, NativeLocalSerializerContext.Default.NativeFileInfo);

			//写入配置
			await File.WriteAllTextAsync(infoPath, infoJson);
			return true;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "FileId [{FileId}] FileInfo Write Error ", fileId);
			return false;
		}
		finally
		{
			slim.Release();
		}
	}

	public async IAsyncEnumerable<NativeFileInfo> GetInfosAsync()
	{
		//获取当前文件夹下所有的配置文件path
		var filePaths = Directory.GetFiles(ConfigDirectoryPath, $"*.{ConfigFileSuffix}", SearchOption.AllDirectories);
		foreach (var fielPath in filePaths)
		{
			NativeFileInfo? info = null;
			try
			{
				var infoJosn = await File.ReadAllTextAsync(fielPath);
				info = JsonSerializer.Deserialize(infoJosn, NativeLocalSerializerContext.Default.NativeFileInfo);
			}
			catch (Exception)
			{

			}

			if (info is not null)
			{
				yield return info;
			}
		}
	}

	public Task<bool> RemoveInfoAsync(NativeFileInfo info)
	{
		var fileId = info.FileId;

		try
		{
			var infoPath = ConfigFilePath(fileId);
			if (!File.Exists(infoPath))
				return Task.FromResult(true);

			File.Delete(infoPath);

			return Task.FromResult(!File.Exists(infoPath));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "FileId [{FileId}] FileInfo Remove Error", fileId);
			return Task.FromResult(false);
		}
	}

	private string ConfigFilePath(string fileId)
	{
		var temp = fileId.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries);
		var sanitized = string.Join("_", temp);
		var filename = sanitized + "." + ConfigFileSuffix;
		var filePath = Path.Combine(ConfigDirectoryPath, filename);
		return filePath;
	}
}
