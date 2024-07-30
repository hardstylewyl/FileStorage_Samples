using FreeRedis;

namespace FileStorage.Caching;

public interface IRedisCache : ICache, IRedisClient
{
}
