using FileStorage.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FileStorage.TusLocal.UrlStorage;

public static class TusUrlStorageApi
{

	private const string RedisKey = "tus_urlstorage";

	public static IEndpointRouteBuilder MapTusStorageForRedis(this IEndpointRouteBuilder app, string prefix = "TusStorage")
	{
		app.MapGet(prefix + "/FindAllUploads", FindAllUploads);
		app.MapGet(prefix + "/FindUploadsByFileId", FindUploadsByFileId);
		app.MapGet(prefix + "/RemoveUpload", RemoveUpload);
		app.MapPost(prefix + "/AddUpload", AddUpload);
		return app;
	}


	public static async Task<List<PreviousUpload>> FindAllUploads([AsParameters] FreeRedisCache freeRedisCache)
	{
		var result = await freeRedisCache
			.RedisClient
			.HGetAllAsync<PreviousUpload>(RedisKey);

		return
			result.Count != 0 ? [.. result.Values] : [];
	}

	public static async Task<PreviousUpload> FindUploadsByFileId([AsParameters] FreeRedisCache freeRedisCache,
		string fileId)
	{
		return await freeRedisCache.
			RedisClient
			.HGetAsync<PreviousUpload>(RedisKey, fileId);
	}


	public static async Task<long> RemoveUpload([AsParameters] FreeRedisCache freeRedisCache,
		string fileId)
	{
		return await freeRedisCache.
			RedisClient
			.HDelAsync(RedisKey, fileId);
	}

	public static async Task<long> AddUpload([AsParameters] FreeRedisCache freeRedisCache,
		[AsParameters] TusAddUploadRequest request)
	{
		return await freeRedisCache
			.RedisClient
			.HSetAsync(RedisKey, request.FileId, request.Value);
	}
}
