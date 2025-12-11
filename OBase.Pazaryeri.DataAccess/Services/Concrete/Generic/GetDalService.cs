using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Utilities;
using System.Linq.Expressions;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Generic
{
	public class GetDalService : IGetDalService
	{

		protected readonly IRepository _repository;

		public GetDalService(IRepository repository)
		{
			_repository = repository;
		}

		public async Task<T> GetAsync<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes) where T : class, IEntity, new()
		{
			var query = _repository.GetTable<T>().AsQueryable();
			if (includes != null)
			{
				query = query.IncludeMultiple(includes);
			}
			return await query.FirstOrDefaultAsync(filter);
		}

		public async Task<List<T>> GetListAsync<T>(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes) where T : class, IEntity, new()
		{

			return filter == null
			? await _repository.GetTable<T>().IncludeMultiple(includes).ToListAsync()
			: await _repository.GetTable<T>().IncludeMultiple(includes).Where(filter).ToListAsync();
		}

		public async Task<List<T>> GetListOrderAsync<T>(Expression<Func<T, bool>> filter = null, Expression<Func<T, dynamic>> order = null, bool descending = false) where T : class, IEntity, new()
		{
			IQueryable<T> res = _repository.GetTable<T>();
			if (filter != null)
				res = res.Where(filter);
			if (order != null)
			{
				if (descending)
					res = res.OrderByDescending(order);
				else
					res = res.OrderBy(order);
			}

			return await res.ToListAsync();
		}

		public async Task<List<T>> GetListOrderWithSkipTakeAsync<T>(Expression<Func<T, bool>> filter, Expression<Func<T, dynamic>> order, int skip, int take, bool descending = false) where T : class, IEntity, new()
		{
			return descending
				? await _repository.GetTable<T>().Where(filter).OrderByDescending(order).Skip(skip).Take(take).ToListAsync()
				: await _repository.GetTable<T>().Where(filter).OrderBy(order).Skip(skip).Take(take).ToListAsync();
		}
		public IQueryable<T> GetTable<T>(Expression<Func<T, bool>> filter = null) where T : class, IEntity, new()
		{

			return filter == null
			? _repository.GetTable<T>()
			: _repository.GetTable<T>().Where(filter);
		}
	}
}
