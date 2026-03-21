using BarangayConnect.Models;
using BarangayConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace BarangayConnect.Controllers;

public class PortalController : Controller
{
    private readonly PortalRepository _repository;

    public PortalController(PortalRepository repository)
    {
        _repository = repository;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = await _repository.GetDashboardAsync();
        return View(dashboard);
    }

    public async Task<IActionResult> Announcements()
    {
        var announcements = await _repository.GetAnnouncementsAsync();
        return View(announcements);
    }

    [HttpGet]
    public async Task<IActionResult> Residents()
    {
        var residents = await _repository.GetResidentsAsync();
        return View(new ResidentsPageViewModel
        {
            Residents = residents,
            NewResident = new ResidentInputModel()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Residents(ResidentsPageViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _repository.AddResidentAsync(model.NewResident);
            TempData["StatusMessage"] = "Resident record added successfully.";
            return RedirectToAction(nameof(Residents));
        }

        model.Residents = await _repository.GetResidentsAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Services()
    {
        var services = await _repository.GetServicesAsync();
        return View(services);
    }

    [HttpGet]
    public async Task<IActionResult> Appointments()
    {
        var viewModel = await BuildAppointmentsPageAsync(new AppointmentInputModel
        {
            AppointmentDate = DateTime.Today.AddDays(1)
        });

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Appointments(AppointmentsPageViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _repository.AddAppointmentAsync(model.NewAppointment);
            TempData["StatusMessage"] = "Appointment scheduled successfully.";
            return RedirectToAction(nameof(Appointments));
        }

        return View(await BuildAppointmentsPageAsync(model.NewAppointment));
    }

    [HttpGet]
    public async Task<IActionResult> Requests()
    {
        var viewModel = await BuildRequestsPageAsync(new ServiceRequestInputModel());
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Requests(RequestsPageViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _repository.AddServiceRequestAsync(model.NewRequest);
            TempData["StatusMessage"] = "Service request submitted successfully.";
            return RedirectToAction(nameof(Requests));
        }

        return View(await BuildRequestsPageAsync(model.NewRequest));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    private async Task<AppointmentsPageViewModel> BuildAppointmentsPageAsync(AppointmentInputModel inputModel)
    {
        var residents = await _repository.GetResidentsAsync();
        var services = await _repository.GetServicesAsync();
        var appointments = await _repository.GetAppointmentsAsync();

        return new AppointmentsPageViewModel
        {
            Residents = residents,
            Services = services,
            Appointments = appointments,
            NewAppointment = inputModel,
            ResidentOptions = residents
                .Select(resident => new SelectListItem(resident.FullName, resident.Id.ToString()))
                .ToList(),
            ServiceOptions = services
                .Select(service => new SelectListItem(service.Name, service.Id.ToString()))
                .ToList()
        };
    }

    private async Task<RequestsPageViewModel> BuildRequestsPageAsync(ServiceRequestInputModel inputModel)
    {
        var residents = await _repository.GetResidentsAsync();
        var services = await _repository.GetServicesAsync();
        var requests = await _repository.GetServiceRequestsAsync();

        return new RequestsPageViewModel
        {
            Residents = residents,
            Services = services,
            Requests = requests,
            NewRequest = inputModel,
            ResidentOptions = residents
                .Select(resident => new SelectListItem(resident.FullName, resident.Id.ToString()))
                .ToList(),
            ServiceOptions = services
                .Select(service => new SelectListItem(service.Name, service.Id.ToString()))
                .ToList()
        };
    }
}
