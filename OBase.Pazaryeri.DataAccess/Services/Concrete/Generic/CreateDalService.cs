using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Generic
{
    public class CreateDalService : ICreateDalService
    {

        private readonly IRepository _repository;

        public CreateDalService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<T> AddAsync<T>(T entity) where T : class, IEntity, new()
        {
            return await _repository.AddAsync(entity).ConfigureAwait(false);
        }

        public async Task AddRangeAsync<T>(IEnumerable<T> entityList) where T : class, IEntity, new()
        {
            await _repository.AddRangeAsync(entityList).ConfigureAwait(false);
        }
    }
}

