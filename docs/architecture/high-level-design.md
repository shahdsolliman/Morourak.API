# High-Level Architecture Design

Morourak.API is built following the principles of **Clean Architecture** and **CQRS (Command Query Responsibility Segregation)**. This ensures that the system is scalable, testable, and maintainable.

## 🏛️ Project Layers

### 1. **Morourak.Domain**
The core of the system. Contains:
- **Entities**: Domain objects (e.g., `Payment`, `Appointment`, `DrivingLicense`).
- **Enums**: System-wide enums (e.g., `PaymentStatus`, `RequestStatus`).
- **Common**: Base classes and interfaces.

### 2. **Morourak.Application**
The business logic layer. Contains:
- **CQRS Commands/Queries**: Separation of write (Commands) and read (Queries) operations.
- **Interfaces**: Definitions for services, repositories, and unit of work.
- **DTOs**: Data Transfer Objects for input and output.
- **Mappings**: AutoMapper profiles.

### 3. **Morourak.Infrastructure**
Implementation details for external concerns. Contains:
- **Persistence**: EF Core implementation of repositories and Unit of Work.
- **Identity**: ASP.NET Core Identity integration for security.
- **Services**: Implementations of external integrations (Paymob, Redis).

### 4. **Morourak.API**
The entrance to the system. Contains:
- **Controllers**: Thin controllers that delegate work to MediatR.
- **Middleware**: Exception handling, logging, and performance monitoring.
- **Extensions**: Service registration and DI configuration.

## 🔄 The MediatR Workflow

1.  **Request**: The Controller receives an HTTP request and converts it into a MediatR `Command` or `Query`.
2.  **Pipeline**: The request passes through **Behaviors** (Caching, Invalidation, Validation, Logging).
3.  **Handler**: The dedicated `Handler` processes the logic using domain services.
4.  **Response**: The Handler returns a DTO which the Controller sends back to the user.

---
*For more details on caching, see the [Caching Strategy Guide](./caching-strategy.md).*
