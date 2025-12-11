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

namespace OBase.Pazaryeri.Business.BackgroundJobs.HepsiExpress
{
    [DisableConcurrentExecution(0)]
    public class HXPushPriceStockAllJob : IBackgroundJob
    {
        #region Private
        private readonly IHepsiExpressPushPriceStockService _hepsiExpressPushPriceStockService;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.HepsiExpress);
        #endregion

        #region Const
        public HXPushPriceStockAllJob(IHepsiExpressPushPriceStockService hepsiExpressPushPriceStockService)
        {
            _hepsiExpressPushPriceStockService = hepsiExpressPushPriceStockService;
        }
        #endregion
        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("HXPushPriceStockAllJob started.", _logFolderName);
				await _hepsiExpressPushPriceStockService.PushPriceStockAsync(properties, CommonEnums.JobType.All);
            }
            catch (Exception ex)
            {
                Logger.Error("HXPushPriceStockAllJob Error: {exception}", _logFolderName , ex);
            }
			stopwatch.Stop();
            Logger.Information($"HXPushPriceStockAllJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }
    }
}