-- BarangayConnect SQL statement reference
-- These are the SQL statements used by the application for schema creation,
-- data retrieval, and data insertion/update workflows.

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

SELECT COUNT(*) FROM Residents;
SELECT COUNT(*) FROM Appointments;
SELECT COUNT(*) FROM ServiceRequests WHERE Status <> 'Completed';
SELECT COUNT(*) FROM Announcements;

SELECT AnnouncementId, Title, Category, Summary, PublishedOn, Audience
FROM Announcements
ORDER BY date(PublishedOn) DESC
LIMIT 6;

SELECT ResidentId, FullName, HouseholdNo, ContactNumber, EmailAddress, Purok, RegisteredOn
FROM Residents
ORDER BY FullName;

INSERT INTO Residents (FullName, HouseholdNo, ContactNumber, EmailAddress, Purok, RegisteredOn)
VALUES (@FullName, @HouseholdNo, @ContactNumber, @EmailAddress, @Purok, @RegisteredOn);

SELECT ServiceId, Name, Office, Description, Schedule, Requirements
FROM Services
ORDER BY Name;

SELECT a.AppointmentId, r.FullName, s.Name, a.AppointmentDate, a.TimeSlot, a.Status, a.Notes
FROM Appointments a
INNER JOIN Residents r ON r.ResidentId = a.ResidentId
INNER JOIN Services s ON s.ServiceId = a.ServiceId
ORDER BY date(a.AppointmentDate), a.TimeSlot
LIMIT 8;

INSERT INTO Appointments (ResidentId, ServiceId, AppointmentDate, TimeSlot, Status, Notes)
VALUES (@ResidentId, @ServiceId, @AppointmentDate, @TimeSlot, 'Scheduled', @Notes);

SELECT sr.RequestId, r.FullName, s.Name, sr.Description, sr.Priority, sr.Status, sr.SubmittedOn
FROM ServiceRequests sr
INNER JOIN Residents r ON r.ResidentId = sr.ResidentId
INNER JOIN Services s ON s.ServiceId = sr.ServiceId
ORDER BY date(sr.SubmittedOn) DESC, sr.RequestId DESC
LIMIT 8;

INSERT INTO ServiceRequests (ResidentId, ServiceId, Description, Priority, Status, SubmittedOn)
VALUES (@ResidentId, @ServiceId, @Description, @Priority, 'Pending', @SubmittedOn);
