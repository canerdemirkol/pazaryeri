#region General
using Hangfire;
#endregion

#region Project
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using System.Diagnostics;

#endregion

namespace OBase.Pazaryeri.Business.BackgroundJobs.TrendyolGo
{
    [DisableConcurrentExecution(0)]
    public class TGGetCancelledOrderJob : IBackgroundJob
    {
        #region Private
        private readonly ITrendyolGoOrderService _trendyolGoOrderService;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.TrendyolGo);
        #endregion

        #region Const
        public TGGetCancelledOrderJob(ITrendyolGoOrderService trendyolGoOrderService)
        {
            _trendyolGoOrderService = trendyolGoOrderService;
        }
        #endregion

        #region Metot
        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("TyGoGetCancelledOrderJob started.", _logFolderName);
				await _trendyolGoOrderService.ProcessTyGoCancelledOrdersAsync(properties);

            }
            catch (Exception ex)
            {
                Logger.Error("TyGoGetCancelledOrderJob Error: {exception}", _logFolderName, ex);
            }
			stopwatch.Stop();
			Logger.Information("TyGoGetCancelledOrderJob finished in {elapsedTime}ms.", _logFolderName, stopwatch.ElapsedMilliseconds);
		}
        #endregion
    }
}
