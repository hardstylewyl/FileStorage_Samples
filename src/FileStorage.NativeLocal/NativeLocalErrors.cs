using FileStorage.Models;

namespace FileStorage.NativeLocal;

public static class NativeLocalErrors
{
	public static readonly Error FileUploadInfoNotFound =
	   new("NativeLocal.FileUploadInfoNotFound", "文件上传信息未找到");

	public static readonly Error FileUploadInfoAlreadyExist =
		new("NativeLocal.FileUploadInfoAlreadyExist", "文件上传信息已存在");

	public static readonly Error FileUploadInfoCreateFailed =
		new("NativeLocal.FileUploadInfoCreateFailed", "文件上传信息创建失败");

	public static readonly Error Md5CheckFailed =
		new("NativeLocal.Md5CheckFailed", "文件MD5校验失败，可能上传过程出错");

	public static readonly Error WriteFileFailed =
		new("NativeLocal.WriteFileFailed", "写文件失败，清及时联系管理员");
}
