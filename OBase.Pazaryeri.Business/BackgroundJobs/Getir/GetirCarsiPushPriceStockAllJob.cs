#region General
using Hangfire;
#endregion

#region Project
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using System.Diagnostics;

#endregion

namespace OBase.Pazaryeri.Business.BackgroundJobs.Getir
{
    [DisableConcurrentExecution(0)]
    public class GetirCarsiPushPriceStockAllJob : IBackgroundJob
	{
		#region Private
		private readonly IGetirCarsiPushPriceStockService _getirPushPriceStockService;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.GetirCarsi);
		#endregion

		#region Const
		public GetirCarsiPushPriceStockAllJob(IGetirCarsiPushPriceStockService getirPushPriceStockService)
		{
			_getirPushPriceStockService = getirPushPriceStockService;
		}
		#endregion

		#region Metot
		public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("GetirCarsiPushPriceStockAllJob started.", _logFolderName);
				await _getirPushPriceStockService.PushPriceStockAsync(properties, CommonEnums.JobType.All);
			}
			catch (Exception ex)
			{
				Logger.Error("GetirPushPriceStockAllJob Error: {exception}", _logFolderName, ex);
			}
			stopwatch.Stop();
			Logger.Information("GetirCarsiPushPriceStockAllJob finished in {elapsedTime}ms.", _logFolderName, stopwatch.ElapsedMilliseconds);
		}
		#endregion
	}
}