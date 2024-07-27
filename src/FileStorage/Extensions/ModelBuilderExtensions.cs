using FileStorage.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.Extensions;

internal static class ModelBuilderExtensions
{
	public static void UseFileStorage(this ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new FileMetadataEntityTypeConfiguration());
	}
}
