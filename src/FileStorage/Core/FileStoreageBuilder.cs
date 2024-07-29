using Microsoft.Extensions.DependencyInjection;

namespace FileStorage.Core;

public sealed class FileStoreageBuilder
{
	public IServiceCollection Services { get; set; } = null!;
}
