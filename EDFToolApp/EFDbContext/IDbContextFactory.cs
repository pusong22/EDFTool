namespace EDFToolApp.EFDbContext;

public interface IDbContextFactory
{
    FileDbContext CreateDbContext();
}
