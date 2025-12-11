using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Generic
{
	public class DeleteDalService : IDeleteDalService
	{
		private readonly IRepository _repository;

		public DeleteDalService(IRepository repository)
		{
			_repository = repository;
		}
		public async Task Delete<T>(Expression<Func<T, bool>> filter = null) where T : class, IEntity, new()
		{
			await _repository.Delete(filter);
		}
		public async Task TruncateTable(string tableName)
		{
			await _repository.TruncateTable(tableName);
		}
	}
}
