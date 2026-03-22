namespace BarangayConnect.Models;

public class ResidentsPageViewModel
{
    public bool IsAdmin { get; set; }
    public Resident? CurrentResident { get; set; }
    public List<Resident> Residents { get; set; } = [];
    public ResidentInputModel NewResident { get; set; } = new();
}
