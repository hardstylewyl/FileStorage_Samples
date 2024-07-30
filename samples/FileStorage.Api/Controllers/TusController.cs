using Microsoft.AspNetCore.Mvc;
using SolidTUS.Attributes;
using SolidTUS.Extensions;

namespace FileStorage.Api.Controllers;

[ApiController]
public class TusController(
	ILogger<TusController> logger,
	UploadToMinioJobService minioJobService)
	: ControllerBase
{
	[TusCreation("/upload")]
	[RequestSizeLimit(5_000_000_000)]
	public async Task<IActionResult> TusCreation()
	{
		var metadata = HttpContext.TusInfo().Metadata;
		if (metadata is null)
		{
			logger.LogWarning("无法从httpContext获取TusInfo");
			throw new ArgumentNullException(nameof(metadata));
		}

		//文件id使用{md5}_{size}组成由客户端组装
		var fileId = metadata["fileId"];

		var ctx = HttpContext
			.TusCreation(fileId)
			.SetRouteName("TusUpload")
			.Build("fileId", ("name", "world"));

		//创建一个配置对象，代表已经开始跟踪这个文件的上传进度
		await ctx.StartCreationAsync(HttpContext);

		//将成功转化为创建的201响应
		return Ok();
	}

	[TusUpload("/upload/{fileId}/hello/{name}", Name = "TusUpload")]
	[RequestSizeLimit(5_000_000_000)]
	public async Task<ActionResult> TusUpload(string fileId, string name)
	{
		var ctx = HttpContext
		  .TusUpload(fileId)
		  //本地上传完成上传至minio
		  .OnUploadFinished(minioJobService.StartScheduleJob)
		  .Build();

		//追加写入文件
		await ctx.StartAppendDataAsync(HttpContext);

		//上传成功且无正文内容时必须始终返回204
		return NoContent();
	}
}
