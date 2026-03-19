# Local Setup & Environment Guide

This guide describes how to set up the **Morourak.API** development environment on a local machine.

## 🛠️ Prerequisites
- **.NET 8.0 SDK**
- **SQL Server** (Express or LocalDB)
- **Redis** (Optional: Running on Docker or as a Windows service)
- **Postman** (For API testing)

## 🏗️ Step-by-Step Setup

1.  **Clone the Repository**
    ```bash
    git clone https://github.com/shahdsolliman/Morourak.API.git
    cd Morourak.API
    ```

2.  **Configure `appsettings.json`**
    Update the following sections in `Morourak.API/appsettings.json`:
    - `ConnectionStrings:DefaultConnection`: Your SQL Server connection string.
    - `RedisSettings:ConnectionString`: e.g., `localhost:6379,abortConnect=false`.
    - `Paymob`: Add your sandbox credentials.

3.  **Database Migration**
    Apply the EF Core migrations to create the database schema:
    ```bash
    dotnet ef database update --project Morourak.Infrastructure --startup-project Morourak.API
    ```

4.  **Run the Application**
    ```bash
    dotnet run --project Morourak.API
    ```

5.  **Access Swagger**
    Open your browser to: `https://localhost:7021/swagger`

## 🐳 Running Redis with Docker
If you don't have Redis installed, use Docker:
```bash
docker run -d --name morourak-redis -p 6379:6379 redis
```

---
*For testing the system features, see the [Testing Guide](./testing-guide.md).*
