using FileStorage.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Quartz;

namespace FileStorage.MinioRemote;

public static class RemoteStoreBuilderExtensions
{
	public static void AddMinioRemoteStorage(this RemoteStoreBuilder builder, Action<MinioRemoteOptions> optionsSetup)
	{
		var services = builder.Services;
		var options = new MinioRemoteOptions();
		optionsSetup?.Invoke(options);
		services.Configure(optionsSetup!);
		builder.AddMinioRemoteStorageCore(options);
	}

	public static void AddMinioRemoteStorage(this RemoteStoreBuilder builder, IConfiguration configuration)
	{
		var services = builder.Services;
		services.Configure<MinioRemoteOptions>(configuration);
		builder.AddMinioRemoteStorageCore(configuration.Get<MinioRemoteOptions>()!);
	}

	public static void AddMinioRemoteStorage(this RemoteStoreBuilder builder, IConfiguration configuration, string name)
	{
		var services = builder.Services;
		var section = configuration.GetRequiredSection(name);
		services.Configure<MinioRemoteOptions>(section);
		builder.AddMinioRemoteStorageCore(section.Get<MinioRemoteOptions>()!);
	}

	internal static void AddMinioRemoteStorageCore(this RemoteStoreBuilder builder, MinioRemoteOptions options)
	{
		var services = builder.Services;
		//minio文件服务（包装MinioClient）
		services.AddSingleton(sp =>
		{
			var s = new MinioFileService(
				 sp.GetRequiredService<ILogger<MinioFileService>>(),
				  sp.GetRequiredService<IOptions<MinioRemoteOptions>>());

			s.WithCredentials(options.AccessKey, options.SecretKey)
		   .WithEndpoint(options.Endpoint)
		   .WithSSL(options.EnableSSL)
		   .Build();

			return s;
		});

		//同步服务（用于监测上传进度和结果）使用IRedisCache
		services.AddSingleton<MinioSyncStateCache>();

		//定时任务
		services.AddQuartz(o =>
		{
			o.UseSimpleTypeLoader();
			o.UseMicrosoftDependencyInjectionJobFactory();
		});
		services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
	}


}
