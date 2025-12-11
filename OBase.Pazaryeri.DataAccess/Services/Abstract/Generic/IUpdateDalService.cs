using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Generic
{
	public interface IUpdateDalService
	{
		Task UpdateAsync<T>(T entity) where T : class, IEntity, new();
	}
}
