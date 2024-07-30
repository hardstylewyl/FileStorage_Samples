using FileStorage.Api.Contracts.Requests;
using FileStorage.Models;
using FileStorage.NativeLocal;
using FileStorage.NativeLocal.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileStorage.Api.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class NativeController(
	ILogger<NativeController> logger,
	NativeFileService nativeFileService,
	UploadToMinioJobService minioJobService)
	: ControllerBase
{
	[HttpPost]
	public async Task<Result> FragmentUploadCreate(FragmentUploadCreateRequest request)
	{
		var (success, error) = await nativeFileService.FragmentUploadCreateAsync(request);
		return success ? Result.Success() : Result.Failure(error!);
	}

	[HttpPost]
	public async Task<Result> FragmentUpload(FragmentUploadRequest request)
	{
		var context = new FragmentUploadContextBuilder(request.FileId, request.File, request.ChunkSeq)
			// .EnableCheckMd5(request.Md5)
			.OnFragmentUploadCompleted(minioJobService.StartScheduleJob)
			.Build();

		var (success, error) = await nativeFileService.FragmentUploadAsync(context);
		return success ? Result.Success() : Result.Failure(error!);
	}
}
