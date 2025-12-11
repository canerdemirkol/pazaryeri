#region General
using Hangfire;
#endregion

#region Project
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Product;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Domain.Enums;
using System.Diagnostics;
#endregion

namespace OBase.Pazaryeri.Business.BackgroundJobs.Getir
{
    [DisableConcurrentExecution(0)]
    public class GetGetirCarsiProductInfosJob : IBackgroundJob
	{
		#region Private
		private readonly IGetirCarsiProductService _getirCarsiProductService;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.GetirCarsi);
		#endregion

		#region Const
		public GetGetirCarsiProductInfosJob(IGetirCarsiProductService getirCarsiProductService)
		{
			_getirCarsiProductService = getirCarsiProductService;
		}
		#endregion

		#region Metot
		public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("GetGetirCarsiProductInfosJob started.", _logFolderName);
				await _getirCarsiProductService.GetGetirProductInfos(properties);
			}
			catch (Exception ex)
			{
				Logger.Error("GetGetirCarsiProductInfosJob çalışırken bir hata alındı: {exception}", _logFolderName, ex);
			}
			stopwatch.Stop();
			Logger.Information("GetGetirCarsiProductInfosJob finished in {elapsedTime}ms.", _logFolderName, stopwatch.ElapsedMilliseconds);
		}
		#endregion
	}
}