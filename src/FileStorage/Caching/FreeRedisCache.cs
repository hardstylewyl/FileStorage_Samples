
using FreeRedis;

namespace FileStorage.Caching;

public sealed class FreeRedisCache(
	ConnectionStringBuilder connectionString,
	params ConnectionStringBuilder[] slaveConnectionStrings)
	: RedisClient(connectionString, slaveConnectionStrings), IRedisCache
{
	public async Task AddAsync<T>(string key, T item, TimeSpan timeSpan)
	{
		await SetAsync(key, item);
		await ExpireAsync(key, timeSpan.Seconds);
	}

	public async Task<T?> GetOrCreateAsync<T>(string key, T item, TimeSpan timeSpan)
	{
		var v = await GetAsync<T>(key);
		if (v == null)
		{
			await AddAsync(key, item, timeSpan);
		}

		return v;
	}

	public async Task RemoveAsync(string key)
	{
		await DelAsync(key);
	}
}
