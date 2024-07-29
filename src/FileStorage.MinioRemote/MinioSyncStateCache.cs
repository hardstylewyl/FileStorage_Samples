using FileStorage.Caching;

namespace FileStorage.MinioRemote;

public sealed class MinioSyncStateCache(IRedisCache redisCache)
{
	private const string RedisKey = "minio_sync_state";

	public Task<long> AddOrUpdateAsync(MinioSyncState minioSyncState)
	{
		return redisCache.HSetAsync(RedisKey, minioSyncState.FileId, minioSyncState);
	}

	public Task StartSyncStateAsync(string fileId)
	{
		return AddOrUpdateAsync(new MinioSyncState { FileId = fileId, SyncError = false });
	}

	public Task SyncErrorAsync(string fileId, string errorMessage = "sync error", string errorTrace = "none trace")
	{
		return AddOrUpdateAsync(new MinioSyncState { FileId = fileId, SyncError = true, ErrorMessage = errorMessage, ErrorTrace = errorTrace });
	}

	public async Task<long> SyncChangeProgressAsync(string fileId, int progress)
	{
		var s = await FindAsync(fileId);
		if (s == null)
			return 0;

		s.Progress = progress;
		return await AddOrUpdateAsync(s);
	}

	public async Task<MinioSyncState?> FindAsync(string fileId)
	{
		return await redisCache.HGetAsync<MinioSyncState>(RedisKey, fileId);
	}

	public Task<bool> HasSyncAsync(string fileId)
	{
		return redisCache.HExistsAsync(RedisKey, fileId);
	}

	public Task<long> RemoveAsync(string fileId)
	{
		return redisCache.HDelAsync(RedisKey, fileId);
	}

}
