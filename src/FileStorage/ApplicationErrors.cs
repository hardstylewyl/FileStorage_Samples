using FileStorage.Models;

namespace FileStorage;

public static class ApplicationErrors
{
	public static readonly Error InternalServerError = new("InternalServerError", "服务器异常请联系管理员");
}
