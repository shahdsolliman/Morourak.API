using System.Linq.Expressions;

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

        // Get a single entity matching a predicate, with optional Include expressions
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);


        // Add, update, remove
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}