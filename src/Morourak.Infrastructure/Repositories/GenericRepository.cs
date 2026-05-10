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

        /// <summary>
        /// Ensures the entity is tracked by the context. 
        /// If another instance with the same ID is already tracked, it returns that instance.
        /// Otherwise, it attaches the provided entity and returns it.
        /// </summary>
        public T Track(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State != EntityState.Detached)
                return entity;

            var primaryKey = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
            if (primaryKey == null)
            {
                _dbSet.Attach(entity);
                return entity;
            }

            var keyValues = primaryKey.Properties
                .Select(p => entry.Property(p.Name).CurrentValue)
                .ToArray();

            // If any key is default (e.g. 0 for int), it's a new entity, so we just attach it (or let Update handle it)
            if (keyValues.Any(v => v == null || v.Equals(GetDefault(v.GetType()))))
            {
                _dbSet.Attach(entity);
                return entity;
            }

            var trackedEntity = _dbSet.Local.FirstOrDefault(e =>
            {
                var eEntry = _context.Entry(e);
                return primaryKey.Properties.All(p =>
                    eEntry.Property(p.Name).CurrentValue?.Equals(entry.Property(p.Name).CurrentValue) == true);
            });

            if (trackedEntity != null)
                return trackedEntity;

            _dbSet.Attach(entity);
            return entity;
        }

        public void Update(T entity)
        {
            var entry = _context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                // Identity Unification: Check if another instance is already tracked
                var primaryKey = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
                if (primaryKey != null)
                {
                    var keyValues = primaryKey.Properties
                        .Select(p => entry.Property(p.Name).CurrentValue)
                        .ToArray();

                    // Only unify for existing entities (non-default keys)
                    if (keyValues.All(v => v != null && !v.Equals(GetDefault(v.GetType()))))
                    {
                        var trackedEntity = _dbSet.Local.FirstOrDefault(e =>
                        {
                            var eEntry = _context.Entry(e);
                            return primaryKey.Properties.All(p =>
                                eEntry.Property(p.Name).CurrentValue?.Equals(entry.Property(p.Name).CurrentValue) == true);
                        });

                        if (trackedEntity != null)
                        {
                            if (!ReferenceEquals(trackedEntity, entity))
                            {
                                // Sync values to the already tracked instance
                                _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
                            }
                            
                            if (_context.Entry(trackedEntity).State == EntityState.Unchanged)
                                _context.Entry(trackedEntity).State = EntityState.Modified;
                                
                            return;
                        }
                    }
                }

                _dbSet.Update(entity);
            }
            else if (entry.State == EntityState.Unchanged)
            {
                entry.State = EntityState.Modified;
            }
        }

        public void Remove(T entity) => _dbSet.Remove(entity);

        private static object? GetDefault(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}