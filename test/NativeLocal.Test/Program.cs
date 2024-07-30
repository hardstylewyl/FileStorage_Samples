using FileStorage.Extensions;
using FileStorage.NativeLocal;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging();

var tempPath = Path.Combine(Path.GetTempPath(), "NativeLocal.Test/TempDirectoryPath/");
var configPath = Path.Combine(Path.GetTempPath(), "NativeLocal.Test/ConfigDirectoryPath/");

if (!Directory.Exists(tempPath))
{
	Directory.CreateDirectory(tempPath);
}
if (!Directory.Exists(configPath))
{
	Directory.CreateDirectory(configPath);
}

services.AddFileStoreage()
	.AddLocalStore(x =>
	{
		x.AddNativeLocalStorage(o =>
		{
			o.TempDirectoryPath = tempPath;
			o.ConfigDirectoryPath = configPath;
		});
	});

var provider = services.BuildServiceProvider();

var nativeFileService = provider.GetRequiredService<NativeFileService>();

var created = await nativeFileService.FragmentUploadCreateAsync(new FileStorage.NativeLocal.Models.FragmentUploadCreateArgs()
{
	FileId = "D2B75007-CF80-4A78-A7D0-F3007D9F8CC9",
	ChunkSize = 5 * 1024 * 1024,
	ChunkCount = 10,
	Extension = ".zip",
	Filename = "a.zip"
});
