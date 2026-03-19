# Caching Strategy with Redis

Morourak.API uses a distributed caching strategy to optimize performance and reduce database load for read-heavy operations like querying appointments or service requests.

## 🛠️ Implementation Details

### 1. **ICacheService & RedisCacheService**
- An abstraction (`ICacheService`) is defined in the **Application** layer.
- The implementation (`RedisCacheService`) is in the **Infrastructure** layer using `StackExchange.Redis`.
- **Resilience**: The service includes try-catch blocks and connection monitoring. If Redis is down, the system gracefully falls back to the database.

### 2. **MediatR Pipeline Behaviors**
Caching logic is decoupled from business handlers using **Pipeline Behaviors**:

- **`CachingBehavior<TRequest, TResponse>`**:
    - Automatically intercepts calls to any `IRequest` that also implements `ICacheableRequest`.
    - Checks Redis for a cached response using a structured key (e.g., `user:{NationalId}:appointments:*`).
    - If found, returns the cached DTO.
    - If not found, executes the handler and caches the result.

- **`InvalidationBehavior<TRequest, TResponse>`**:
    - Automatically intercepts commands that implement `IInvalidateCacheRequest`.
    - Removes related cache patterns (e.g., when a new appointment is created, it clears the user's appointment list cache).

## 🔑 Cache Key Strategy

Keys are structured to ensure data isolation and easy invalidation:
- **Pattern**: `user:{NationalId}:{Feature}:{Params}`
- **Example**: `user:29001011234567:appointments:page:1:size:10`

---
*For more technical setup details, see the [Setup Guide](../guides/setup-guide.md).*
