using System.ComponentModel.DataAnnotations;

namespace BarangayConnect.Models;

public class ResidentInputModel
{
    [Display(Name = "Full Name")]
    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Household No.")]
    [Required]
    [StringLength(30)]
    public string HouseholdNo { get; set; } = string.Empty;

    [Display(Name = "Contact Number")]
    [Required]
    [StringLength(30)]
    public string ContactNumber { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string EmailAddress { get; set; } = string.Empty;

    [Display(Name = "Purok / Zone")]
    [Required]
    [StringLength(50)]
    public string Purok { get; set; } = string.Empty;
}
