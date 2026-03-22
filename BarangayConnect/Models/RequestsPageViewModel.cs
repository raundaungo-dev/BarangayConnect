using Microsoft.AspNetCore.Mvc.Rendering;

namespace BarangayConnect.Models;

public class RequestsPageViewModel
{
    public bool IsAdmin { get; set; }
    public Resident? CurrentResident { get; set; }
    public List<Resident> Residents { get; set; } = [];
    public List<Service> Services { get; set; } = [];
    public List<ServiceRequest> Requests { get; set; } = [];
    public ServiceRequestInputModel NewRequest { get; set; } = new();
    public List<SelectListItem> ResidentOptions { get; set; } = [];
    public List<SelectListItem> ServiceOptions { get; set; } = [];
}
