using EDFToolApp.EFDbContext;
using Microsoft.EntityFrameworkCore;
using Model;

namespace EDFToolApp.Service;
public class FileDbService(IDbContextFactory dbFactory)
{
    public async Task<RecentFileModel> Create(RecentFileModel item)
    {
        using var context = dbFactory.CreateDbContext();
        var createdResult = await context.Set<RecentFileModel>().AddAsync(item);
        await context.SaveChangesAsync();
        return createdResult.Entity;
    }


    public async Task<RecentFileModel> Update(RecentFileModel item)
    {
        using var context = dbFactory.CreateDbContext();
        var createdResult = context.Set<RecentFileModel>().Update(item);
        await context.SaveChangesAsync();
        return createdResult.Entity;
    }

    public async Task<RecentFileModel?> Get(string path)
    {
        using var context = dbFactory.CreateDbContext();
        return await context.Set<RecentFileModel>().Where(i => path.Equals(i.FilePath)).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RecentFileModel>> GetAll()
    {
        using var context = dbFactory.CreateDbContext();
        return await context.Set<RecentFileModel>().ToListAsync();
    }
}
