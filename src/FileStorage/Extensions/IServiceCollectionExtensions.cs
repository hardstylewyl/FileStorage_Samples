using FileStorage.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FileStorage.Extensions;

public static class IServiceCollectionExtensions
{
	public static FileStoreageBuilder AddFileStoreage(this IServiceCollection services)
	{
		return new FileStoreageBuilder() { Services = services };

	}
}
