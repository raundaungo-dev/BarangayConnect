namespace BarangayConnect.Models;

public class Announcement
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime PublishedOn { get; set; }
    public string Audience { get; set; } = string.Empty;
}
