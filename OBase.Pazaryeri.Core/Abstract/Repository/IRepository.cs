using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Oracle.ManagedDataAccess.Client;
using System.Linq.Expressions;

namespace OBase.Pazaryeri.Core.Abstract.Repository
{
	public interface IRepository
	{
		Task BeginTransactionAsync();
		Task TruncateTable(string tableName);
		Task CommitTransactionAsync();
		Task RollbackTransactionAsync();
		DbSet<TEntity> GetTable<TEntity>() where TEntity : class, IEntity, new();
		Task<List<TEntity>> ToListAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class, IEntity, new();
		Task Delete<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity, new();
        Task<T> AddAsync<T>(T entity) where T : class, IEntity, new();
		Task UpdateAsync<T>(T entity) where T : class, IEntity, new();
		Task UpdateRangeAsync<T>(IEnumerable<T> entities) where T : class, IEntity, new();
        IList<TEntity> ExecuteSqlCommand<TEntity>(string sql, object[] parameters = null) where TEntity : class, IEntity, new();
		public T ExecuteScalar<T>(string sql);

		Task<int> ExecuteStoredProcedureAsync(string procedure, object[] parameters = null);
		Task<IList<TEntity>> ExecuteStoredProcedureEfCoreAsync<TEntity>(string procedure, object[] parameters = null) where TEntity : class, IEntity, new();
        Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entity) where TEntity : class, IEntity, new();
		Task<int> ExecuteSqlRawAsync(string sql, object[] parameters = null);

    }
}
