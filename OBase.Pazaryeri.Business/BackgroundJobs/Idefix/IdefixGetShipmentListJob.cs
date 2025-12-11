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

namespace OBase.Pazaryeri.Business.BackgroundJobs.Idefix
{
    [DisableConcurrentExecution(90)]
    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class IdefixGetShipmentListJob : IBackgroundJob
    {
        private readonly IIdefixOrderService _idefixOrderService;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.Idefix);
		public IdefixGetShipmentListJob(IIdefixOrderService idefixOrderService)
        {
            _idefixOrderService = idefixOrderService;
        }

        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("IdefixGetShipmentListJob started.", _logFolderName);
				await _idefixOrderService.ProcessIdefixCreatedOrdersAsync(properties);
            }
            catch (Exception ex)
            {
                Logger.Error("IdefixGetShipmentListJob Error: {exception}", _logFolderName, ex);
            }
			stopwatch.Stop();
			Logger.Information("IdefixGetShipmentListJob finished in {elapsedTime}ms.", _logFolderName, stopwatch.ElapsedMilliseconds);
		}
    }
}
