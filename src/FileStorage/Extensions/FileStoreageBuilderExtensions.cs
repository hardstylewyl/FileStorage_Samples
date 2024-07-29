using FileStorage.Core;

namespace FileStorage.Extensions;

public static class FileStoreageBuilderExtensions
{
	public static FileStoreageBuilder AddCore(this FileStoreageBuilder builder, Action<FileStoreageCoreBuilder> setupAction)
	{
		var optionsBuilder = new FileStoreageCoreBuilder(builder);
		setupAction.Invoke(optionsBuilder);
		return builder;
	}


	public static FileStoreageBuilder AddRemoteStore(this FileStoreageBuilder builder, Action<RemoteStoreBuilder> setupAction)
	{
		var optionsBuilder = new RemoteStoreBuilder(builder);
		setupAction.Invoke(optionsBuilder);
		return builder;
	}

	public static FileStoreageBuilder AddLocalStore(this FileStoreageBuilder builder, Action<LocalStoreBuilder> setupAction)
	{
		var optionsBuilder = new LocalStoreBuilder(builder);
		setupAction.Invoke(optionsBuilder);
		return builder;
	}

}
