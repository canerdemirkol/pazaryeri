using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.Core.Utility;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OBase.Pazaryeri.Core.Concrete.Repository
{
    public class BaseRepository : IRepository
    {
        private readonly IDbContext _context;

        public BaseRepository(IDbContext context)
        {
            _context = context;
        }
        public DbSet<TEntity> GetTable<TEntity>() where TEntity : class, IEntity, new()
        {
            return _context.Set<TEntity>();
        }
        public async Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entity) where TEntity : class, IEntity, new()
        {
            await GetTable<TEntity>().AddRangeAsync(entity).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task<TEntity> AddAsync<TEntity>(TEntity entity) where TEntity : class, IEntity, new()
        {
            var response = await GetTable<TEntity>().AddAsync(entity).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return response.Entity;
        }

        public async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class, IEntity, new()
        {
            GetTable<TEntity>().Update(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public IList<TEntity> ExecuteSqlCommand<TEntity>(string sql, object[] parameters = null) where TEntity : class, IEntity, new()
        {
            if (parameters is null)
            {
                return _context.Set<TEntity>().FromSqlRaw(sql).ToList();
            }
            else
            {
                return _context.Set<TEntity>().FromSqlRaw(sql, parameters).ToList();
            }
        }
        public T ExecuteScalar<T>(string sql)
        {
            return _context.Database.SqlQueryRaw<T>(sql).FirstOrDefault();
        }

        public async Task BeginTransactionAsync()
        {
            if (_context.Database.CurrentTransaction is null)
                await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync()
        {
            if (_context.Database.CurrentTransaction is not null)
                await _context.Database.CommitTransactionAsync();
        }
        public async Task RollbackTransactionAsync()
        {
            if (_context.Database.CurrentTransaction is not null)
                await _context.Database.RollbackTransactionAsync();
        }


        public async Task<int> ExecuteStoredProcedureAsync(string procedure, object[] parameters = null)
        {
            string command = $"BEGIN {procedure}; END;";
            int result = parameters == null
                ? await _context.Database.ExecuteSqlRawAsync(command).ConfigureAwait(false)
                : await _context.Database.ExecuteSqlRawAsync(command, parameters).ConfigureAwait(false);

            return result;
        }

        public async Task<IList<TEntity>> ExecuteStoredProcedureEfCoreAsync<TEntity>(string procedure, object[] parameters = null) where TEntity : class, IEntity, new()
        {
            string command = $"BEGIN {procedure}; END;";

            if (parameters is null)
            {
                return await _context.Set<TEntity>().FromSqlRaw(command).ToListAsync();
            }
            else
            {
                return await _context.Set<TEntity>().FromSqlRaw(command, parameters).ToListAsync();
            }
        }


     


        public async Task<int> ExecuteSqlRawAsync(string sql, object[] parameters = null)
        {

            int result = parameters == null
                ? await _context.Database.ExecuteSqlRawAsync(sql).ConfigureAwait(false)
                : await _context.Database.ExecuteSqlRawAsync(sql, parameters).ConfigureAwait(false);


            return result;
        }

        public async Task UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity, new()
        {
            GetTable<TEntity>().UpdateRange(entities);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<List<TEntity>> ToListAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class, IEntity, new()
        {
            return filter is null
                ? await GetTable<TEntity>().ToListAsync()
                : await GetTable<TEntity>().Where(filter).ToListAsync();
        }

        public async Task Delete<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity, new()
        {
            if (filter is null)
            {
                GetTable<TEntity>().RemoveRange(GetTable<TEntity>());
            }
            else
            {
                GetTable<TEntity>().RemoveRange(GetTable<TEntity>().Where(filter));
            }
            await _context.SaveChangesAsync();
        }
        public async Task TruncateTable(string tableName)
        {
            await _context.Database.ExecuteSqlRawAsync($@"TRUNCATE TABLE {tableName};");
        }
    }
}
