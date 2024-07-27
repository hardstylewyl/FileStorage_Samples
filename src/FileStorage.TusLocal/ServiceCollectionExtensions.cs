using Microsoft.Extensions.DependencyInjection;
using SolidTUS.Extensions;
using SolidTUS.Models;

namespace FileStorage.TusLocal;

public static class ServiceCollectionExtensions
{


	public static IServiceCollection AddTusLocalStorage(this IServiceCollection services, Action<TusLocalOptions> optionsSetup)
	{
		var options = new TusLocalOptions();
		optionsSetup.Invoke(options);
		return services.AddTusLocalStorage(options);
	}


	public static IServiceCollection AddTusLocalStorage(this IServiceCollection services, TusLocalOptions options)
	{
		services.AddScoped<TusFileService>();

		services.AddTus(o =>
			{
				//提供发现
				o.HasTermination = true;
				//使用滑动过期策略 10分钟
				o.ExpirationStrategy = ExpirationStrategy.SlideAfterAbsoluteExpiration;
				//设置每10分钟清理一次（不完整的文件碎片缓存）
				o.ExpirationJobRunnerInterval = TimeSpan.FromMinutes(10);
			})
			//配置验证器(元数据必须包含的字段)
			.SetMetadataValidator(m =>
				m.ContainsKey("filename") &&
				m.ContainsKey("contentType") &&
				m.ContainsKey("fileId"))
			//启用过期处理后台服务
			.WithExpirationJobRunner()
			//配置存储位置
			.FileStorageConfiguration(o =>
			{
				o.DirectoryPath = options.DirectoryPath;
				o.MetaDirectoryPath = options.MetaDirectoryPath;
			});

		return services;
	}
}
