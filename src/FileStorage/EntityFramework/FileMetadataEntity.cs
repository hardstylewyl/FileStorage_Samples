namespace FileStorage.EntityFramework;

public sealed class FileMetadataEntity
{

#nullable disable
	private FileMetadataEntity()
	{
	}

	public FileMetadataEntity(string fileName, string userId, string bucketName, string objectName, string fileUrl,
		long size, string extension, string md5)
	{
		FileName = fileName;
		UserId = userId;
		BucketName = bucketName;
		ObjectName = objectName;
		FileUrl = fileUrl;
		Size = size;
		Extension = extension;
		Md5 = md5;
		Key = BuildKey(md5, size);
		UploadTimeOnUtc = DateTime.UtcNow;
	}

	public long Id { get; set; }

	/// <summary>
	///     用于索引的key {md5}_{size}
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	///     文件名 如a.png
	/// </summary>
	public string FileName { get; set; }

	/// <summary>
	///     上传用户Id
	/// </summary>
	public string UserId { get; set; }

	/// <summary>
	///     Minio的存储桶名称
	/// </summary>
	public string BucketName { get; set; }

	/// <summary>
	///     Minio对象名称 BucketName/ObjectName 组合必须是唯一的
	/// </summary>
	public string ObjectName { get; set; }

	/// <summary>
	///     文件的minio访问地址
	/// </summary>
	public string FileUrl { get; set; }

	/// <summary>
	///     文件大小
	/// </summary>
	public long Size { get; set; }

	/// <summary>
	///     文件的扩展名称 如 .jpg
	/// </summary>
	public string Extension { get; set; }

	/// <summary>
	///     文件的md5
	/// </summary>
	public string Md5 { get; set; }

	/// <summary>
	///     上传时间
	/// </summary>
	public DateTime UploadTimeOnUtc { get; set; }


	public static string BuildKey(string md5, long size)
	{
		return $"{md5}_{size}";
	}
}
