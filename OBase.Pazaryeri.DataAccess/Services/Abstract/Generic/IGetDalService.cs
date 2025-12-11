using OBase.Pazaryeri.Core.Abstract.Repository;
using System.Linq.Expressions;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Generic
{
	public interface IGetDalService
	{
		Task<T> GetAsync<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes) where T : class, IEntity, new();
		Task<List<T>> GetListAsync<T>(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes) where T : class, IEntity, new();
        Task<List<T>> GetListOrderAsync<T>(Expression<Func<T, bool>> filter = null, Expression<Func<T, dynamic>> order = null, bool descending = false) where T : class, IEntity, new();
        Task<List<T>> GetListOrderWithSkipTakeAsync<T>(Expression<Func<T, bool>> filter, Expression<Func<T, dynamic>> order, int skip, int take, bool descending = false) where T : class, IEntity, new();

		IQueryable<T> GetTable<T>(Expression<Func<T, bool>> filter = null) where T : class, IEntity, new();
	}
}
