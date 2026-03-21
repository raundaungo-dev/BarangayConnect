namespace BarangayConnect.Models;

public class Appointment
{
    public int Id { get; set; }
    public string ResidentName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
