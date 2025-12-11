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
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

#endregion

namespace OBase.Pazaryeri.Business.BackgroundJobs.Trendyol
{
    [DisableConcurrentExecution(0)]
    public class TyPushPriceStockAllJob : IBackgroundJob
    {
        #region Variables

        private readonly ITrendyolPushPriceStockService _trendyolPushPriceStockService;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.Trendyol);

		#endregion

		#region Ctor
		public TyPushPriceStockAllJob(ITrendyolPushPriceStockService trendyolPushPriceStockService)
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
				Logger.Information("TyPushPriceStockAllJob started.", _logFolderName);
				await _trendyolPushPriceStockService.PushPriceStockAsync(properties, CommonEnums.JobType.All);
			}
			catch (Exception ex)
			{
				Logger.Error("TyPushPriceStockAllJob Error: {exception}", _logFolderName, ex);
			}
			stopwatch.Stop();
            Logger.Information($"TyPushPriceStockAllJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }

        #endregion
    }
}