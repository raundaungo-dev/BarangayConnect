namespace BarangayConnect.Models;

public class ResidentsPageViewModel
{
    public List<Resident> Residents { get; set; } = [];
    public ResidentInputModel NewResident { get; set; } = new();
}
