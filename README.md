TravelCheck – Business Trip Management System

Backend system for registering and processing business trips, built with Clean Architecture and designed for cloud deployment on Microsoft Azure.

The application focuses on backend architecture, asynchronous processing and cloud-native patterns rather than UI presentation.

Overview

The solution consists of backend services responsible for registering business trips, processing them in the background and managing their lifecycle.

Planned frontend (Angular / React) will act as a thin client consuming the API.

Current scope covers full backend architecture and Azure integration.

Backend Features

Business trip registration (CRUD)

Azure Cosmos DB persistence

Domain-driven design

Background processing with Worker Service / Azure Functions (planned)

Message-based communication using Azure Service Bus (planned)

File uploads using Azure Blob Storage (planned)

Trip lifecycle management:

New

Processing

Completed

Rejected

Clean Architecture separation

REST API using ASP.NET Core

Swagger / OpenAPI

Cloud-ready configuration

Partition key support

Repository pattern

Dependency injection

Architecture

Backend implemented using Clean Architecture:

Domain – entities and business logic

Application – services, use cases

Infrastructure – Cosmos DB repository and Azure SDK

API – HTTP layer

Worker (planned) – background processing

Frontend (planned) – Angular / React client

High-level architecture:

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
                     |  → CreateAsync()                     |
                     |  → GetAllAsync()                     |
                     |  → UpdateAsync()                     |
                     |  → DeleteAsync()                     |
                     +------------------+------------------+
                                        |
                                        | delegates
                                        v

                     +--------------------------------------+
                     |           APPLICATION LAYER          |
                     |                                      |
                     |  TripService                         |
                     |    → Create trip                     |
                     |    → Update trip                     |
                     |    → Manage state                    |
                     +------------------+------------------+
                                        |
                                        | uses
                                        v

                     +--------------------------------------+
                     |          INFRASTRUCTURE LAYER        |
                     |                                      |
                     |    CosmosTripRepository              |
                     |                                      |
                     |   CreateItemAsync()                  |
                     |   ReplaceItemAsync()                 |
                     |   QueryIterator                      |
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
    Azure Service Bus → Worker Service → Status Updates
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
