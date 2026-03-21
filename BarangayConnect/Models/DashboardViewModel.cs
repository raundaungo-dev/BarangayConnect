namespace BarangayConnect.Models;

public class DashboardViewModel
{
    public int ResidentCount { get; set; }
    public int AppointmentCount { get; set; }
    public int PendingRequestCount { get; set; }
    public int ActiveAnnouncementCount { get; set; }
    public List<Announcement> RecentAnnouncements { get; set; } = [];
    public List<Appointment> UpcomingAppointments { get; set; } = [];
    public List<ServiceRequest> LatestRequests { get; set; } = [];
}
