
using FileStorage.NativeLocal;
using FileStorage.TusLocal;
using Microsoft.Extensions.Options;

namespace FileStorage.Api;

public sealed class CheckOrCreateStoreDirectoryHostedService(
	ILogger<CheckOrCreateStoreDirectoryHostedService> logger,
	IOptions<TusLocalOptions> tusOptions,
	IOptions<NativeLocalOptions> nativeOptions)
	: IHostedService
{

	public Task StartAsync(CancellationToken cancellationToken)
	{
		var tus = tusOptions.Value;
		var native = nativeOptions.Value;
		//检查的路径
		string[] dirs = [tus.MetaDirectoryPath, tus.DirectoryPath, native.ConfigDirectoryPath, native.TempDirectoryPath];

		foreach (var dir in dirs)
		{
			if (!Directory.Exists(dir))
			{
				var dirInfo = Directory.CreateDirectory(dir);
				logger.LogInformation("创建文件夹{name}成功", dirInfo.FullName);
			}
		}

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
