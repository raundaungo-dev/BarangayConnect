using BarangayConnect.Models;
using Microsoft.Data.Sqlite;

namespace BarangayConnect.Services;

public class PortalRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public PortalRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var schemaSql = """
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS Residents (
                ResidentId INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                HouseholdNo TEXT NOT NULL,
                ContactNumber TEXT NOT NULL,
                EmailAddress TEXT NOT NULL,
                Purok TEXT NOT NULL,
                RegisteredOn TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Services (
                ServiceId INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Office TEXT NOT NULL,
                Description TEXT NOT NULL,
                Schedule TEXT NOT NULL,
                Requirements TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Announcements (
                AnnouncementId INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Category TEXT NOT NULL,
                Summary TEXT NOT NULL,
                PublishedOn TEXT NOT NULL,
                Audience TEXT NOT NULL
            );

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
            """;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = schemaSql;
            await command.ExecuteNonQueryAsync();
        }

        await SeedIfEmptyAsync(connection);
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        return new DashboardViewModel
        {
            ResidentCount = await ExecuteScalarIntAsync("SELECT COUNT(*) FROM Residents;"),
            AppointmentCount = await ExecuteScalarIntAsync("SELECT COUNT(*) FROM Appointments;"),
            PendingRequestCount = await ExecuteScalarIntAsync("SELECT COUNT(*) FROM ServiceRequests WHERE Status <> 'Completed';"),
            ActiveAnnouncementCount = await ExecuteScalarIntAsync("SELECT COUNT(*) FROM Announcements;"),
            RecentAnnouncements = await GetAnnouncementsAsync(),
            UpcomingAppointments = await GetAppointmentsAsync(),
            LatestRequests = await GetServiceRequestsAsync()
        };
    }

    public async Task<List<Announcement>> GetAnnouncementsAsync()
    {
        const string sql = """
            SELECT AnnouncementId, Title, Category, Summary, PublishedOn, Audience
            FROM Announcements
            ORDER BY date(PublishedOn) DESC
            LIMIT 6;
            """;

        return await ExecuteReaderAsync(sql, reader => new Announcement
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Category = reader.GetString(2),
            Summary = reader.GetString(3),
            PublishedOn = DateTime.Parse(reader.GetString(4)),
            Audience = reader.GetString(5)
        });
    }

    public async Task<List<Resident>> GetResidentsAsync()
    {
        const string sql = """
            SELECT ResidentId, FullName, HouseholdNo, ContactNumber, EmailAddress, Purok, RegisteredOn
            FROM Residents
            ORDER BY FullName;
            """;

        return await ExecuteReaderAsync(sql, reader => new Resident
        {
            Id = reader.GetInt32(0),
            FullName = reader.GetString(1),
            HouseholdNo = reader.GetString(2),
            ContactNumber = reader.GetString(3),
            EmailAddress = reader.GetString(4),
            Purok = reader.GetString(5),
            RegisteredOn = DateTime.Parse(reader.GetString(6))
        });
    }

    public async Task AddResidentAsync(ResidentInputModel model)
    {
        const string sql = """
            INSERT INTO Residents (FullName, HouseholdNo, ContactNumber, EmailAddress, Purok, RegisteredOn)
            VALUES (@FullName, @HouseholdNo, @ContactNumber, @EmailAddress, @Purok, @RegisteredOn);
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@FullName", model.FullName);
            command.Parameters.AddWithValue("@HouseholdNo", model.HouseholdNo);
            command.Parameters.AddWithValue("@ContactNumber", model.ContactNumber);
            command.Parameters.AddWithValue("@EmailAddress", model.EmailAddress);
            command.Parameters.AddWithValue("@Purok", model.Purok);
            command.Parameters.AddWithValue("@RegisteredOn", DateTime.Today.ToString("yyyy-MM-dd"));
        });
    }

    public async Task<List<Service>> GetServicesAsync()
    {
        const string sql = """
            SELECT ServiceId, Name, Office, Description, Schedule, Requirements
            FROM Services
            ORDER BY Name;
            """;

        return await ExecuteReaderAsync(sql, reader => new Service
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Office = reader.GetString(2),
            Description = reader.GetString(3),
            Schedule = reader.GetString(4),
            Requirements = reader.GetString(5)
        });
    }

    public async Task<List<Appointment>> GetAppointmentsAsync()
    {
        const string sql = """
            SELECT a.AppointmentId, r.FullName, s.Name, a.AppointmentDate, a.TimeSlot, a.Status, a.Notes
            FROM Appointments a
            INNER JOIN Residents r ON r.ResidentId = a.ResidentId
            INNER JOIN Services s ON s.ServiceId = a.ServiceId
            ORDER BY date(a.AppointmentDate), a.TimeSlot
            LIMIT 8;
            """;

        return await ExecuteReaderAsync(sql, reader => new Appointment
        {
            Id = reader.GetInt32(0),
            ResidentName = reader.GetString(1),
            ServiceName = reader.GetString(2),
            AppointmentDate = DateTime.Parse(reader.GetString(3)),
            TimeSlot = reader.GetString(4),
            Status = reader.GetString(5),
            Notes = reader.GetString(6)
        });
    }

    public async Task AddAppointmentAsync(AppointmentInputModel model)
    {
        const string sql = """
            INSERT INTO Appointments (ResidentId, ServiceId, AppointmentDate, TimeSlot, Status, Notes)
            VALUES (@ResidentId, @ServiceId, @AppointmentDate, @TimeSlot, 'Scheduled', @Notes);
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@ResidentId", model.ResidentId);
            command.Parameters.AddWithValue("@ServiceId", model.ServiceId);
            command.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@TimeSlot", model.TimeSlot);
            command.Parameters.AddWithValue("@Notes", model.Notes);
        });
    }

    public async Task<List<ServiceRequest>> GetServiceRequestsAsync()
    {
        const string sql = """
            SELECT sr.RequestId, r.FullName, s.Name, sr.Description, sr.Priority, sr.Status, sr.SubmittedOn
            FROM ServiceRequests sr
            INNER JOIN Residents r ON r.ResidentId = sr.ResidentId
            INNER JOIN Services s ON s.ServiceId = sr.ServiceId
            ORDER BY date(sr.SubmittedOn) DESC, sr.RequestId DESC
            LIMIT 8;
            """;

        return await ExecuteReaderAsync(sql, reader => new ServiceRequest
        {
            Id = reader.GetInt32(0),
            ResidentName = reader.GetString(1),
            ServiceName = reader.GetString(2),
            Description = reader.GetString(3),
            Priority = reader.GetString(4),
            Status = reader.GetString(5),
            SubmittedOn = DateTime.Parse(reader.GetString(6))
        });
    }

    public async Task AddServiceRequestAsync(ServiceRequestInputModel model)
    {
        const string sql = """
            INSERT INTO ServiceRequests (ResidentId, ServiceId, Description, Priority, Status, SubmittedOn)
            VALUES (@ResidentId, @ServiceId, @Description, @Priority, 'Pending', @SubmittedOn);
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@ResidentId", model.ResidentId);
            command.Parameters.AddWithValue("@ServiceId", model.ServiceId);
            command.Parameters.AddWithValue("@Description", model.Description);
            command.Parameters.AddWithValue("@Priority", model.Priority);
            command.Parameters.AddWithValue("@SubmittedOn", DateTime.Today.ToString("yyyy-MM-dd"));
        });
    }

    private async Task SeedIfEmptyAsync(SqliteConnection connection)
    {
        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Services;";
        var existingServices = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

        if (existingServices > 0)
        {
            return;
        }

        var seedSql = """
            INSERT INTO Residents (FullName, HouseholdNo, ContactNumber, EmailAddress, Purok, RegisteredOn) VALUES
            ('Ana Dela Cruz', 'HH-001', '09171234567', 'ana.delacruz@example.com', 'Purok 1', '2026-03-01'),
            ('Miguel Santos', 'HH-014', '09181234567', 'miguel.santos@example.com', 'Purok 3', '2026-03-02'),
            ('Leah Ramos', 'HH-027', '09191234567', 'leah.ramos@example.com', 'Purok 5', '2026-03-05');

            INSERT INTO Services (Name, Office, Description, Schedule, Requirements) VALUES
            ('Barangay Clearance', 'Administrative Desk', 'Issuance of barangay clearance for employment, travel, and other legal uses.', 'Monday to Friday, 8:00 AM - 4:00 PM', 'Valid ID, filled-out request form'),
            ('Certificate of Residency', 'Records Unit', 'Official certification that a resident currently lives within the barangay jurisdiction.', 'Monday to Friday, 8:00 AM - 3:00 PM', 'Valid ID, proof of address'),
            ('Business Permit Endorsement', 'Business One-Stop Desk', 'Initial barangay endorsement required before city or municipal business permit processing.', 'Tuesday and Thursday, 9:00 AM - 3:00 PM', 'DTI registration, lease contract, valid ID'),
            ('Community Event Registration', 'Youth and Sports Desk', 'Registration and scheduling for barangay-wide seminars, youth programs, and wellness drives.', 'Wednesday to Saturday, 9:00 AM - 5:00 PM', 'Online registration form');

            INSERT INTO Announcements (Title, Category, Summary, PublishedOn, Audience) VALUES
            ('Weekend Clean-Up Drive', 'Community', 'Residents are encouraged to join the barangay clean-up drive this Saturday at 7:00 AM.', '2026-03-18', 'All residents'),
            ('Free Health Screening', 'Health', 'The barangay health center will conduct blood pressure and glucose screening for senior citizens.', '2026-03-16', 'Senior citizens'),
            ('Scholarship Orientation', 'Education', 'An orientation session for local scholarship applicants will be held at the session hall.', '2026-03-14', 'Students and parents');

            INSERT INTO Appointments (ResidentId, ServiceId, AppointmentDate, TimeSlot, Status, Notes) VALUES
            (1, 1, '2026-03-24', '9:00 AM - 9:30 AM', 'Scheduled', 'Employment requirement'),
            (2, 2, '2026-03-25', '1:00 PM - 1:30 PM', 'Scheduled', 'Updated residency document'),
            (3, 3, '2026-03-26', '10:00 AM - 10:30 AM', 'Scheduled', 'Small food business application');

            INSERT INTO ServiceRequests (ResidentId, ServiceId, Description, Priority, Status, SubmittedOn) VALUES
            (1, 4, 'Register two household members for the livelihood seminar.', 'Normal', 'Pending', '2026-03-19'),
            (2, 1, 'Requesting an additional copy of barangay clearance for visa processing.', 'High', 'In Review', '2026-03-20'),
            (3, 2, 'Need certificate of residency for school enrollment before March 28.', 'Urgent', 'Pending', '2026-03-21');
            """;

        await using var seedCommand = connection.CreateCommand();
        seedCommand.CommandText = seedSql;
        await seedCommand.ExecuteNonQueryAsync();
    }

    private async Task<int> ExecuteScalarIntAsync(string sql)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task ExecuteNonQueryAsync(string sql, Action<SqliteCommand>? configure = null)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure?.Invoke(command);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<List<T>> ExecuteReaderAsync<T>(string sql, Func<SqliteDataReader, T> map)
    {
        var items = new List<T>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(map(reader));
        }

        return items;
    }
}
