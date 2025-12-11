#region General
using Hangfire;
#endregion

#region Project
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using System.Diagnostics;

#endregion

namespace OBase.Pazaryeri.Business.BackgroundJobs.TrendyolGo
{
    [DisableConcurrentExecution(0)]
    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class TGVerifyPriceStockJob : IBackgroundJob
    {
        #region Private
        private readonly ITrendyolGoPushPriceStockService _trendyolGoPushPriceStockService;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.TrendyolGo);
        #endregion

        #region Const
        public TGVerifyPriceStockJob(ITrendyolGoPushPriceStockService trendyolGoPushPriceStockService)
        {
            _trendyolGoPushPriceStockService = trendyolGoPushPriceStockService;
        }
        #endregion

        #region Metot
        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("TyGoVerifyPriceStockJob started.", _logFolderName);
				await _trendyolGoPushPriceStockService.VerifyPriceStockAsync(properties);

            }
            catch (Exception ex)
            {
                Logger.Error("TyGoVerifyPriceStockJob Error: {exception}", _logFolderName, ex);
            }
			stopwatch.Stop();
            Logger.Information($"TyGoVerifyPriceStockJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
		}
        #endregion

    }
}