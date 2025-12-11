using Hangfire;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Business.Services.Concrete.Order;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Business.BackgroundJobs.Idefix
{
    [DisableConcurrentExecution(0)]
    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class IdefixCancelledOrderJob : IBackgroundJob
    {
        #region Private
        private readonly IIdefixOrderService _idefixOrderService;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.Idefix);
        #endregion

        #region Const
        public IdefixCancelledOrderJob(IIdefixOrderService idefixOrderService)
        {
            _idefixOrderService = idefixOrderService;
        }
        #endregion
        #region Metot
        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                Logger.Information("IdefixCancelledOrderJob started.", _logFolderName);
                await _idefixOrderService.ProcessIdefixCancelledOrdersAsync(properties);

            }
            catch (Exception ex)
            {
                Logger.Error("IdefixCancelledOrderJob Error: {exception}", _logFolderName, ex);
            }
            stopwatch.Stop();
            Logger.Information("IdefixCancelledOrderJob finished in {elapsedTime}ms.", _logFolderName, stopwatch.ElapsedMilliseconds);

        }
        #endregion

    }
}
