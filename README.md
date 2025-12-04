# TravelCheck – Business Trip Management System

Backend system for registering and processing business trips, built with Clean Architecture and designed for cloud deployment on Microsoft Azure.

The application focuses on backend architecture, asynchronous workflows and cloud-native patterns rather than UI presentation.

---

## Overview

The solution is a backend-first system responsible for managing business trip data and controlling the full lifecycle of travel requests.

A future frontend (Angular / React) will act as a thin client consuming the API.

The main focus of the project is production-grade backend development: architecture, domain modeling, cloud services and background processing.

---

## Backend Features

- CRUD operations for business trips  
- Azure Cosmos DB persistence  
- Partition key support  
- Clean Architecture structure  
- Domain-driven design  
- Repository pattern  
- Dependency injection  
- REST API with ASP.NET Core  
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

Backend implemented using layered architecture:

- Domain layer – business rules and entities  
- Application layer – services and use cases  
- Infrastructure layer – Azure and persistence  
- API – HTTP interface  
- Worker (planned) – background execution  
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

             (Planned Extensions)
                   |
                   v
  Azure Service Bus → Worker Service → Status updates
  Azure Blob Storage → Attachments


    TravelCheck.Domain
 └─ Entities
 └─ Enums

TravelCheck.Application
 └─ Dtos
 └─ Interfaces
 └─ Services

TravelCheck.Infrastructure
 └─ Repositories

TravelCheck.Api
 └─ Controllers
 └─ Program.cs
 └─ appsettings.json

 Technologies
Backend

.NET 8

ASP.NET Core Web API

Azure Cosmos DB

Azure SDK

Clean Architecture

Dependency Injection

Repository Pattern

Swagger / OpenAPI

Planned

Azure Service Bus

Azure Blob Storage

Azure Functions / Worker Service

Azure Application Insights

Angular / React frontend


Configuration

File: TravelCheck.Api/appsettings.json

{
  "CosmosDb": {
    "AccountEndpoint": "https://YOUR-ACCOUNT.documents.azure.com:443/",
    "AccountKey": "YOUR-KEY",
    "DatabaseName": "TravelCheckDb",
    "ContainerName": "Trips"
  }
}

Local Startup


Backend

Configure Cosmos DB credentials

Run API:

dotnet run


Open Swagger:

/swagger


Roadmap
Infrastructure

Azure Blob Storage

Azure Service Bus

Worker Service / Azure Functions

Secrets management

Retry policies

Application Insights

Business Logic

Status transitions

Background execution

Event processing

Soft delete

History audit

Validation layer

Centralized error handling

Frontend

Angular / React

Real-time status

File upload

Forms and lists

Author

Maciek Darłak
