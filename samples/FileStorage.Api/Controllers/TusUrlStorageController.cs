using FileStorage.Caching;
using FileStorage.Models;
using FileStorage.TusLocal.UrlStorage;
using Microsoft.AspNetCore.Mvc;

namespace FileStorage.Api.Controllers;

[Route("TusStorage/[action]")]
[ApiController]
public class TusUrlStorageController(IRedisCache redisCache) : ControllerBase
{
	private const string RedisKey = "tus_urlstorage";

	[HttpGet]
	public async Task<Result<List<PreviousUpload>>> FindAllUploads()
	{
		var result = await redisCache
			.HGetAllAsync<PreviousUpload>(RedisKey);

		return
			Result.Success(result.Count != 0 ? result.Values.ToList() : []);
	}

	[HttpGet]
	public async Task<Result<PreviousUpload>> FindUploadsByFileId(string fileId)
	{
		return Result.Success(await redisCache
			.HGetAsync<PreviousUpload>(RedisKey, fileId));
	}

	[HttpGet]
	public async Task<Result> RemoveUpload(string fileId)
	{
		return Result.Success(await redisCache
			.HDelAsync(RedisKey, fileId));
	}

	[HttpPost]
	public async Task<Result> AddUpload([FromBody] TusAddUploadRequest request)
	{
		return Result.Success(await redisCache
			.HSetAsync(RedisKey, request.FileId, request.Value));
	}
}
