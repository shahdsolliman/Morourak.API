# Morourak API

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![CQRS](https://img.shields.io/badge/Architecture-CQRS-blue.svg)](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
[![Redis](https://img.shields.io/badge/Cache-Redis-red.svg)](https://redis.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 🌟 Overview

**Morourak API** is a high-performance, production-grade .NET 8 Web API designed to digitalize traffic services. Built with **Clean Architecture** and **CQRS**, it provides a scalable backend for managing licenses, appointments, and payments.

## 🚀 Key Features

- **🛡️ Secure Authentication**: JWT-based auth with Role-Based Access Control (RBAC).
- **🪪 License Lifecycle**: End-to-end workflows for Issuance, Renewal, and Replacement.
- **📅 Smart Appointments**: Advanced booking system with Arabic localization and traffic unit specific metadata.
- **💳 Payment Integration**: Robust **Paymob** gateway integration with atomic transactions and webhook idempotency.
- **⚡ Performance Caching**: Distributed **Redis** caching using MediatR Pipeline Behaviors for read-heavy queries.
- **⚖️ Violation Management**: Real-time query and payment of traffic fines.
- **🌐 Localization**: Full support for Arabic date/time formatting and localized validation messages.

## 🏗️ Architecture & Design Patterns

The project is architected to be resilient, maintainable, and highly decoupled:

- **Clean Architecture**: Domain-centric design with clear separation between API, Application, Domain, and Infrastructure layers.
- **CQRS (MediatR)**: Uses Command Query Responsibility Segregation to separate read and write operations.
- **Pipeline Behaviors**: Cross-cutting concerns like **Caching**, **Validation**, and **Logging** are handled via MediatR pipelines.
- **Unit of Work & Repository**: Abstracted data access to ensure atomic transactions and testability.
- **Idempotency**: Webhook and payment processing logic designed to handle duplicate deliveries gracefully.

## 🛠️ Technology Stack

- **Framework**: .NET 8.0 (C# 12)
- **Persistence**: EF Core 8 (SQL Server)
- **Caching**: StackExchange.Redis (Distributed Cache)
- **Messaging**: MediatR (In-process mediator)
- **Integration**: Paymob Payment API
- **Tooling**: AutoMapper, FluentValidation, Serilog

## ⚙️ Configuration

### Redis Settings
The system includes built-in resilience for Redis. If Redis is unavailable, the application continues to function by falling back to the database.
```json
"RedisSettings": {
  "ConnectionString": "localhost:6379,abortConnect=false",
  "DefaultExpirationMinutes": 60
}
```

### Paymob Settings
Supports Sandbox and Production environments with HMAC signature validation for secure callbacks.

## 🚦 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server
- Redis (Optional but recommended for performance)

### Fast Track
1. **Update Connection Strings** in `appsettings.json`.
2. **Apply Migrations**:
   ```bash
   dotnet ef database update --project Morourak.Infrastructure --startup-project Morourak.API
   ```
3. **Run Application**:
   ```bash
   dotnet run --project Morourak.API
   ```

## 📖 Documentation
Interactive Swagger documentation is available at:
`https://localhost:7021/swagger`

---
*Developed with focus on Clean Code and SOLID principles.*
