using Microsoft.Extensions.DependencyInjection;

namespace FileStorage.Core;

public sealed class RemoteStoreBuilder(FileStoreageBuilder InnerBuilder)
{
	public IServiceCollection Services => InnerBuilder.Services;
}
