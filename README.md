# TravelCheck – Event-Driven Business Trip Processing System

Cloud-native, event-driven backend system for registering and processing business trips, built with Clean Architecture and Microsoft Azure.

The application focuses on asynchronous workflows, background processing and production-grade backend architecture rather than UI presentation.

---

### Overview

The solution is a backend-first system responsible for managing business trip data and controlling the full lifecycle of travel requests.

A future frontend (Angular / React) will act as a thin client consuming the API.

The main focus of the project is production-grade backend development: architecture, domain modeling, cloud services and background processing.

---

## Backend Features

- Event-driven backend architecture  
- Business trip lifecycle management  
- Clean Architecture (Domain / Application / Infrastructure)  
- Domain-first design  
- Azure Cosmos DB persistence  
  - Trips container (business state)
  - Outbox container (event storage)
- Outbox Pattern (reliable event publishing)  
- Azure Service Bus messaging  
- Background processing via Worker Services  
- Explicit trip status lifecycle:
  - New  
  - Processing  
  - Completed  
  - Rejected  
- Dependency Injection  
- Repository pattern  
- ASP.NET Core Web API  
- Swagger / OpenAPI documentation  
- Cloud-ready configuration  

Planned functionality:

- Background processing via Worker Service / Azure Functions  
- Message-based communication with Azure Service Bus  
- File storage using Azure Blob Storage  
- Centralized status handling:
  - New  
  - Processing  
  - Completed  
  - Rejected  
- Audit history and soft-delete  
- Error handling and retry policies  
- Application Insights integration  

---

## Architecture

Backend implemented using Clean Architecture and event-driven patterns:

- Domain – business rules, entities and state  
- Application – use cases, orchestration and business events  
- Infrastructure – Azure adapters (Cosmos DB, Service Bus)  
- API – HTTP entry point  
- Worker – background processing and event consumers  
- Frontend (planned) – Angular / React UI  

High-level architecture diagram:

```text
                     +-----------------------------+
                     |           FRONTEND          |
                     |        Angular / React      |
                     |                             |
                     |     GET /api/trips          |
                     |     POST /api/trips         |
                     +--------------+--------------+
                                    |
                                    | HTTP / JSON
                                    v

                     +--------------------------------------+
                     |                 API                  |
                     |          TripsController             |
                     |                                      |
                     |  → CreateTrip()                      |
                     |  → GetTrips()                        |
                     |  → UpdateTrip()                      |
                     |  → DeleteTrip()                      |
                     +------------------+------------------+
                                        |
                                        | delegates
                                        v

                     +--------------------------------------+
                     |           APPLICATION LAYER          |
                     |                                      |
                     |  TripService                         |
                     |    → business rules                  |
                     |    → state management                |
                     +------------------+------------------+
                                        |
                                        | uses
                                        v

                     +--------------------------------------+
                     |        INFRASTRUCTURE LAYER          |
                     |                                      |
                     |      CosmosTripRepository            |
                     |                                      |
                     |    Create / Update / Query           |
                     +------------------+------------------+
                                        |
                                        | Azure SDK
                                        v

                     +--------------------------------------+
                     |            AZURE COSMOS DB           |
                     |                                      |
                     |          Trips container             |
                     |        Partition Key: Country        |
                     +--------------------------------------+

                         (Asynchronous Processing)
                                       |
                                       v
  Azure Service Bus → Worker Services → Trip status updates
  Outbox Pattern → Reliable message publishing
  Azure Blob Storage (planned) → Attachments


```

## Solution Structure

### TravelCheck.Domain
- Entities
- Enums

### TravelCheck.Application
- Dtos
- Interfaces
- Application services
- Business events

### TravelCheck.Infrastructure
- Repositories

### TravelCheck.Api
- Controllers
- Program.cs
- appsettings.json


## Technologies

### Backend
- .NET 8
- ASP.NET Core Web API
- Azure Cosmos DB
- Azure SDK
- Clean Architecture
- Dependency Injection
- Repository Pattern
- Swagger / OpenAPI

### Planned
- Azure Blob Storage
- Azure Application Insights
- Retry and dead-letter handling
- Authentication (Azure AD)
- Angular / React frontend


## Configuration

**File:** TravelCheck.Api/appsettings.json

Cosmos DB configuration:

- AccountEndpoint
- AccountKey
- DatabaseName = TravelCheckDb
- ContainerName = Trips


## Local Startup

### Backend

1. Configure Cosmos DB credentials
2. Run API:
   - dotnet run
3. Open Swagger:
   - /swagger


## Roadmap

### Infrastructure
- Retry and dead-letter handling
- Application Insights telemetry
- Secrets management
- Azure Blob Storage

### Business Logic
- Status transitions
- Background execution
- Event processing
- Soft delete
- History audit
- Validation layer
- Centralized error handling

### Frontend
- Angular / React
- Real-time status
- File upload
- Forms and lists


## Author

Maciek Darłak

