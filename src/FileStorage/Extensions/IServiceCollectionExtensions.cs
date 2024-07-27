using System.Text.Json;
using FileStorage.Caching;
using FileStorage.Core;
using FreeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileStorage.Extensions;

public static class IServiceCollectionExtensions
{
	public static FileStoreageBuilder AddFileStoreage(this IServiceCollection services)
	{
		return new FileStoreageBuilder() { Services = services };

	}


	public static IServiceCollection AddRedisCache(this IServiceCollection services, FreeRedisCacheOptions options)
	{

		services.AddSingleton<IRedisClient>(sp =>
		{
			var cli = new RedisClient(options.Configuration);
			cli.Serialize += o => JsonSerializer.Serialize(o);
			cli.Deserialize += (json, type) => JsonSerializer.Deserialize(json, type);

			//不开启日志
			if (!options.EnableCallCommandLog)
			{
				return cli;
			}

			var logger = sp.GetRequiredService<ILogger<RedisClient>>();
			cli.Notice += (s, e)
					=> logger.Log(options.LogLevel, "Exec Redis Command:{cmd}", e.Log);

			return cli;
		});


		return services;
	}




}
