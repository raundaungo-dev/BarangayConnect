# Entity Reference Document

## 1. Residents

| Attribute | Data Type | Description | Key |
| --- | --- | --- | --- |
| `ResidentId` | `INTEGER` | Unique resident identifier | Primary Key |
| `FullName` | `TEXT` | Full resident name | |
| `HouseholdNo` | `TEXT` | Household reference number | |
| `ContactNumber` | `TEXT` | Mobile or contact number | |
| `EmailAddress` | `TEXT` | Resident email address | |
| `Purok` | `TEXT` | Zone, purok, or neighborhood | |
| `RegisteredOn` | `TEXT` | Resident registration date | |

## 2. Services

| Attribute | Data Type | Description | Key |
| --- | --- | --- | --- |
| `ServiceId` | `INTEGER` | Unique service identifier | Primary Key |
| `Name` | `TEXT` | Name of barangay service | |
| `Office` | `TEXT` | Responsible office or desk | |
| `Description` | `TEXT` | Service description | |
| `Schedule` | `TEXT` | Office service schedule | |
| `Requirements` | `TEXT` | Documentary or processing requirements | |

## 3. Announcements

| Attribute | Data Type | Description | Key |
| --- | --- | --- | --- |
| `AnnouncementId` | `INTEGER` | Unique announcement identifier | Primary Key |
| `Title` | `TEXT` | Announcement title | |
| `Category` | `TEXT` | Community, health, education, etc. | |
| `Summary` | `TEXT` | Short public notice content | |
| `PublishedOn` | `TEXT` | Publication date | |
| `Audience` | `TEXT` | Intended resident audience | |

## 4. Appointments

| Attribute | Data Type | Description | Key |
| --- | --- | --- | --- |
| `AppointmentId` | `INTEGER` | Unique appointment identifier | Primary Key |
| `ResidentId` | `INTEGER` | Resident who booked the appointment | Foreign Key -> `Residents.ResidentId` |
| `ServiceId` | `INTEGER` | Service requested for the visit | Foreign Key -> `Services.ServiceId` |
| `AppointmentDate` | `TEXT` | Scheduled appointment date | |
| `TimeSlot` | `TEXT` | Time range for the visit | |
| `Status` | `TEXT` | Appointment state such as Scheduled | |
| `Notes` | `TEXT` | Appointment reason or notes | |

## 5. ServiceRequests

| Attribute | Data Type | Description | Key |
| --- | --- | --- | --- |
| `RequestId` | `INTEGER` | Unique service request identifier | Primary Key |
| `ResidentId` | `INTEGER` | Resident who submitted the request | Foreign Key -> `Residents.ResidentId` |
| `ServiceId` | `INTEGER` | Requested service | Foreign Key -> `Services.ServiceId` |
| `Description` | `TEXT` | Request details | |
| `Priority` | `TEXT` | Priority level such as Normal, High, Urgent | |
| `Status` | `TEXT` | Request processing status | |
| `SubmittedOn` | `TEXT` | Submission date | |

## Relationship Summary

- One `Resident` can have many `Appointments`.
- One `Service` can have many `Appointments`.
- One `Resident` can have many `ServiceRequests`.
- One `Service` can have many `ServiceRequests`.
- `Announcements` are standalone informational records.
