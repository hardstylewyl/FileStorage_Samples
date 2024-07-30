using FileStorage.Models;

namespace FileStorage.Api;

public static class ApplicationErrors
{
	public static readonly Error FileTooLarge =
		new("FileTooLarge", "单文件上传文件大小超过5MB限制,请使用分片上传的方式");

	public static readonly Error InternalServerError =
		new("InternalServerError", "服务器异常请联系管理员");

	public static readonly Error Md5CheckFailed =
		new("Md5CheckFailed", "文件MD5校验失败，可能上传过程出错");
}
