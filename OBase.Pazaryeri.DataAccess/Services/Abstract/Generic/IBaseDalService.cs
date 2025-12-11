using OBase.Pazaryeri.Core.Abstract.Repository;
using System.Linq.Expressions;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Generic
{
	public interface IBaseDalService
	{
		Task<T> GetAsync<T>(Expression<Func<T, bool>> filter) where T : class, IEntity, new();
		Task<List<T>> GetListAsync<T>(Expression<Func<T, bool>> filter = null) where T : class, IEntity, new();
        Task<T> AddAsync<T>(T entity) where T : class, IEntity, new();
        Task AddRangeAsync<T>(IEnumerable<T> entity) where T : class, IEntity, new();
        Task UpdateAsync<T>(T entity) where T : class, IEntity, new();
        Task UpdateRangeAsync<T>(IEnumerable<T> entity) where T : class, IEntity, new();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
