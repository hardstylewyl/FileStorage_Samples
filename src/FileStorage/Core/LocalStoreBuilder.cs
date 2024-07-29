using Microsoft.Extensions.DependencyInjection;

namespace FileStorage.Core;

public sealed class LocalStoreBuilder(FileStoreageBuilder InnerBuilder)
{
	public IServiceCollection Services => InnerBuilder.Services;


}
