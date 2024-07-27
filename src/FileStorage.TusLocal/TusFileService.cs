using SolidTUS.Handlers;

namespace FileStorage.TusLocal;

public sealed class TusFileService(IUploadMetaHandler uploadMetaHandler)
{
	public async Task<TusCheckFileResult> CheckAsync(string fileId)
	{
		var info = await uploadMetaHandler.GetResourceAsync(fileId, default);

		if (info == null)
		{
			return new TusCheckFileResult(true, false);
		}

		return info.Done
			? new TusCheckFileResult(false, false)
			: new TusCheckFileResult(false, true);
	}
}
