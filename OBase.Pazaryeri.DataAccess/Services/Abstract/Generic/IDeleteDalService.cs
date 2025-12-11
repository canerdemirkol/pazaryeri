using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Generic
{
	public interface IDeleteDalService
	{
		Task Delete<T>(Expression<Func<T, bool>> filter = null) where T : class, IEntity, new();
		Task TruncateTable(string tableName);
	}
}
