TravelCheck — Business Trip Management System (Backend-first, Azure-ready)

TravelCheck is a production-style backend system for managing business trips, built with clean architecture principles and cloud-native design in mind.

The goal of this project is not to deliver a UI showcase, but to demonstrate real enterprise backend engineering: domain modeling, infrastructure separation, asynchronous processing, and Azure service integration.

Purpose

TravelCheck allows employees and administrators to manage business travel data and provides:

persistent storage using Azure Cosmos DB

background job processing via Worker Service / Azure Functions

event-based communication using Service Bus

future file upload support with Blob Storage

centralized trip status handling (New → Processing → Completed / Rejected)

scalable, cloud-ready architecture

This project is intended as a portfolio-grade backend application and learning platform for cloud-native development.

Architecture

The solution follows Clean Architecture:

API → Application → Domain
↓
Infrastructure (Cosmos DB / Azure SDK)

Each layer has a single responsibility and communicates through abstractions.

Solution Structure

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

Current Features

Clean Architecture

Domain-driven model

CRUD API

Azure Cosmos DB integration

Partition keys

Dependency Injection

Repository Pattern

Async operations

Swagger / OpenAPI

Cloud-ready configuration

Azure SDK usage

Roadmap
Infrastructure

Azure Blob Storage (file upload)

Azure Service Bus (messaging)

Worker Service / Azure Functions

Secrets management

Retry policies

Application Insights

Business Logic

Status lifecycle management

Background processing

History tracking

Soft delete

Validation

Error handling middleware

Frontend

Angular / React

Trip forms

Trip list and details

File uploads

Real-time status updates

Configuration

Configuration file:

TravelCheck.Api/appsettings.json

Example:

{
"CosmosDb": {
"AccountEndpoint": "https://YOUR-ACCOUNT.documents.azure.com:443/
",
"AccountKey": "YOUR-KEY",
"DatabaseName": "TravelCheckDb",
"ContainerName": "Trips"
}
}

Run locally

Set Cosmos DB credentials in appsettings.json

Run TravelCheck.Api

Open in browser: /swagger

Tech Stack

.NET 8

ASP.NET Core Web API

Azure Cosmos DB

Azure SDK

Clean Architecture

Repository Pattern

Dependency Injection

Swagger

Author

Maciek Darlak
