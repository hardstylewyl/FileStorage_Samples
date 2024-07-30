using System.Security.Cryptography;

namespace FileStorage.Extensions;

public static class StreamExtensions
{
	//检查流md5是否正确
	public static async Task<bool> CheckMd5Async(this Stream stream, string md5Value)
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
