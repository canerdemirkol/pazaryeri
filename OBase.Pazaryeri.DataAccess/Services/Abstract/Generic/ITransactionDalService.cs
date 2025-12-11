namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Generic
{
	public interface ITransactionDalService
	{
		Task BeginTransactionAsync();
		Task CommitTransactionAsync();
		Task RollbackTransactionAsync();
	}
}
