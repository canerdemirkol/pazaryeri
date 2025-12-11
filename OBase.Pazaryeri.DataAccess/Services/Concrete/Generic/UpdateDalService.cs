using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Generic
{
	public class UpdateDalService : IUpdateDalService
	{
		private readonly IRepository _repository;

		public UpdateDalService(IRepository repository)
		{
			_repository = repository;
		}


		public async Task UpdateAsync<T>(T entity) where T : class, IEntity, new()
		{
			await _repository.UpdateAsync(entity);
		}

	}
}
