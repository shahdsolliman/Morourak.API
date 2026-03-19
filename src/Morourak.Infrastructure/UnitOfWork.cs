using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Repositories;
using Morourak.Infrastructure.Persistence;
using Morourak.Infrastructure.Repositories;
using System.Collections;

namespace Morourak.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PersistenceDbContext _context;
        private Hashtable _repositories;

        public UnitOfWork(PersistenceDbContext context)
        {
            _context = context;
            _repositories = new Hashtable();
        }

        public IGenericRepository<TEntity> Repository<TEntity>()
            where TEntity : class
        {
            var type = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(
                    repositoryType.MakeGenericType(typeof(TEntity)),
                    _context
                );

                _repositories.Add(type, repositoryInstance!);
            }

            return (IGenericRepository<TEntity>)_repositories[type]!;
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
                await _context.Database.CurrentTransaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
                await _context.Database.CurrentTransaction.RollbackAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}