using System.Text.Json.Serialization;
using FileStorage.NativeLocal.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileStorage.NativeLocal;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNativeLocalStorage(this IServiceCollection services, Action<NativeLocalOptions> setupAction)
	{
		services.Configure(setupAction);
		return AddNativeLocalStorageCore(services);
	}

	public static IServiceCollection AddNativeLocalStorage(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<NativeLocalOptions>(configuration);
		return AddNativeLocalStorageCore(services);
	}

	public static IServiceCollection AddNativeLocalStorage(this IServiceCollection services, IConfiguration configuration, string name)
	{
		services.Configure<NativeLocalOptions>(configuration.GetSection(name));
		return AddNativeLocalStorageCore(services);
	}

	internal static IServiceCollection AddNativeLocalStorageCore(this IServiceCollection services)
	{

		services.AddSingleton<NativeFileStore>();
		services.AddSingleton<NativeFileConfigStore>();
		services.AddSingleton<NativeFileService>();
		return services;
	}
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(NativeFileInfo))]
public partial class NativeLocalSerializerContext : JsonSerializerContext;
