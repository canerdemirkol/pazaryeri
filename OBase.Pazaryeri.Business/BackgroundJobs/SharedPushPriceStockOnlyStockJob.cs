#region General
using Hangfire;
using OBase.Pazaryeri.Business.LogHelper;

#endregion

#region Project
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.Business.Services.Concrete.PushPrice;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Domain.Enums;
using System.Diagnostics;
using static OBase.Pazaryeri.Domain.Constants.Constants;

#endregion

namespace OBase.Pazaryeri.Business.BackgroundJobs
{
    [DisableConcurrentExecution(0)]
    public class SharedPushPriceStockOnlyStockJob : IBackgroundJob
    {
        #region Private
        private readonly ISharedPriceStockOnlyStockService _sharedPriceStockOnlyStockService;
		private readonly string _logFolderName = CommonConstants.SharedPriceStockOnlyStockJob;
		#endregion

		#region Const
		public SharedPushPriceStockOnlyStockJob(ISharedPriceStockOnlyStockService sharedPriceStockOnlyStockService)
        {
            _sharedPriceStockOnlyStockService = sharedPriceStockOnlyStockService;
        }
        #endregion

        #region Metot
        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("SharedPushPriceStockOnlyStockJob started.", _logFolderName);
				await _sharedPriceStockOnlyStockService.SendPushPriceStockOnlyStocks(properties);

			}
			catch (Exception ex)
			{
				Logger.Error("SharedPushPriceStockOnlyStockJob Error: {exception}", _logFolderName, ex);
			}
			stopwatch.Stop();
            Logger.Information($"SharedPushPriceStockOnlyStockJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }
        #endregion

    }
}