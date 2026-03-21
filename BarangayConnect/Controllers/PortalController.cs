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

    [HttpGet]
    public IActionResult Documentation()
    {
        var model = new SystemDocumentationViewModel
        {
            ErdImagePath = "/docs/Final-ERD.jpg",
            ErdPdfPath = "/docs/Final-ERD.pdf",
            Features =
            [
                new FeatureHighlight
                {
                    Title = "Resident Registry",
                    Description = "Stores core resident records used by appointments and request transactions.",
                    Route = "/Portal/Residents"
                },
                new FeatureHighlight
                {
                    Title = "Announcements Board",
                    Description = "Publishes public notices, schedules, and community updates in one page.",
                    Route = "/Portal/Announcements"
                },
                new FeatureHighlight
                {
                    Title = "Service Catalog",
                    Description = "Lists available barangay services together with office schedules and document requirements.",
                    Route = "/Portal/Services"
                },
                new FeatureHighlight
                {
                    Title = "Appointment Scheduling",
                    Description = "Allows residents to reserve service slots that link to both resident and service records.",
                    Route = "/Portal/Appointments"
                },
                new FeatureHighlight
                {
                    Title = "Service Request Tracking",
                    Description = "Captures follow-up requests with description, priority, and status tracking.",
                    Route = "/Portal/Requests"
                }
            ],
            Previews =
            [
                new ScreenPreview
                {
                    Title = "Dashboard Preview",
                    Description = "Shows the main civic dashboard with statistics, updates, and service access.",
                    Route = "/Portal/Index"
                },
                new ScreenPreview
                {
                    Title = "Residents Preview",
                    Description = "Displays the resident registry form and searchable resident table.",
                    Route = "/Portal/Residents"
                },
                new ScreenPreview
                {
                    Title = "Appointments Preview",
                    Description = "Shows the appointment form and scheduled visits table.",
                    Route = "/Portal/Appointments"
                },
                new ScreenPreview
                {
                    Title = "Requests Preview",
                    Description = "Displays the request intake form and tracking table.",
                    Route = "/Portal/Requests"
                }
            ],
            Tables =
            [
                new DatabaseTableReference
                {
                    TableName = "Residents",
                    Summary = "Stores resident identity and contact data used across the system.",
                    Columns =
                    [
                        new ColumnReference { Name = "ResidentId", DataType = "INTEGER", Description = "Unique resident identifier", KeyType = "Primary Key" },
                        new ColumnReference { Name = "FullName", DataType = "TEXT", Description = "Resident full name", KeyType = "" },
                        new ColumnReference { Name = "HouseholdNo", DataType = "TEXT", Description = "Household reference number", KeyType = "" },
                        new ColumnReference { Name = "ContactNumber", DataType = "TEXT", Description = "Primary contact number", KeyType = "" },
                        new ColumnReference { Name = "EmailAddress", DataType = "TEXT", Description = "Resident email address", KeyType = "" },
                        new ColumnReference { Name = "Purok", DataType = "TEXT", Description = "Purok or zone assignment", KeyType = "" },
                        new ColumnReference { Name = "RegisteredOn", DataType = "TEXT", Description = "Registration date", KeyType = "" }
                    ]
                },
                new DatabaseTableReference
                {
                    TableName = "Services",
                    Summary = "Contains barangay services, offices, schedules, and requirements.",
                    Columns =
                    [
                        new ColumnReference { Name = "ServiceId", DataType = "INTEGER", Description = "Unique service identifier", KeyType = "Primary Key" },
                        new ColumnReference { Name = "Name", DataType = "TEXT", Description = "Service name", KeyType = "" },
                        new ColumnReference { Name = "Office", DataType = "TEXT", Description = "Responsible office or desk", KeyType = "" },
                        new ColumnReference { Name = "Description", DataType = "TEXT", Description = "Service description", KeyType = "" },
                        new ColumnReference { Name = "Schedule", DataType = "TEXT", Description = "Office schedule", KeyType = "" },
                        new ColumnReference { Name = "Requirements", DataType = "TEXT", Description = "Document and process requirements", KeyType = "" }
                    ]
                },
                new DatabaseTableReference
                {
                    TableName = "Announcements",
                    Summary = "Stores public notices published for residents.",
                    Columns =
                    [
                        new ColumnReference { Name = "AnnouncementId", DataType = "INTEGER", Description = "Unique announcement identifier", KeyType = "Primary Key" },
                        new ColumnReference { Name = "Title", DataType = "TEXT", Description = "Announcement title", KeyType = "" },
                        new ColumnReference { Name = "Category", DataType = "TEXT", Description = "Notice category", KeyType = "" },
                        new ColumnReference { Name = "Summary", DataType = "TEXT", Description = "Announcement content summary", KeyType = "" },
                        new ColumnReference { Name = "PublishedOn", DataType = "TEXT", Description = "Publication date", KeyType = "" },
                        new ColumnReference { Name = "Audience", DataType = "TEXT", Description = "Target audience", KeyType = "" }
                    ]
                },
                new DatabaseTableReference
                {
                    TableName = "Appointments",
                    Summary = "Links residents and services to scheduled visits.",
                    Columns =
                    [
                        new ColumnReference { Name = "AppointmentId", DataType = "INTEGER", Description = "Unique appointment identifier", KeyType = "Primary Key" },
                        new ColumnReference { Name = "ResidentId", DataType = "INTEGER", Description = "Resident who booked the appointment", KeyType = "Foreign Key -> Residents.ResidentId" },
                        new ColumnReference { Name = "ServiceId", DataType = "INTEGER", Description = "Chosen service", KeyType = "Foreign Key -> Services.ServiceId" },
                        new ColumnReference { Name = "AppointmentDate", DataType = "TEXT", Description = "Date of scheduled visit", KeyType = "" },
                        new ColumnReference { Name = "TimeSlot", DataType = "TEXT", Description = "Scheduled time range", KeyType = "" },
                        new ColumnReference { Name = "Status", DataType = "TEXT", Description = "Appointment status", KeyType = "" },
                        new ColumnReference { Name = "Notes", DataType = "TEXT", Description = "Purpose or notes", KeyType = "" }
                    ]
                },
                new DatabaseTableReference
                {
                    TableName = "ServiceRequests",
                    Summary = "Tracks resident service concerns and follow-up cases.",
                    Columns =
                    [
                        new ColumnReference { Name = "RequestId", DataType = "INTEGER", Description = "Unique request identifier", KeyType = "Primary Key" },
                        new ColumnReference { Name = "ResidentId", DataType = "INTEGER", Description = "Resident who submitted the request", KeyType = "Foreign Key -> Residents.ResidentId" },
                        new ColumnReference { Name = "ServiceId", DataType = "INTEGER", Description = "Requested service", KeyType = "Foreign Key -> Services.ServiceId" },
                        new ColumnReference { Name = "Description", DataType = "TEXT", Description = "Request details", KeyType = "" },
                        new ColumnReference { Name = "Priority", DataType = "TEXT", Description = "Priority level", KeyType = "" },
                        new ColumnReference { Name = "Status", DataType = "TEXT", Description = "Request processing status", KeyType = "" },
                        new ColumnReference { Name = "SubmittedOn", DataType = "TEXT", Description = "Submission date", KeyType = "" }
                    ]
                }
            ],
            SqlStatements =
            [
                new SqlStatementReference { Label = "Create Residents Table", Category = "Schema", Sql = """
                    CREATE TABLE IF NOT EXISTS Residents (
                        ResidentId INTEGER PRIMARY KEY AUTOINCREMENT,
                        FullName TEXT NOT NULL,
                        HouseholdNo TEXT NOT NULL,
                        ContactNumber TEXT NOT NULL,
                        EmailAddress TEXT NOT NULL,
                        Purok TEXT NOT NULL,
                        RegisteredOn TEXT NOT NULL
                    );
                    """ },
                new SqlStatementReference { Label = "Create Services Table", Category = "Schema", Sql = """
                    CREATE TABLE IF NOT EXISTS Services (
                        ServiceId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Office TEXT NOT NULL,
                        Description TEXT NOT NULL,
                        Schedule TEXT NOT NULL,
                        Requirements TEXT NOT NULL
                    );
                    """ },
                new SqlStatementReference { Label = "Create Announcements Table", Category = "Schema", Sql = """
                    CREATE TABLE IF NOT EXISTS Announcements (
                        AnnouncementId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Category TEXT NOT NULL,
                        Summary TEXT NOT NULL,
                        PublishedOn TEXT NOT NULL,
                        Audience TEXT NOT NULL
                    );
                    """ },
                new SqlStatementReference { Label = "Create Appointments Table", Category = "Schema", Sql = """
                    CREATE TABLE IF NOT EXISTS Appointments (
                        AppointmentId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ResidentId INTEGER NOT NULL,
                        ServiceId INTEGER NOT NULL,
                        AppointmentDate TEXT NOT NULL,
                        TimeSlot TEXT NOT NULL,
                        Status TEXT NOT NULL,
                        Notes TEXT NOT NULL,
                        FOREIGN KEY (ResidentId) REFERENCES Residents(ResidentId),
                        FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId)
                    );
                    """ },
                new SqlStatementReference { Label = "Create ServiceRequests Table", Category = "Schema", Sql = """
                    CREATE TABLE IF NOT EXISTS ServiceRequests (
                        RequestId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ResidentId INTEGER NOT NULL,
                        ServiceId INTEGER NOT NULL,
                        Description TEXT NOT NULL,
                        Priority TEXT NOT NULL,
                        Status TEXT NOT NULL,
                        SubmittedOn TEXT NOT NULL,
                        FOREIGN KEY (ResidentId) REFERENCES Residents(ResidentId),
                        FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId)
                    );
                    """ },
                new SqlStatementReference { Label = "Dashboard Counts", Category = "Read", Sql = """
                    SELECT COUNT(*) FROM Residents;

                    SELECT COUNT(*) FROM Appointments;

                    SELECT COUNT(*)
                    FROM ServiceRequests
                    WHERE Status <> 'Completed';

                    SELECT COUNT(*) FROM Announcements;
                    """ },
                new SqlStatementReference { Label = "Get Announcements", Category = "Read", Sql = """
                    SELECT AnnouncementId, Title, Category, Summary, PublishedOn, Audience
                    FROM Announcements
                    ORDER BY date(PublishedOn) DESC
                    LIMIT 6;
                    """ },
                new SqlStatementReference { Label = "Get Residents", Category = "Read", Sql = """
                    SELECT ResidentId, FullName, HouseholdNo, ContactNumber, EmailAddress, Purok, RegisteredOn
                    FROM Residents
                    ORDER BY FullName;
                    """ },
                new SqlStatementReference { Label = "Insert Resident", Category = "Write", Sql = """
                    INSERT INTO Residents (
                        FullName,
                        HouseholdNo,
                        ContactNumber,
                        EmailAddress,
                        Purok,
                        RegisteredOn
                    )
                    VALUES (
                        @FullName,
                        @HouseholdNo,
                        @ContactNumber,
                        @EmailAddress,
                        @Purok,
                        @RegisteredOn
                    );
                    """ },
                new SqlStatementReference { Label = "Get Services", Category = "Read", Sql = """
                    SELECT ServiceId, Name, Office, Description, Schedule, Requirements
                    FROM Services
                    ORDER BY Name;
                    """ },
                new SqlStatementReference { Label = "Get Appointments", Category = "Read", Sql = """
                    SELECT
                        a.AppointmentId,
                        r.FullName,
                        s.Name,
                        a.AppointmentDate,
                        a.TimeSlot,
                        a.Status,
                        a.Notes
                    FROM Appointments a
                    INNER JOIN Residents r ON r.ResidentId = a.ResidentId
                    INNER JOIN Services s ON s.ServiceId = a.ServiceId
                    ORDER BY date(a.AppointmentDate), a.TimeSlot
                    LIMIT 8;
                    """ },
                new SqlStatementReference { Label = "Insert Appointment", Category = "Write", Sql = """
                    INSERT INTO Appointments (
                        ResidentId,
                        ServiceId,
                        AppointmentDate,
                        TimeSlot,
                        Status,
                        Notes
                    )
                    VALUES (
                        @ResidentId,
                        @ServiceId,
                        @AppointmentDate,
                        @TimeSlot,
                        'Scheduled',
                        @Notes
                    );
                    """ },
                new SqlStatementReference { Label = "Get Service Requests", Category = "Read", Sql = """
                    SELECT
                        sr.RequestId,
                        r.FullName,
                        s.Name,
                        sr.Description,
                        sr.Priority,
                        sr.Status,
                        sr.SubmittedOn
                    FROM ServiceRequests sr
                    INNER JOIN Residents r ON r.ResidentId = sr.ResidentId
                    INNER JOIN Services s ON s.ServiceId = sr.ServiceId
                    ORDER BY date(sr.SubmittedOn) DESC, sr.RequestId DESC
                    LIMIT 8;
                    """ },
                new SqlStatementReference { Label = "Insert Service Request", Category = "Write", Sql = """
                    INSERT INTO ServiceRequests (
                        ResidentId,
                        ServiceId,
                        Description,
                        Priority,
                        Status,
                        SubmittedOn
                    )
                    VALUES (
                        @ResidentId,
                        @ServiceId,
                        @Description,
                        @Priority,
                        'Pending',
                        @SubmittedOn
                    );
                    """ }
            ]
        };

        return View(model);
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
