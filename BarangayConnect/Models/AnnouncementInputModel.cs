using System.ComponentModel.DataAnnotations;

namespace BarangayConnect.Models;

public class AnnouncementInputModel
{
    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Summary { get; set; } = string.Empty;

    [Display(Name = "Published On")]
    [DataType(DataType.Date)]
    public DateTime PublishedOn { get; set; } = DateTime.Today;

    [Required]
    [StringLength(80)]
    public string Audience { get; set; } = string.Empty;
}
