using FileStorage.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FileStorage.EntityFramework;

public sealed class FileMetadataManager<TContext>(TContext dbContext) : IFileMetadataManager
	 where TContext : DbContext
{
	private readonly DbSet<FileMetadataEntity> Store = dbContext.Set<FileMetadataEntity>();

	public async Task<FileMetadataEntity> CreateAsync(FileMetadataEntity entity, bool saveChanges = true)
	{
		var entry = await Store.AddAsync(entity);
		if (saveChanges)
		{
			await dbContext.SaveChangesAsync();
		}

		return entry.Entity;
	}

	public Task<FileMetadataEntity?> FindAsync(string md5, long size)
	{
		return FindAsync(FileMetadataEntity.BuildFileId(md5, size));
	}
	public Task<FileMetadataEntity?> FindAsync(string fileId)
	{
		return Store
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.FileId == fileId);
	}

	public Task<List<FileMetadataEntity>> GetAllAsync()
	{
		return Store.
			AsNoTracking()
			.ToListAsync();
	}

	public Task<List<FileMetadataEntity>> PagingGetAsync(int pageNum, int pageSize)
	{
		return Store
			.AsNoTracking()
			.Paged(pageNum, pageSize)
			.ToListAsync();
	}

	public Task<List<FileMetadataEntity>> PagingGetByUserIdAsync(string userId, int pageNum, int pageSize)
	{
		return Store
			.AsNoTracking()
			.Where(x => x.UserId == userId)
			.Paged(pageNum, pageSize)
			.ToListAsync();
	}

	public async Task<FileMetadataEntity> RemoveAsync(FileMetadataEntity entity, bool saveChanges = true)
	{
		var entry = Store.Remove(entity);
		if (saveChanges)
		{
			await dbContext.SaveChangesAsync();
		}
		return entry.Entity;
	}
}
