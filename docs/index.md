# Morourak API Documentation

Welcome to the official documentation for the **Morourak API**. This documentation provides in-depth technical details about the system's architecture, API endpoints, and integration guides.

## 📂 Structure

### [🏛️ Architecture](./architecture/high-level-design.md)
Detailed look into the Clean Architecture, CQRS patterns, and state management used in the project.

### [📡 API Reference](./api/auth.md)
Comprehensive specifications for all available endpoints, grouped by module:
- [Authentication & Identity](./api/auth.md)
- [Licenses (Driving & Vehicle)](./api/licenses.md)
- [Appointments](./api/appointments.md)
- [Payments & Webhooks](./api/payments.md)

### [⚡ Performance & Caching](./architecture/caching-strategy.md)
How we use Redis and MediatR Pipeline Behaviors to achieve high performance and scalability.

### [🛠️ Guides](./guides/setup-guide.md)
Step-by-step instructions for developers:
- [Local Setup & Environment](./guides/setup-guide.md)
- [Testing & Webhook Simulation](./guides/testing-guide.md)

---
*For a quick start, please refer to the main [README.md](../README.md) in the root directory.*
