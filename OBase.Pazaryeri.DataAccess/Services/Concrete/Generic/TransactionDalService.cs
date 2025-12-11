using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Generic
{
	public class TransactionDalService : ITransactionDalService
	{
		private readonly IRepository _repository;

		public TransactionDalService(IRepository repository)
		{
			_repository=repository;
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
