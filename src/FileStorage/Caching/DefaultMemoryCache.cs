using Microsoft.Extensions.Caching.Memory;

namespace FileStorage.Caching;

public sealed class DefaultMemoryCache(IMemoryCache memoryCache) : ICache
{
	public Task AddAsync<T>(string key, T item, TimeSpan timeSpan)
	{
		memoryCache.Set(key, item, timeSpan);
		return Task.CompletedTask;
	}

	public Task<T?> GetAsync<T>(string key)
	{
		return Task.FromResult(memoryCache.Get<T>(key));
	}

	public Task<T?> GetOrCreateAsync<T>(string key, T item, TimeSpan timeSpan)
	{
		return memoryCache.GetOrCreateAsync(key, cacheEntry =>
		{
			cacheEntry.AbsoluteExpiration = DateTimeOffset.Now.Add(timeSpan);
			return Task.FromResult(item);
		});
	}

	public Task RemoveAsync(string key)
	{
		memoryCache.Remove(key);
		return Task.CompletedTask;
	}
}
