using Microsoft.AspNetCore.Mvc.Rendering;

namespace BarangayConnect.Models;

public class AppointmentsPageViewModel
{
    public bool IsAdmin { get; set; }
    public Resident? CurrentResident { get; set; }
    public List<Resident> Residents { get; set; } = [];
    public List<Service> Services { get; set; } = [];
    public List<Appointment> Appointments { get; set; } = [];
    public AppointmentInputModel NewAppointment { get; set; } = new();
    public List<SelectListItem> ResidentOptions { get; set; } = [];
    public List<SelectListItem> ServiceOptions { get; set; } = [];
}
