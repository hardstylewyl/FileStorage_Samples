using Microsoft.EntityFrameworkCore;

namespace FileStorage.Api;

public sealed class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}
}
