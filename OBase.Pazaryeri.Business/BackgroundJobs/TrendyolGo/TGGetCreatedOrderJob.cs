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
    [DisableConcurrentExecution(90)]
    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class TGGetCreatedOrderJob : IBackgroundJob
    {
        private readonly ITrendyolGoOrderService _trendyolGoOrderService;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.TrendyolGo);
		public TGGetCreatedOrderJob(ITrendyolGoOrderService trendyolGoOrderService)
        {
            _trendyolGoOrderService = trendyolGoOrderService;
        }

        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("TGGetCreatedOrderJob started.", _logFolderName);
				await _trendyolGoOrderService.ProcessTyGoCreatedOrdersAsync(properties);
            }
            catch (Exception ex)
            {
                Logger.Error("TyGoGetCreatedOrderJob Error: {exception}", _logFolderName, ex);
            }
			stopwatch.Stop();
			Logger.Information("TGGetCreatedOrderJob finished in {elapsedTime}ms.", _logFolderName, stopwatch.ElapsedMilliseconds);
		}
    }
}
