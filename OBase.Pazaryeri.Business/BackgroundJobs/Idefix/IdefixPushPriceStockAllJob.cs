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

namespace OBase.Pazaryeri.Business.BackgroundJobs.Idefix
{
    [DisableConcurrentExecution(0)]
    public class IdefixPushPriceStockAllJob : IBackgroundJob
    {
        #region Private
        private readonly IIdefixPushPriceStockService _idefixPushPriceStockService;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.Idefix);
        #endregion

        #region Const
        public IdefixPushPriceStockAllJob(IIdefixPushPriceStockService idefixPushPriceStockService)
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
                Logger.Information("IdefixPushPriceStockAllJob started.", _logFolderName);
                await _idefixPushPriceStockService.PushPriceStockAsync(properties, CommonEnums.JobType.All);
            }
            catch (Exception ex)
            {
                Logger.Error("IdefixPushPriceStockAllJob Error: {exception}", _logFolderName, ex);
            }
            stopwatch.Stop();
            Logger.Information($"IdefixPushPriceStockAllJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }

        #endregion
    }
}