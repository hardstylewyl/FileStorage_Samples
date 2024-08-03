using FileStorage.Api.Contracts.Requests;
using FileStorage.Api.Contracts.Responses;
using FileStorage.EntityFramework;
using FileStorage.Extensions;
using FileStorage.MinioRemote;
using FileStorage.Models;
using FileStorage.NativeLocal;
using FileStorage.TusLocal;
using Microsoft.AspNetCore.Mvc;

namespace FileStorage.Api.Controllers;

[ApiController]
public sealed class FileBaseController(
	IFileMetadataManager metaManager,
	MinioSyncStateCache syncStateCache,
	MinioFileService minioFileService,
	NativeFileService nativeFileService,
	TusFileService tusFileService) : ControllerBase
{
	//单文件最大size 5mb
	private const long MaxSize = 5242880L;

	[HttpPost("/CheckFile")]
	public async Task<Result<CheckFileResponse>> CheckFile(CheckFileRequest request)
	{
		var fileId = request.FileId;

		//查询同步状态信息
		var syncState = await syncStateCache.FindAsync(fileId);
		if (syncState != null)
		{
			return syncState.SyncError
				? new CheckFileResponse() { Status = FileUploadStatus.SyncFailed }
				: new CheckFileResponse() { Status = FileUploadStatus.InSynchronizing };
		}

		//查询持久化元数据
		var meta = await metaManager.FindAsync(fileId);
		if (meta != null)
		{
			return new CheckFileResponse()
			{
				Status = FileUploadStatus.Completed,
				FileUrl = meta.FileUrl
			};
		}

		//以下switch的其他情况出现的原因
		//说明文件已经完整存储在本地，但是数据库中没有存储。
		//检查是否上传至远程存储
		//已在远程存储存在，则添加数据库即可，如果不存在还需要进行上传

		//tus
		if (request.IsTus)
		{
			var result = await tusFileService.CheckAsync(request.FileId);
			return result switch
			{
				{ NotExist: true } => new CheckFileResponse() { Status = FileUploadStatus.NotExist },
				{ InProgress: true } => new CheckFileResponse() { Status = FileUploadStatus.InProgress },
				_ => Result.Failure<CheckFileResponse>(ApplicationErrors.InternalServerError)
			};
		}
		//native，处于InProgress状态会返回缺失的分片号
		else
		{
			var result = await nativeFileService.CheckAsync(fileId);
			return result switch
			{
				{ NotExist: true } => new CheckFileResponse() { Status = FileUploadStatus.NotExist },
				{ InProgress: true } => new CheckFileResponse() { Status = FileUploadStatus.InProgress, MissChunkList = result.MissChunkList },
				_ => Result.Failure<CheckFileResponse>(ApplicationErrors.InternalServerError)
			};
		}
	}

	[HttpPost("/DirectUpload")]
	[RequestSizeLimit(MaxSize)]
	public async Task<Result<DirectUploadResponse>> DirectUpload(DirectUploadRequest request, CancellationToken cancellationToken)
	{
		var file = request.File;

		//超过限制大小
		if (file.Length > MaxSize)
		{
			return Result.Failure<DirectUploadResponse>(ApplicationErrors.FileTooLarge);
		}

		var inputStream = file.OpenReadStream();

		//检查md5
		var md5Valid = await inputStream.CheckMd5Async(request.Md5);
		if (!md5Valid)
		{
			return Result.Failure<DirectUploadResponse>(ApplicationErrors.Md5CheckFailed);
		}

		//重置文件流位置
		inputStream.Seek(0, SeekOrigin.Begin);
		//上传至minio
		var uploadResult = await minioFileService.UploadAsync(
			   inputStream,
			   request.FileId,
			   request.Extension,
			   cancellationToken);
		if (!uploadResult.IsSuccess)
		{
			return Result.Failure<DirectUploadResponse>(ApplicationErrors.InternalServerError);
		}

		var userId = "uid0";
		//上传成功持久化元数据到数据库
		await metaManager.CreateAsync(new FileMetadataEntity(
			request.Filename,
			  userId,
			  uploadResult.BucketName,
			  uploadResult.ObjectName,
			  uploadResult.FileUrl,
			  file.Length,
			  request.Extension,
			  request.Md5
			));

		return new DirectUploadResponse { FileUrl = uploadResult.FileUrl };
	}
}
