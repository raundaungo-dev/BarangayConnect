# BarangayConnect

BarangayConnect is a full ASP.NET Core MVC web application for a local government or community office. It simulates a digital resident services portal where users can:

- view public announcements
- manage resident records
- browse available barangay services
- schedule service appointments
- submit service requests

The project uses an embedded SQLite database stored directly inside the source tree at `App_Data/barangayconnect.db`, which makes it easy to move, back up, inspect, and update without requiring a separate database server.

## Tech Stack

- ASP.NET Core MVC on `.NET 10`
- `Microsoft.Data.Sqlite`
- Bootstrap 5
- Raw SQL repository methods for transparent database access

## Run The Project

```powershell
dotnet restore
dotnet run
```

By default, the application initializes the SQLite database on startup and seeds sample data if the database is empty.

## Submission Files

The required project artifacts are stored in [`docs/`](/C:/Users/rjrms/Documents/New%20project/BarangayConnect/docs):

- [`Final-ERD.pdf`](/C:/Users/rjrms/Documents/New%20project/BarangayConnect/docs/Final-ERD.pdf)
- [`Final-ERD.jpg`](/C:/Users/rjrms/Documents/New%20project/BarangayConnect/docs/Final-ERD.jpg)
- [`Entity-Reference-Document.md`](/C:/Users/rjrms/Documents/New%20project/BarangayConnect/docs/Entity-Reference-Document.md)
- [`Application-SQL-Statements.sql`](/C:/Users/rjrms/Documents/New%20project/BarangayConnect/docs/Application-SQL-Statements.sql)
- [`IT114L-Project-Presentation.pptx`](/C:/Users/rjrms/Documents/New%20project/BarangayConnect/docs/IT114L-Project-Presentation.pptx)
