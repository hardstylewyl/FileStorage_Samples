using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileStorage.EntityFramework;

public sealed class FileMetadataEntityTypeConfiguration : IEntityTypeConfiguration<FileMetadataEntity>
{
	public void Configure(EntityTypeBuilder<FileMetadataEntity> builder)
	{
		builder.ToTable("FileMetadata");
		builder.HasKey(x => x.Id);

		builder.Property(x => x.FileName)
			.HasMaxLength(255)
			.IsRequired();

		builder.HasIndex(x => x.UserId);
		builder.HasIndex(x => x.FileId).IsUnique();
	}
}
