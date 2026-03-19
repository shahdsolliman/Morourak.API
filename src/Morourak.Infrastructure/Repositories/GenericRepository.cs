using Microsoft.EntityFrameworkCore;
using Morourak.Application.Interfaces.Repositories;
using Morourak.Infrastructure.Persistence;
using Morourak.Infrastructure.Extensions;
using System.Linq.Expressions;

namespace Morourak.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly PersistenceDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(PersistenceDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();
            foreach (var include in includes)
                query = query.Include(include);

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();
            foreach (var include in includes)
                query = query.Include(include);

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsNoTracking().Where(predicate);
            foreach (var include in includes)
                query = query.Include(include);

            return await query.ToListAsync();
        }

        public async Task<Morourak.Application.Common.PagedResult<T>> FindPagedAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            int pageNumber,
            int pageSize,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsNoTracking().Where(predicate);
            foreach (var include in includes)
                query = query.Include(include);

            query = orderBy(query);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<Morourak.Application.Common.PagedResult<TProjection>> FindPagedAsync<TProjection>(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            Expression<Func<T, TProjection>> selector,
            int pageNumber,
            int pageSize,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsNoTracking().Where(predicate);
            foreach (var include in includes)
                query = query.Include(include);

            query = orderBy(query);

            return await query
                .Select(selector)
                .ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsNoTracking().Where(predicate);
            foreach (var include in includes)
                query = query.Include(include);

            return await query.FirstOrDefaultAsync();
        }


        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Remove(T entity) => _dbSet.Remove(entity);
    }
}