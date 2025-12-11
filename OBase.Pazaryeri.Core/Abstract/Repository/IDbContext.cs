using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace OBase.Pazaryeri.Core.Abstract.Repository
{
	public interface IDbContext : IDisposable
	{
		DatabaseFacade Database { get; }
        DbSet<TEntity> Set<TEntity>() where TEntity : class, IEntity, new();
		int SaveChanges();
		Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);
	}
}
