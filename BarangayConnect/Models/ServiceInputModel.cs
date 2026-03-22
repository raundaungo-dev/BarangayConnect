using System.ComponentModel.DataAnnotations;

namespace BarangayConnect.Models;

public class ServiceInputModel
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Office { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Schedule { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Requirements { get; set; } = string.Empty;
}
