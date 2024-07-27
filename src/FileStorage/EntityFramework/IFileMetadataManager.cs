namespace FileStorage.EntityFramework;

public interface IFileMetadataManager
{
	Task<FileMetadataEntity> CreateAsync(FileMetadataEntity entity, bool saveChanges = true);
	Task<FileMetadataEntity?> FindAsync(string md5, long size);
	Task<FileMetadataEntity?> FindAsync(string key);
	Task<List<FileMetadataEntity>> GetAllAsync();
	Task<List<FileMetadataEntity>> PagingGetAsync(int pageNum, int pageSize);
	Task<FileMetadataEntity> RemoveAsync(FileMetadataEntity entity, bool saveChanges = true);
}
