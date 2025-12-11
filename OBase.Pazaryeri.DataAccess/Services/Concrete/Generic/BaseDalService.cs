using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using System.Linq.Expressions;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Generic
{
    public class BaseDalService : IBaseDalService
	{
		protected readonly IRepository _repository;

		protected BaseDalService(IRepository repository)
		{
			_repository=repository;
		}
        public async Task<T> AddAsync<T>(T entity) where T : class, IEntity, new()
        {
            return await _repository.AddAsync(entity).ConfigureAwait(false);
        }
        public async Task AddRangeAsync<T>(IEnumerable<T> entity) where T : class, IEntity, new()
        {
            await _repository.AddRangeAsync(entity);
        }
        public async Task UpdateAsync<T>(T entity) where T : class, IEntity, new()
        {
            await _repository.UpdateAsync(entity);
        }
        public async Task UpdateRangeAsync<T>(IEnumerable<T> entity) where T : class, IEntity, new()
        {
            await _repository.UpdateRangeAsync(entity);
        }
        public async Task<T> GetAsync<T>(Expression<Func<T, bool>> filter) where T : class, IEntity, new()
		{
			return await _repository.GetTable<T>().FirstOrDefaultAsync(filter);
		}
		public async Task<List<T>> GetListAsync<T>(Expression<Func<T, bool>> filter = null) where T : class, IEntity, new()
		{

			return filter == null
			? await _repository.GetTable<T>().ToListAsync()
			: await _repository.GetTable<T>().Where(filter).ToListAsync();
		}
        public async Task BeginTransactionAsync()
        {
            await _repository.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync()
        {
            await _repository.CommitTransactionAsync();
        }
        public async Task RollbackTransactionAsync()
        {
            await _repository.RollbackTransactionAsync();
        }
	}
}