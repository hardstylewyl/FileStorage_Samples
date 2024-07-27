using FileStorage.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FileStorage.Extensions;

public static class DbContextOptionsBuilderExtensions
{
	public static void UseFileStorage(this DbContextOptionsBuilder builder)
	{
		builder.ReplaceService<IModelCustomizer, FileStorageEntityFrameworkCustomizer>();
	}
}
