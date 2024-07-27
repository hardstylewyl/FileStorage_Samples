using System.Security.Cryptography;
using FileStorage.NativeLocal.Models;
using Microsoft.Extensions.Logging;

namespace FileStorage.NativeLocal;

public sealed class NativeFileService(
	ILogger<NativeFileService> logger,
	NativeFileStore fileStore,
	NativeFileConfigStore configStore)
{
	//检查文件状态
	public async Task<NativeCheckFileResult> CheckAsync(string fileId)
	{
		var info = await configStore.GetInfoAsync(fileId);

		return info == null
			? new NativeCheckFileResult(true, null)
			: new NativeCheckFileResult(false, info.GetMissingChunks().ToList());
	}

	//分片配置创建
	public async Task<bool> FragmentUploadCreateAsync(FragmentUploadCreateArgs args)
	{
		var info = await configStore.GetInfoAsync(args.FileId);
		if (info != null)
		{
			return false;
		}

		info = new NativeFileInfo(
			 args.FileId,
			args.Filename,
			args.ChunkCount,
			args.ChunkSize,
			//文件名使用文件id+扩展名
			args.FileId + args.Extension);

		return await configStore.CreateOrUpdateInfoAsync(info);
	}

	//分片数据上传
	public async Task<bool> FragmentUploadAsync(FragmentUploadContext context)
	{
		var info = await configStore.GetInfoAsync(context.FileId);
		if (info is null)
		{
			return false;
		}

		//开启远程流
		var inputStream = context.FormFile.OpenReadStream();

		//如果开启了md5检查
		if (context.IsCheckMd5)
		{
			var md5Valid = await CheckMd5Async(inputStream, context.Md5);
			if (!md5Valid)
			{
				logger.LogWarning("FileId [{id}] ChunkSeq [{seq}] Md5 Check Fail", info.FileId, context.ChunkSeq);
				return false;
			}

			//md5校验完成需要重试设置文件流位置为初始位置
			inputStream.Seek(0, SeekOrigin.Begin);
		}


		//写入分片数据，写入成功会更新info中的指定标记位
		var writeSuccess = await fileStore.WriteFragmentDataAsync(info, context.ChunkSeq, inputStream);
		if (!writeSuccess)
		{
			return false;
		}

		//释放流
		await inputStream.DisposeAsync();

		//更新配置
		await configStore.CreateOrUpdateInfoAsync(info);


		//可选文件上传完成回调
		var completedFunc = context.OnCompletedCallbackFunc;
		if (info.IsCompleted() && completedFunc != null)
		{
			await completedFunc(info);
		}

		return true;
	}

	//检查分片流的md5是否正确
	public async static Task<bool> CheckMd5Async(Stream stream, string md5Value)
	{
		byte[] hashBytes;
		using (var md5 = MD5.Create())
		{
			hashBytes = await md5.ComputeHashAsync(stream);
		}

		var md5Hash = BitConverter.ToString(hashBytes).Replace("-", "");

		return string.Equals(md5Hash, md5Value, StringComparison.OrdinalIgnoreCase);
	}

}
