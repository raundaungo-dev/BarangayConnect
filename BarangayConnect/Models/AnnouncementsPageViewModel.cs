namespace BarangayConnect.Models;

public class AnnouncementsPageViewModel
{
    public bool IsAdmin { get; set; }
    public List<Announcement> Announcements { get; set; } = [];
    public AnnouncementInputModel NewAnnouncement { get; set; } = new();
}
