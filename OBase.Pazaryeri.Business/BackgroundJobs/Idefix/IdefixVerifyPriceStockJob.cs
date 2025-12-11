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

namespace OBase.Pazaryeri.Business.BackgroundJobs.Idefix
{
    [DisableConcurrentExecution(0)]
    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class IdefixVerifyPriceStockJob : IBackgroundJob
    {
        #region Private
        private readonly IIdefixPushPriceStockService _idefixPushPriceStockService;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.Idefix);
        #endregion

        #region Const
        public IdefixVerifyPriceStockJob(IIdefixPushPriceStockService idefixPushPriceStockService)
        {
            _idefixPushPriceStockService = idefixPushPriceStockService;
        }
        #endregion

        #region Metot
        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                Logger.Information("IdefixVerifyPriceStockJob started.", _logFolderName);
                await _idefixPushPriceStockService.VerifyPriceStockAsync(properties);
            }
            catch (Exception ex)
            {
                Logger.Error("IdefixVerifyPriceStockJob Error: {exception}", _logFolderName, ex);
            }
            stopwatch.Stop();
            Logger.Information($"IdefixVerifyPriceStockJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }

        #endregion
    }
}