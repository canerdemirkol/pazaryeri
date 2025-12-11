using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Generic
{
	public interface ICreateDalService
	{
		Task<T> AddAsync<T>(T entity) where T : class, IEntity, new();
		Task AddRangeAsync<T>(IEnumerable<T> entityList) where T : class, IEntity, new();
    }
}
