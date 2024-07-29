using System.Text.Json.Serialization;
using FileStorage.Core;
using FileStorage.NativeLocal.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileStorage.NativeLocal;

public static class LocalStoreBuilderExtensions
{
	public static void AddNativeLocalStorage(this LocalStoreBuilder builder, Action<NativeLocalOptions> setupAction)
	{
		builder.Services.Configure(setupAction);
		AddNativeLocalStorageCore(builder);
	}

	public static void AddNativeLocalStorage(this LocalStoreBuilder builder, IConfiguration configuration)
	{
		builder.Services.Configure<NativeLocalOptions>(configuration);
		AddNativeLocalStorageCore(builder);
	}

	public static void AddNativeLocalStorage(this LocalStoreBuilder builder, IConfiguration configuration, string name)
	{
		builder.Services.Configure<NativeLocalOptions>(configuration.GetSection(name));
		AddNativeLocalStorageCore(builder);
	}

	internal static void AddNativeLocalStorageCore(this LocalStoreBuilder builder)
	{
		var services = builder.Services;
		services.AddSingleton<NativeFileStore>();
		services.AddSingleton<NativeFileConfigStore>();
		services.AddSingleton<NativeFileService>();
	}
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(NativeFileInfo))]
public partial class NativeLocalSerializerContext : JsonSerializerContext;
