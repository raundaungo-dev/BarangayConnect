using System.ComponentModel.DataAnnotations;

namespace BarangayConnect.Models;

public class AppointmentInputModel
{
    [Display(Name = "Resident")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a resident.")]
    public int ResidentId { get; set; }

    [Display(Name = "Service")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a service.")]
    public int ServiceId { get; set; }

    [Display(Name = "Appointment Date")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; }

    [Display(Name = "Time Slot")]
    [Required]
    [StringLength(50)]
    public string TimeSlot { get; set; } = string.Empty;

    [Display(Name = "Reason / Notes")]
    [Required]
    [StringLength(250)]
    public string Notes { get; set; } = string.Empty;
}
