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
}
