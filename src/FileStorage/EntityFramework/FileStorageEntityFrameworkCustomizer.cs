using FileStorage.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FileStorage.EntityFramework;

public sealed class FileStorageEntityFrameworkCustomizer : RelationalModelCustomizer
{
	public FileStorageEntityFrameworkCustomizer(ModelCustomizerDependencies dependencies)
		: base(dependencies)
	{
	}

	public override void Customize(ModelBuilder modelBuilder, DbContext context)
	{
		if (modelBuilder == null)
			throw new ArgumentNullException(nameof(modelBuilder));
		if (context == null)
			throw new ArgumentNullException(nameof(context));

		modelBuilder.UseFileStorage();
		base.Customize(modelBuilder, context);
	}
}
