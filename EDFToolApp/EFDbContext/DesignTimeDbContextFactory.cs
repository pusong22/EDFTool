using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EDFToolApp.EFDbContext;
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FileDbContext>
{
    public FileDbContext CreateDbContext(string[] args)
    {
        DbContextOptions options = new DbContextOptionsBuilder().UseSqlite("Data Source=C:\\Users\\pusong\\source\\code\\file.db").Options;

        return new FileDbContext(options);
    }
}
