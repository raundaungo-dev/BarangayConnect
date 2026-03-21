using System.ComponentModel.DataAnnotations;

namespace BarangayConnect.Models;

public class ServiceRequestInputModel
{
    [Display(Name = "Resident")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a resident.")]
    public int ResidentId { get; set; }

    [Display(Name = "Service")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a service.")]
    public int ServiceId { get; set; }

    [Display(Name = "Request Description")]
    [Required]
    [StringLength(400)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Priority { get; set; } = "Normal";
}
