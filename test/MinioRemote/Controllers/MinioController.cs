using FileStorage.MinioRemote;
using Microsoft.AspNetCore.Mvc;

namespace MinioRemote.Controllers;


public sealed class UploadRequest
{
	public IFormFile FormFile { get; set; }
	public string FileId { get; set; }
	public string Extension { get; set; }
}


[Route("api/[controller]/[action]")]
[ApiController]
public class MinioController(MinioFileService minioFileService) : ControllerBase
{






	[HttpPost]
	public async Task<IActionResult> Upload(UploadRequest request, CancellationToken cancellation)
	{
		var resp = await minioFileService
			.UploadAsync(request.FormFile.OpenReadStream(), request.FileId, request.Extension, cancellation);
		return Ok(resp);
	}



}
