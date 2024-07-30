using System.Text.Json;
using FileStorage.Caching;
using FileStorage.EntityFramework;
using FreeRedis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileStorage.Core;

public sealed class FileStoreageCoreBuilder(FileStoreageBuilder InnerBuilder)
{
	public IServiceCollection Services => InnerBuilder.Services;

	public FileStoreageCoreBuilder UseDbContext<TDbContext>()
		where TDbContext : DbContext
	{
		InnerBuilder.Services.AddScoped<IFileMetadataManager, FileMetadataManager<TDbContext>>();
		return this;
	}

	public FileStoreageCoreBuilder UseDbContext(Type dbContextType)
	{
		ArgumentNullException.ThrowIfNull(dbContextType);
		if (!typeof(DbContext).IsAssignableFrom(dbContextType))
		{
			throw new ArgumentException("{type} is not DbContext ", nameof(dbContextType));
		}

		InnerBuilder.Services.AddScoped(
		typeof(IFileMetadataManager),
		 typeof(FileMetadataManager<>).MakeGenericType([dbContextType]));

		return this;
	}

	public FileStoreageCoreBuilder UseMemoryCache()
	{
		InnerBuilder.Services.AddMemoryCache();
		InnerBuilder.Services.AddSingleton<ICache, DefaultMemoryCache>();
		return this;
	}

	public FileStoreageCoreBuilder UseFreeRedisCache(Action<FreeRedisCacheOptions>? optionsSetup)
	{
		var options = new FreeRedisCacheOptions();
		optionsSetup?.Invoke(options);
		return UseFreeRedisCache(options);
	}

	public FileStoreageCoreBuilder UseFreeRedisCache(string configuration)
	{
		var options = new FreeRedisCacheOptions() { Configuration = configuration };
		return UseFreeRedisCache(options);
	}

	public FileStoreageCoreBuilder UseFreeRedisCache(FreeRedisCacheOptions options)
	{
		InnerBuilder.Services.AddSingleton<IRedisCache>(sp =>
		{
			//构建redisClient
			var cli = new FreeRedisCache(options.Configuration);
			cli.Serialize += o => JsonSerializer.Serialize(o);
			cli.Deserialize += (json, type) => JsonSerializer.Deserialize(json, type);

			//不开启日志
			if (options.EnableCallCommandLog)
			{
				var logger = sp.GetRequiredService<ILogger<RedisClient>>();
				cli.Notice += (s, e)
						=> logger.Log(options.LogLevel, "Exec Redis Command:{cmd}", e.Log);
			}

			return cli;
		});

		InnerBuilder.Services.AddSingleton<ICache>(sp => sp.GetRequiredService<FreeRedisCache>());

		return this;
	}
}
