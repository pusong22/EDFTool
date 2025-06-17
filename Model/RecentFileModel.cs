namespace Model;

public class RecentFileModel
{
    public int Id { get; set; }
    public DateTime AccessedTime { get; set; }
    public string? FilePath { get; set; }
}
