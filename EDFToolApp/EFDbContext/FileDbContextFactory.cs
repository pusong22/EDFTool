using Microsoft.EntityFrameworkCore;

namespace EDFToolApp.EFDbContext;
public class FileDbContextFactory : IDbContextFactory
{
    private readonly string _connectionString;

    public FileDbContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public FileDbContext CreateDbContext()
    {
        DbContextOptions options = new DbContextOptionsBuilder().UseSqlite(_connectionString).Options;

        return new FileDbContext(options);
    }
}
