namespace FileStorage.Caching;

public interface ICache
{
	Task<T?> GetOrCreateAsync<T>(string key, T item, TimeSpan timeSpan);

	Task AddAsync<T>(string key, T item, TimeSpan timeSpan);

	Task RemoveAsync(string key);
}
