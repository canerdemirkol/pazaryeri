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
    public class TGPushPriceStockAllJob : IBackgroundJob
    {
        private readonly ITrendyolGoPushPriceStockService _trendyolGoPushPriceStockService;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.TrendyolGo);

		public TGPushPriceStockAllJob(ITrendyolGoPushPriceStockService trendyolGoPushPriceStockService)
        {
            _trendyolGoPushPriceStockService = trendyolGoPushPriceStockService;
        }

        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("TyGoPushPriceStockAllJob started.", _logFolderName);
				await _trendyolGoPushPriceStockService.PushPriceStock(properties, CommonEnums.JobType.All);
            }
            catch (Exception ex)
            {
                Logger.Error("TyGoPushPriceStockAllJob Error: {exception}", _logFolderName, ex);
            }
			stopwatch.Stop();
            Logger.Information($"TyGoPushPriceStockAllJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }
    }
}