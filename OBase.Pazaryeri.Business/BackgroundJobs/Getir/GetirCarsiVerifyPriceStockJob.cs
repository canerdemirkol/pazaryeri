#region General
using Hangfire;
#endregion

#region Project
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using System.Diagnostics;
using static OBase.Pazaryeri.Domain.Constants.Constants;

#endregion

namespace OBase.Pazaryeri.Business.BackgroundJobs.Getir
{
    [DisableConcurrentExecution(0)]
    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class GetirCarsiVerifyPriceStockJob : IBackgroundJob
    {
        #region Private
        private readonly IGetirCarsiPushPriceStockService _getirPushPriceStockService;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.GetirCarsi);
        #endregion

        #region Const
        public GetirCarsiVerifyPriceStockJob(IGetirCarsiPushPriceStockService getirPushPriceStockService)
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
                Logger.Information("GetirCarsiVerifyPriceStockJob started.", _logFolderName);
                await _getirPushPriceStockService.VerifyPriceStockAsync(properties);
            }
            catch (Exception ex)
            {
                Logger.Error("GetirVerifyPriceStockJob Error: {exception}", _logFolderName, ex);
            }
            stopwatch.Stop();
            Logger.Information($"GetirCarsiVerifyPriceStockJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }

        #endregion
    }
}