# Morourak API

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Overview

**Morourak API** is a comprehensive backend solution for digitalizing traffic services. It provides a robust set of features for citizens to manage their driving and vehicle licenses, book appointments at traffic units, and pay traffic violations online. The system aims to streamline administrative processes and reduce physical queues at traffic departments.

## Features

- **🛡️ Secure Authentication**: JWT-based authentication with role-based access control (Citizen, Staff, Admin).
- **🪪 Driving License Management**: Digital workflows for issuing, renewing, and replacing lost or damaged driving licenses.
- **🚗 Vehicle License Management**: Comprehensive vehicle services including license renewal, technical inspections, and document uploads.
- **📅 Smart Appointment System**: Real-time booking for medical examinations and technical tests across various traffic units and governorates.
- **💳 Payment Integration**: Fully integrated with **Paymob Payment Gateway** for secure online transactions.
- **⚖️ Traffic Violations**: Query outstanding violations by license number and process immediate online payments.
- **📈 Admin & Staff Dashboards**: Specialized endpoints for managing users, approving requests, and updating reference data.

## Architecture

The project follows a **Clean Architecture** (Layered) pattern to ensure separation of concerns, maintainability, and testability.

- **Morourak.API**: The presentation layer containing Controllers, API-specific DTOs, and Middleware.
- **Morourak.Application**: The business logic layer containing Services, Application DTOs, Interfaces, and AutoMapper profiles.
- **Morourak.Domain**: The core layer containing Entities, Enums, and Constants. This layer has zero dependencies on other projects.
- **Morourak.Infrastructure**: Implementation of data persistence (EF Core + SQL Server), Identity Management, and external service clients (Paymob).

## Technologies Used

- **Framework**: .NET 8.0
- **Language**: C# 12
- **Persistence**: Entity Framework Core 8.0
- **Database**: SQL Server
- **Documentation**: Swagger / OpenAPI (Swashbuckle)
- **Integration**: Paymob Payment API
- **Tooling**: AutoMapper, FluentValidation, System.Text.Json

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or higher)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or VS Code

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-username/Morourak.API.git
   cd Morourak.API
   ```

2. **Configure the database**:
   Update the `DefaultConnection` string in `Morourak.API/appsettings.json` to point to your SQL Server instance.

3. **Apply Migrations**:
   Run the following command in the root directory:
   ```bash
   dotnet ef database update --project Morourak.Infrastructure --startup-project Morourak.API
   ```

4. **Run the project**:
   ```bash
   dotnet run --project Morourak.API
   ```

## API Documentation

The API includes comprehensive **Swagger** documentation. Once the project is running, you can access the interactive documentation at:

👉 [https://localhost:7021/swagger/index.html](https://localhost:7021/swagger/index.html) *(Port may vary)*

Every endpoint is documented with its purpose, request body schema, and possible HTTP response codes.

## Environment Configuration

Configuration is managed via `appsettings.json`. Key settings include:

- **ConnectionStrings**: SQL Server connection settings.
- **JwtSettings**: Token secret key and expiration duration.
- **Paymob**: Sandbox/Live API keys and integration IDs.

> [!IMPORTANT]
> For production environments, never commit sensitive keys to the repository. Use **Environment Variables** or **Azure Key Vault**.

## Error Handling

The system uses a centralized error handling strategy with structured exceptions. Validation errors are returned using `AppEx.ValidationException`, which provides clear error codes and localized messages.

## Contributing

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the MIT License. See `LICENSE` for more information.
