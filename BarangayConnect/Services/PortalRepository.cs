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

            CREATE TABLE IF NOT EXISTS Accounts (
                AccountId INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                DisplayName TEXT NOT NULL,
                Role TEXT NOT NULL,
                ResidentId INTEGER NULL,
                FOREIGN KEY (ResidentId) REFERENCES Residents(ResidentId)
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

        await SeedReferenceDataAsync(connection);
        await SeedAccountsAsync(connection);
    }

    public async Task<int> GetResidentCountAsync()
    {
        return await ExecuteScalarIntAsync("SELECT COUNT(*) FROM Residents;");
    }

    public async Task<int> GetAppointmentCountAsync()
    {
        return await ExecuteScalarIntAsync("SELECT COUNT(*) FROM Appointments;");
    }

    public async Task<int> GetPendingRequestCountAsync()
    {
        return await ExecuteScalarIntAsync("SELECT COUNT(*) FROM ServiceRequests WHERE Status <> 'Completed';");
    }

    public async Task<int> GetAnnouncementCountAsync()
    {
        return await ExecuteScalarIntAsync("SELECT COUNT(*) FROM Announcements;");
    }

    public async Task<Account?> GetAccountByCredentialsAsync(string username, string password)
    {
        const string sql = """
            SELECT AccountId, Username, Password, DisplayName, Role, ResidentId
            FROM Accounts
            WHERE lower(Username) = lower(@Username) AND Password = @Password
            LIMIT 1;
            """;

        return await ExecuteSingleAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);
        }, MapAccount);
    }

    public async Task<Account?> GetAccountByIdAsync(int accountId)
    {
        const string sql = """
            SELECT AccountId, Username, Password, DisplayName, Role, ResidentId
            FROM Accounts
            WHERE AccountId = @AccountId
            LIMIT 1;
            """;

        return await ExecuteSingleAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@AccountId", accountId);
        }, MapAccount);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM Accounts
            WHERE lower(Username) = lower(@Username);
            """;

        return await ExecuteScalarIntAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@Username", username.Trim());
        }) > 0;
    }

    public async Task<int> AddAccountAsync(RegisterViewModel model)
    {
        const string sql = """
            INSERT INTO Accounts (Username, Password, DisplayName, Role, ResidentId)
            VALUES (@Username, @Password, @DisplayName, 'User', NULL);
            SELECT last_insert_rowid();
            """;

        return await ExecuteScalarIntAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@Username", model.Username.Trim());
            command.Parameters.AddWithValue("@Password", model.Password);
            command.Parameters.AddWithValue("@DisplayName", model.DisplayName.Trim());
        });
    }

    public async Task<List<Announcement>> GetAnnouncementsAsync()
    {
        const string sql = """
            SELECT AnnouncementId, Title, Category, Summary, PublishedOn, Audience
            FROM Announcements
            ORDER BY date(PublishedOn) DESC
            LIMIT 6;
            """;

        return await ExecuteReaderAsync(sql, null, reader => new Announcement
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Category = reader.GetString(2),
            Summary = reader.GetString(3),
            PublishedOn = DateTime.Parse(reader.GetString(4)),
            Audience = reader.GetString(5)
        });
    }

    public async Task AddAnnouncementAsync(AnnouncementInputModel model)
    {
        const string sql = """
            INSERT INTO Announcements (Title, Category, Summary, PublishedOn, Audience)
            VALUES (@Title, @Category, @Summary, @PublishedOn, @Audience);
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@Title", model.Title.Trim());
            command.Parameters.AddWithValue("@Category", model.Category.Trim());
            command.Parameters.AddWithValue("@Summary", model.Summary.Trim());
            command.Parameters.AddWithValue("@PublishedOn", model.PublishedOn.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Audience", model.Audience.Trim());
        });
    }

    public async Task UpdateAnnouncementAsync(int announcementId, AnnouncementInputModel model)
    {
        const string sql = """
            UPDATE Announcements
            SET Title = @Title,
                Category = @Category,
                Summary = @Summary,
                PublishedOn = @PublishedOn,
                Audience = @Audience
            WHERE AnnouncementId = @AnnouncementId;
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@AnnouncementId", announcementId);
            command.Parameters.AddWithValue("@Title", model.Title.Trim());
            command.Parameters.AddWithValue("@Category", model.Category.Trim());
            command.Parameters.AddWithValue("@Summary", model.Summary.Trim());
            command.Parameters.AddWithValue("@PublishedOn", model.PublishedOn.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Audience", model.Audience.Trim());
        });
    }

    public async Task<List<Resident>> GetResidentsAsync()
    {
        const string sql = """
            SELECT ResidentId, FullName, HouseholdNo, ContactNumber, EmailAddress, Purok, RegisteredOn
            FROM Residents
            ORDER BY FullName;
            """;

        return await ExecuteReaderAsync(sql, null, MapResident);
    }

    public async Task<Resident?> GetResidentByIdAsync(int residentId)
    {
        const string sql = """
            SELECT ResidentId, FullName, HouseholdNo, ContactNumber, EmailAddress, Purok, RegisteredOn
            FROM Residents
            WHERE ResidentId = @ResidentId
            LIMIT 1;
            """;

        return await ExecuteSingleAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@ResidentId", residentId);
        }, MapResident);
    }

    public async Task<int> AddResidentAsync(ResidentInputModel model)
    {
        const string sql = """
            INSERT INTO Residents (FullName, HouseholdNo, ContactNumber, EmailAddress, Purok, RegisteredOn)
            VALUES (@FullName, @HouseholdNo, @ContactNumber, @EmailAddress, @Purok, @RegisteredOn);
            SELECT last_insert_rowid();
            """;

        return await ExecuteScalarIntAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@FullName", model.FullName);
            command.Parameters.AddWithValue("@HouseholdNo", model.HouseholdNo);
            command.Parameters.AddWithValue("@ContactNumber", model.ContactNumber);
            command.Parameters.AddWithValue("@EmailAddress", model.EmailAddress);
            command.Parameters.AddWithValue("@Purok", model.Purok);
            command.Parameters.AddWithValue("@RegisteredOn", DateTime.Today.ToString("yyyy-MM-dd"));
        });
    }

    public async Task UpdateResidentAsync(int residentId, ResidentInputModel model)
    {
        const string sql = """
            UPDATE Residents
            SET FullName = @FullName,
                HouseholdNo = @HouseholdNo,
                ContactNumber = @ContactNumber,
                EmailAddress = @EmailAddress,
                Purok = @Purok
            WHERE ResidentId = @ResidentId;
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@ResidentId", residentId);
            command.Parameters.AddWithValue("@FullName", model.FullName.Trim());
            command.Parameters.AddWithValue("@HouseholdNo", model.HouseholdNo.Trim());
            command.Parameters.AddWithValue("@ContactNumber", model.ContactNumber.Trim());
            command.Parameters.AddWithValue("@EmailAddress", model.EmailAddress.Trim());
            command.Parameters.AddWithValue("@Purok", model.Purok.Trim());
        });
    }

    public async Task<bool> DeleteResidentAsync(int residentId)
    {
        const string sql = """
            DELETE FROM Residents
            WHERE ResidentId = @ResidentId
              AND NOT EXISTS (SELECT 1 FROM Accounts WHERE ResidentId = @ResidentId)
              AND NOT EXISTS (SELECT 1 FROM Appointments WHERE ResidentId = @ResidentId)
              AND NOT EXISTS (SELECT 1 FROM ServiceRequests WHERE ResidentId = @ResidentId);
            """;

        return await ExecuteNonQueryWithResultAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@ResidentId", residentId);
        }) > 0;
    }

    public async Task AssignResidentToAccountAsync(int accountId, int residentId)
    {
        const string sql = """
            UPDATE Accounts
            SET ResidentId = @ResidentId
            WHERE AccountId = @AccountId;
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@ResidentId", residentId);
            command.Parameters.AddWithValue("@AccountId", accountId);
        });
    }

    public async Task<List<Service>> GetServicesAsync()
    {
        const string sql = """
            SELECT ServiceId, Name, Office, Description, Schedule, Requirements
            FROM Services
            ORDER BY Name;
            """;

        return await ExecuteReaderAsync(sql, null, reader => new Service
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Office = reader.GetString(2),
            Description = reader.GetString(3),
            Schedule = reader.GetString(4),
            Requirements = reader.GetString(5)
        });
    }

    public async Task AddServiceAsync(ServiceInputModel model)
    {
        const string sql = """
            INSERT INTO Services (Name, Office, Description, Schedule, Requirements)
            VALUES (@Name, @Office, @Description, @Schedule, @Requirements);
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@Name", model.Name);
            command.Parameters.AddWithValue("@Office", model.Office);
            command.Parameters.AddWithValue("@Description", model.Description);
            command.Parameters.AddWithValue("@Schedule", model.Schedule);
            command.Parameters.AddWithValue("@Requirements", model.Requirements);
        });
    }

    public async Task UpdateServiceAsync(int serviceId, ServiceInputModel model)
    {
        const string sql = """
            UPDATE Services
            SET Name = @Name,
                Office = @Office,
                Description = @Description,
                Schedule = @Schedule,
                Requirements = @Requirements
            WHERE ServiceId = @ServiceId;
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@ServiceId", serviceId);
            command.Parameters.AddWithValue("@Name", model.Name.Trim());
            command.Parameters.AddWithValue("@Office", model.Office.Trim());
            command.Parameters.AddWithValue("@Description", model.Description.Trim());
            command.Parameters.AddWithValue("@Schedule", model.Schedule.Trim());
            command.Parameters.AddWithValue("@Requirements", model.Requirements.Trim());
        });
    }

    public async Task<bool> DeleteServiceAsync(int serviceId)
    {
        const string sql = """
            DELETE FROM Services
            WHERE ServiceId = @ServiceId
              AND NOT EXISTS (SELECT 1 FROM Appointments WHERE ServiceId = @ServiceId)
              AND NOT EXISTS (SELECT 1 FROM ServiceRequests WHERE ServiceId = @ServiceId);
            """;

        return await ExecuteNonQueryWithResultAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@ServiceId", serviceId);
        }) > 0;
    }

    public async Task<List<Appointment>> GetAppointmentsAsync(int? residentId = null)
    {
        var sql = """
            SELECT a.AppointmentId, a.ResidentId, r.FullName, s.Name, a.AppointmentDate, a.TimeSlot, a.Status, a.Notes
            FROM Appointments a
            INNER JOIN Residents r ON r.ResidentId = a.ResidentId
            INNER JOIN Services s ON s.ServiceId = a.ServiceId
            """;

        if (residentId.HasValue)
        {
            sql += "\nWHERE a.ResidentId = @ResidentId";
        }

        sql += """

            ORDER BY date(a.AppointmentDate), a.TimeSlot
            LIMIT 8;
            """;

        return await ExecuteReaderAsync(sql, command =>
        {
            if (residentId.HasValue)
            {
                command.Parameters.AddWithValue("@ResidentId", residentId.Value);
            }
        }, reader => new Appointment
        {
            Id = reader.GetInt32(0),
            ResidentId = reader.GetInt32(1),
            ResidentName = reader.GetString(2),
            ServiceName = reader.GetString(3),
            AppointmentDate = DateTime.Parse(reader.GetString(4)),
            TimeSlot = reader.GetString(5),
            Status = reader.GetString(6),
            Notes = reader.GetString(7)
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

    public async Task UpdateAppointmentStatusAsync(int appointmentId, string status)
    {
        const string sql = """
            UPDATE Appointments
            SET Status = @Status
            WHERE AppointmentId = @AppointmentId;
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
            command.Parameters.AddWithValue("@Status", status);
        });
    }

    public async Task<bool> DeleteClosedAppointmentAsync(int appointmentId)
    {
        const string sql = """
            DELETE FROM Appointments
            WHERE AppointmentId = @AppointmentId
              AND Status IN ('Completed', 'Cancelled');
            """;

        return await ExecuteNonQueryWithResultAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
        }) > 0;
    }

    public async Task<List<ServiceRequest>> GetServiceRequestsAsync(int? residentId = null)
    {
        var sql = """
            SELECT sr.RequestId, sr.ResidentId, r.FullName, s.Name, sr.Description, sr.Priority, sr.Status, sr.SubmittedOn
            FROM ServiceRequests sr
            INNER JOIN Residents r ON r.ResidentId = sr.ResidentId
            INNER JOIN Services s ON s.ServiceId = sr.ServiceId
            """;

        if (residentId.HasValue)
        {
            sql += "\nWHERE sr.ResidentId = @ResidentId";
        }

        sql += """

            ORDER BY date(sr.SubmittedOn) DESC, sr.RequestId DESC
            LIMIT 8;
            """;

        return await ExecuteReaderAsync(sql, command =>
        {
            if (residentId.HasValue)
            {
                command.Parameters.AddWithValue("@ResidentId", residentId.Value);
            }
        }, reader => new ServiceRequest
        {
            Id = reader.GetInt32(0),
            ResidentId = reader.GetInt32(1),
            ResidentName = reader.GetString(2),
            ServiceName = reader.GetString(3),
            Description = reader.GetString(4),
            Priority = reader.GetString(5),
            Status = reader.GetString(6),
            SubmittedOn = DateTime.Parse(reader.GetString(7))
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

    public async Task UpdateServiceRequestAsync(int requestId, string priority, string status)
    {
        const string sql = """
            UPDATE ServiceRequests
            SET Priority = @Priority,
                Status = @Status
            WHERE RequestId = @RequestId;
            """;

        await ExecuteNonQueryAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@RequestId", requestId);
            command.Parameters.AddWithValue("@Priority", priority);
            command.Parameters.AddWithValue("@Status", status);
        });
    }

    public async Task<bool> DeleteCompletedServiceRequestAsync(int requestId)
    {
        const string sql = """
            DELETE FROM ServiceRequests
            WHERE RequestId = @RequestId
              AND Status = 'Completed';
            """;

        return await ExecuteNonQueryWithResultAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@RequestId", requestId);
        }) > 0;
    }

    private async Task SeedReferenceDataAsync(SqliteConnection connection)
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

    private async Task SeedAccountsAsync(SqliteConnection connection)
    {
        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Accounts;";
        var existingAccounts = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

        if (existingAccounts > 0)
        {
            return;
        }

        const string seedSql = """
            INSERT INTO Accounts (Username, Password, DisplayName, Role, ResidentId) VALUES
            ('admin', 'admin123', 'Barangay Administrator', 'Admin', NULL),
            ('ana.delacruz', 'user123', 'Ana Dela Cruz', 'User', (SELECT ResidentId FROM Residents WHERE FullName = 'Ana Dela Cruz' LIMIT 1)),
            ('miguel.santos', 'user123', 'Miguel Santos', 'User', (SELECT ResidentId FROM Residents WHERE FullName = 'Miguel Santos' LIMIT 1)),
            ('leah.ramos', 'user123', 'Leah Ramos', 'User', (SELECT ResidentId FROM Residents WHERE FullName = 'Leah Ramos' LIMIT 1));
            """;

        await using var seedCommand = connection.CreateCommand();
        seedCommand.CommandText = seedSql;
        await seedCommand.ExecuteNonQueryAsync();
    }

    private static Account MapAccount(SqliteDataReader reader)
    {
        return new Account
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            Password = reader.GetString(2),
            DisplayName = reader.GetString(3),
            Role = reader.GetString(4),
            ResidentId = reader.IsDBNull(5) ? null : reader.GetInt32(5)
        };
    }

    private static Resident MapResident(SqliteDataReader reader)
    {
        return new Resident
        {
            Id = reader.GetInt32(0),
            FullName = reader.GetString(1),
            HouseholdNo = reader.GetString(2),
            ContactNumber = reader.GetString(3),
            EmailAddress = reader.GetString(4),
            Purok = reader.GetString(5),
            RegisteredOn = DateTime.Parse(reader.GetString(6))
        };
    }

    private async Task<int> ExecuteScalarIntAsync(string sql, Action<SqliteCommand>? configure = null)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure?.Invoke(command);
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

    private async Task<int> ExecuteNonQueryWithResultAsync(string sql, Action<SqliteCommand>? configure = null)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure?.Invoke(command);
        return await command.ExecuteNonQueryAsync();
    }

    private async Task<T?> ExecuteSingleAsync<T>(string sql, Action<SqliteCommand>? configure, Func<SqliteDataReader, T> map)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure?.Invoke(command);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return map(reader);
        }

        return default;
    }

    private async Task<List<T>> ExecuteReaderAsync<T>(string sql, Action<SqliteCommand>? configure, Func<SqliteDataReader, T> map)
    {
        var items = new List<T>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure?.Invoke(command);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(map(reader));
        }

        return items;
    }
}
