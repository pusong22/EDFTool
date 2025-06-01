using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EDFToolApp.EFDbContext;
public class MemoryDbContext : IDbContextFactory
{
    private readonly SqliteConnection _connection;

    internal MemoryDbContext()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public FileDbContext CreateDbContext()
    {
        DbContextOptions options = new DbContextOptionsBuilder().UseSqlite(_connection).Options;

        return new FileDbContext(options);
    }
}
