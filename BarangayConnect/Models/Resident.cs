namespace BarangayConnect.Models;

public class Resident
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string HouseholdNo { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string Purok { get; set; } = string.Empty;
    public DateTime RegisteredOn { get; set; }
}
