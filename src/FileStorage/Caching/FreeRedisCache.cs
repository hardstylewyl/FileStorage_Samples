
using FreeRedis;

namespace FileStorage.Caching;

public sealed class FreeRedisCache(IRedisClient redisClient) : ICache
{
	public IRedisClient RedisClient => redisClient;

	public async Task AddAsync<T>(string key, T item, TimeSpan timeSpan)
	{
		await redisClient.SetAsync(key, item);
		await redisClient.ExpireAsync(key, timeSpan.Seconds);
	}

	public async Task<T?> GetAsync<T>(string key)
	{
		return await redisClient.GetAsync<T>(key);
	}

	public async Task<T?> GetOrCreateAsync<T>(string key, T item, TimeSpan timeSpan)
	{
		var v = await redisClient.GetAsync<T>(key);
		if (v == null)
		{
			await AddAsync(key, item, timeSpan);
		}

		return v;
	}

	public async Task RemoveAsync(string key)
	{
		await redisClient.DelAsync(key);
	}
}
