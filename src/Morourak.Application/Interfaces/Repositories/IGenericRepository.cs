using System.Linq.Expressions;
using Morourak.Application.Common;

namespace Morourak.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // Get by Id with optional Include expressions
        Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

        // Get all entities with optional Include expressions
        Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

        // Find entities matching a predicate, with optional Include expressions
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

        // Find entities matching a predicate and return a paged result.
        // Caller must provide a deterministic ordering via the orderBy delegate.
        Task<PagedResult<T>> FindPagedAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            int pageNumber,
            int pageSize,
            params Expression<Func<T, object>>[] includes);

        // Same as FindPagedAsync but allows server-side projection (Select) before pagination materialization.
        Task<PagedResult<TProjection>> FindPagedAsync<TProjection>(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            Expression<Func<T, TProjection>> selector,
            int pageNumber,
            int pageSize,
            params Expression<Func<T, object>>[] includes);

        // Get a single entity matching a predicate, with optional Include expressions
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);


        // Add, update, remove
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}