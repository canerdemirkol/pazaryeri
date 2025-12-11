#region General
using Hangfire;
#endregion

#region Project
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Domain.Enums;
using System.Diagnostics;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
#endregion

namespace OBase.Pazaryeri.Business.BackgroundJobs.Trendyol
{
    [DisableConcurrentExecution(0)]
    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class TYVerifyPriceStockJob : IBackgroundJob
    {
        #region Variables

        private readonly ITrendyolPushPriceStockService _trendyolPushPriceStockService;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.Trendyol);

        #endregion

        #region Ctor

        public TYVerifyPriceStockJob(ITrendyolPushPriceStockService trendyolPushPriceStockService)
        {
            _trendyolPushPriceStockService = trendyolPushPriceStockService;
        }

        #endregion

        #region Methods

        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("TYVerifyPriceStockJob started.", _logFolderName);
				await _trendyolPushPriceStockService.VerifyPriceStockAsync(properties);
			}
			catch (Exception ex)
			{
				Logger.Error("TYVerifyPriceStockJob Error: {exception}", _logFolderName, ex);
			}
			stopwatch.Stop();
            Logger.Information($"TYVerifyPriceStockJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }

        #endregion
    }
}