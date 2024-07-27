
using Microsoft.Extensions.Logging;

namespace FileStorage.Caching;

public sealed class FreeRedisCacheOptions
{
	// Redis链接字符串
	public string Configuration { get; set; } = "localhost:6379";

	// 是否打印redis命令日志
	public bool EnableCallCommandLog { get; set; } = false;

	// 命令日志等级, 启用了打印日志才有效
	public LogLevel LogLevel { get; set; } = LogLevel.Trace;
}
