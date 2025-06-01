using Microsoft.EntityFrameworkCore;
using Model;

namespace EDFToolApp.EFDbContext;
public class FileDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<RecentFileModel> RecentFiles { get; set; }
}
