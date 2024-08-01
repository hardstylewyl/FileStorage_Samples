using FileStorage.Caching;
using FileStorage.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileStorage.TusLocal.UrlStorage;

public static class TusUrlStorageApi
{
	private const string RedisKey = "tus_urlstorage";

	public static IEndpointRouteBuilder MapTusStorageForRedis(this IEndpointRouteBuilder app, string prefix = "TusStorage")
	{
		var api = app.MapGroup(prefix);
		api.MapGet("/FindAllUploads", FindAllUploads);
		api.MapGet("/FindUploadsByFileId", FindUploadsByFileId);
		api.MapGet("/RemoveUpload", RemoveUpload);
		api.MapPost("/AddUpload", AddUpload);
		return app;
	}

	public static async Task<Result<List<PreviousUpload>>> FindAllUploads([FromServices] IRedisCache redisCache)
	{
		var result = await redisCache
			.HGetAllAsync<PreviousUpload>(RedisKey);

		return
			Result.Success(result.Count != 0 ? result.Values.ToList() : []);
	}

	public static async Task<Result<PreviousUpload>> FindUploadsByFileId([FromServices] IRedisCache redisCache,
		string fileId)
	{
		return Result.Success(await redisCache
			.HGetAsync<PreviousUpload>(RedisKey, fileId));
	}

	public static async Task<Result> RemoveUpload([FromServices] IRedisCache redisCache,
		string fileId)
	{
		return Result.Success(await redisCache
			.HDelAsync(RedisKey, fileId));
	}

	public static async Task<Result> AddUpload([FromServices] IRedisCache redisCache,
		[FromBody] TusAddUploadRequest request)
	{
		return Result.Success(await redisCache
			.HSetAsync(RedisKey, request.FileId, request.Value));
	}
}
