using BarangayConnect.Models;
using BarangayConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace BarangayConnect.Controllers;

public class PortalController : Controller
{
    private const string SessionAccountId = "AccountId";
    private const string SessionAccountRole = "AccountRole";
    private readonly PortalRepository _repository;

    public PortalController(PortalRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetInt32(SessionAccountId).HasValue)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var account = await _repository.GetAccountByCredentialsAsync(model.Username, model.Password);
        if (account is null)
        {
            model.ErrorMessage = "Invalid username or password.";
            return View(model);
        }

        HttpContext.Session.SetInt32(SessionAccountId, account.Id);
        HttpContext.Session.SetString(SessionAccountRole, account.Role);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.GetInt32(SessionAccountId).HasValue)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _repository.UsernameExistsAsync(model.Username))
        {
            model.ErrorMessage = "That username is already taken.";
            return View(model);
        }

        var accountId = await _repository.AddAccountAsync(model);
        HttpContext.Session.SetInt32(SessionAccountId, accountId);
        HttpContext.Session.SetString(SessionAccountRole, "User");
        TempData["StatusMessage"] = "Account created successfully. You can now complete your resident profile.";
        return RedirectToAction(nameof(Residents));
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Index()
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var announcements = await _repository.GetAnnouncementsAsync();
        var dashboard = new DashboardViewModel
        {
            DisplayName = account.DisplayName,
            IsAdmin = IsAdmin(account),
            RecentAnnouncements = announcements,
            ActiveAnnouncementCount = announcements.Count
        };

        if (dashboard.IsAdmin)
        {
            dashboard.ResidentCount = await _repository.GetResidentCountAsync();
            dashboard.AppointmentCount = await _repository.GetAppointmentCountAsync();
            dashboard.PendingRequestCount = await _repository.GetPendingRequestCountAsync();
            dashboard.UpcomingAppointments = await _repository.GetAppointmentsAsync();
            dashboard.LatestRequests = await _repository.GetServiceRequestsAsync();
        }

        return View(dashboard);
    }

    public async Task<IActionResult> Announcements()
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var announcements = await _repository.GetAnnouncementsAsync();
        return View(announcements);
    }

    [HttpGet]
    public async Task<IActionResult> Residents()
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (IsAdmin(account))
        {
            return View(new ResidentsPageViewModel
            {
                IsAdmin = true,
                Residents = await _repository.GetResidentsAsync(),
                NewResident = new ResidentInputModel()
            });
        }

        return View(new ResidentsPageViewModel
        {
            IsAdmin = false,
            CurrentResident = await GetCurrentResidentAsync(account),
            NewResident = new ResidentInputModel()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Residents(ResidentsPageViewModel model)
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            model.IsAdmin = IsAdmin(account);
            model.Residents = model.IsAdmin ? await _repository.GetResidentsAsync() : [];
            model.CurrentResident = model.IsAdmin ? null : await GetCurrentResidentAsync(account);
            return View(model);
        }

        var residentId = await _repository.AddResidentAsync(model.NewResident);

        if (!IsAdmin(account) && !account.ResidentId.HasValue)
        {
            await _repository.AssignResidentToAccountAsync(account.Id, residentId);
            HttpContext.Session.SetInt32(SessionAccountId, account.Id);
        }

        TempData["StatusMessage"] = "Resident record saved successfully.";
        return RedirectToAction(nameof(Residents));
    }

    [HttpGet]
    public async Task<IActionResult> Services()
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new ServicesPageViewModel
        {
            IsAdmin = IsAdmin(account),
            Services = await _repository.GetServicesAsync(),
            NewService = new ServiceInputModel()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Services(ServicesPageViewModel model)
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!IsAdmin(account))
        {
            return RedirectToAction(nameof(Services));
        }

        if (!ModelState.IsValid)
        {
            model.IsAdmin = true;
            model.Services = await _repository.GetServicesAsync();
            return View(model);
        }

        await _repository.AddServiceAsync(model.NewService);
        TempData["StatusMessage"] = "Service added successfully.";
        return RedirectToAction(nameof(Services));
    }

    [HttpGet]
    public async Task<IActionResult> Appointments()
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(await BuildAppointmentsPageAsync(account, new AppointmentInputModel
        {
            AppointmentDate = DateTime.Today.AddDays(1)
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Appointments(AppointmentsPageViewModel model)
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!IsAdmin(account))
        {
            var currentResident = await GetCurrentResidentAsync(account);
            if (currentResident is null)
            {
                TempData["StatusMessage"] = "Please register your resident profile first.";
                return RedirectToAction(nameof(Residents));
            }

            model.NewAppointment.ResidentId = currentResident.Id;
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildAppointmentsPageAsync(account, model.NewAppointment));
        }

        await _repository.AddAppointmentAsync(model.NewAppointment);
        TempData["StatusMessage"] = "Appointment scheduled successfully.";
        return RedirectToAction(nameof(Appointments));
    }

    [HttpGet]
    public async Task<IActionResult> Requests()
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(await BuildRequestsPageAsync(account, new ServiceRequestInputModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Requests(RequestsPageViewModel model)
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!IsAdmin(account))
        {
            var currentResident = await GetCurrentResidentAsync(account);
            if (currentResident is null)
            {
                TempData["StatusMessage"] = "Please register your resident profile first.";
                return RedirectToAction(nameof(Residents));
            }

            model.NewRequest.ResidentId = currentResident.Id;
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildRequestsPageAsync(account, model.NewRequest));
        }

        await _repository.AddServiceRequestAsync(model.NewRequest);
        TempData["StatusMessage"] = "Service request submitted successfully.";
        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRequest(int requestId, string priority, string status)
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!IsAdmin(account))
        {
            TempData["StatusMessage"] = "Only administrators can update request records.";
            return RedirectToAction(nameof(Requests));
        }

        var allowedPriorities = new[] { "Normal", "High", "Urgent" };
        var allowedStatuses = new[] { "Pending", "In Review", "Completed" };

        if (!allowedPriorities.Contains(priority) || !allowedStatuses.Contains(status))
        {
            TempData["StatusMessage"] = "Invalid request update values were submitted.";
            return RedirectToAction(nameof(Requests));
        }

        await _repository.UpdateServiceRequestAsync(requestId, priority, status);
        TempData["StatusMessage"] = "Request updated successfully.";
        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRequest(int requestId)
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!IsAdmin(account))
        {
            TempData["StatusMessage"] = "Only administrators can delete completed requests.";
            return RedirectToAction(nameof(Requests));
        }

        var deleted = await _repository.DeleteCompletedServiceRequestAsync(requestId);
        TempData["StatusMessage"] = deleted
            ? "Completed request deleted successfully."
            : "Only completed requests can be deleted.";

        return RedirectToAction(nameof(Requests));
    }

    [HttpGet]
    public async Task<IActionResult> Documentation()
    {
        var account = await RequireAccountAsync();
        if (account is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!IsAdmin(account))
        {
            TempData["StatusMessage"] = "Documentation is only available to administrator accounts.";
            return RedirectToAction(nameof(Index));
        }

        var model = new SystemDocumentationViewModel
        {
            ErdImagePath = "/docs/Final-ERD.jpg",
            ErdPdfPath = "/docs/Final-ERD.pdf",
            Features =
            [
                new FeatureHighlight { Title = "Resident Registry", Description = "Stores core resident records used by appointments and request transactions.", Route = "/Portal/Residents" },
                new FeatureHighlight { Title = "Announcements Board", Description = "Publishes public notices, schedules, and community updates in one page.", Route = "/Portal/Announcements" },
                new FeatureHighlight { Title = "Service Catalog", Description = "Lists available barangay services together with office schedules and document requirements.", Route = "/Portal/Services" },
                new FeatureHighlight { Title = "Appointment Scheduling", Description = "Allows residents to reserve service slots that link to both resident and service records.", Route = "/Portal/Appointments" },
                new FeatureHighlight { Title = "Service Request Tracking", Description = "Captures follow-up requests with description, priority, and status tracking.", Route = "/Portal/Requests" }
            ],
            Previews =
            [
                new ScreenPreview { Title = "Dashboard Preview", Description = "Shows the main civic dashboard with statistics, updates, and service access.", Route = "/Portal/Index" },
                new ScreenPreview { Title = "Residents Preview", Description = "Displays the resident registry form and searchable resident table.", Route = "/Portal/Residents" },
                new ScreenPreview { Title = "Appointments Preview", Description = "Shows the appointment form and scheduled visits table.", Route = "/Portal/Appointments" },
                new ScreenPreview { Title = "Requests Preview", Description = "Displays the request intake form and tracking table.", Route = "/Portal/Requests" }
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
                    TableName = "Accounts",
                    Summary = "Stores login accounts for admins and resident users.",
                    Columns =
                    [
                        new ColumnReference { Name = "AccountId", DataType = "INTEGER", Description = "Unique account identifier", KeyType = "Primary Key" },
                        new ColumnReference { Name = "Username", DataType = "TEXT", Description = "Login username", KeyType = "" },
                        new ColumnReference { Name = "Password", DataType = "TEXT", Description = "Demo password value", KeyType = "" },
                        new ColumnReference { Name = "DisplayName", DataType = "TEXT", Description = "Name shown in the interface", KeyType = "" },
                        new ColumnReference { Name = "Role", DataType = "TEXT", Description = "Admin or User role", KeyType = "" },
                        new ColumnReference { Name = "ResidentId", DataType = "INTEGER", Description = "Linked resident profile", KeyType = "Foreign Key -> Residents.ResidentId" }
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
            ]
        };

        model.SqlStatements =
        [
            new SqlStatementReference { Label = "Create Accounts Table", Category = "Schema", Sql = """
                CREATE TABLE IF NOT EXISTS Accounts (
                    AccountId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    DisplayName TEXT NOT NULL,
                    Role TEXT NOT NULL,
                    ResidentId INTEGER NULL,
                    FOREIGN KEY (ResidentId) REFERENCES Residents(ResidentId)
                );
                """ },
            new SqlStatementReference { Label = "Get Account By Credentials", Category = "Read", Sql = """
                SELECT AccountId, Username, Password, DisplayName, Role, ResidentId
                FROM Accounts
                WHERE lower(Username) = lower(@Username) AND Password = @Password
                LIMIT 1;
                """ },
            new SqlStatementReference { Label = "Assign Resident To Account", Category = "Write", Sql = """
                UPDATE Accounts
                SET ResidentId = @ResidentId
                WHERE AccountId = @AccountId;
                """ },
            new SqlStatementReference { Label = "Insert Service", Category = "Write", Sql = """
                INSERT INTO Services (Name, Office, Description, Schedule, Requirements)
                VALUES (@Name, @Office, @Description, @Schedule, @Requirements);
                """ }
        ];

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    private async Task<AppointmentsPageViewModel> BuildAppointmentsPageAsync(Account account, AppointmentInputModel inputModel)
    {
        var isAdmin = IsAdmin(account);
        var currentResident = await GetCurrentResidentAsync(account);
        var residents = isAdmin ? await _repository.GetResidentsAsync() : currentResident is null ? [] : [currentResident];
        var services = await _repository.GetServicesAsync();
        var appointments = await _repository.GetAppointmentsAsync(isAdmin ? null : currentResident?.Id);

        if (!isAdmin && currentResident is not null)
        {
            inputModel.ResidentId = currentResident.Id;
        }

        return new AppointmentsPageViewModel
        {
            IsAdmin = isAdmin,
            CurrentResident = currentResident,
            Residents = residents,
            Services = services,
            Appointments = appointments,
            NewAppointment = inputModel,
            ResidentOptions = residents.Select(resident => new SelectListItem(resident.FullName, resident.Id.ToString())).ToList(),
            ServiceOptions = services.Select(service => new SelectListItem(service.Name, service.Id.ToString())).ToList()
        };
    }

    private async Task<RequestsPageViewModel> BuildRequestsPageAsync(Account account, ServiceRequestInputModel inputModel)
    {
        var isAdmin = IsAdmin(account);
        var currentResident = await GetCurrentResidentAsync(account);
        var residents = isAdmin ? await _repository.GetResidentsAsync() : currentResident is null ? [] : [currentResident];
        var services = await _repository.GetServicesAsync();
        var requests = await _repository.GetServiceRequestsAsync(isAdmin ? null : currentResident?.Id);

        if (!isAdmin && currentResident is not null)
        {
            inputModel.ResidentId = currentResident.Id;
        }

        return new RequestsPageViewModel
        {
            IsAdmin = isAdmin,
            CurrentResident = currentResident,
            Residents = residents,
            Services = services,
            Requests = requests,
            NewRequest = inputModel,
            ResidentOptions = residents.Select(resident => new SelectListItem(resident.FullName, resident.Id.ToString())).ToList(),
            ServiceOptions = services.Select(service => new SelectListItem(service.Name, service.Id.ToString())).ToList()
        };
    }

    private async Task<Account?> RequireAccountAsync()
    {
        var accountId = HttpContext.Session.GetInt32(SessionAccountId);
        if (!accountId.HasValue)
        {
            return null;
        }

        var account = await _repository.GetAccountByIdAsync(accountId.Value);
        if (account is not null)
        {
            HttpContext.Session.SetString(SessionAccountRole, account.Role);
        }

        return account;
    }

    private async Task<Resident?> GetCurrentResidentAsync(Account account)
    {
        return account.ResidentId.HasValue
            ? await _repository.GetResidentByIdAsync(account.ResidentId.Value)
            : null;
    }

    private static bool IsAdmin(Account account) =>
        string.Equals(account.Role, "Admin", StringComparison.OrdinalIgnoreCase);
}
