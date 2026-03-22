namespace BarangayConnect.Models;

public class ServicesPageViewModel
{
    public List<Service> Services { get; set; } = [];
    public ServiceInputModel NewService { get; set; } = new();
    public bool IsAdmin { get; set; }
}
