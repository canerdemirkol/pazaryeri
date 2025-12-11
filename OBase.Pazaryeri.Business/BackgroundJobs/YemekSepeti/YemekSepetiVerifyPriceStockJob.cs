#region General
using Hangfire;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Promotion;


#endregion

#region Project
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Domain.Enums;
using System.Diagnostics;

#endregion

namespace OBase.Pazaryeri.Business.BackgroundJobs.YemekSepeti
{
    [DisableConcurrentExecution(0)]
    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class YemekSepetiVerifyPriceStockJob : IBackgroundJob
    {
        #region Private
        private readonly IYemekSepetiPushPriceStockService _yemekSepetiPushPriceStockService;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.YemekSepeti);
		#endregion

		#region Const
		public YemekSepetiVerifyPriceStockJob(IYemekSepetiPushPriceStockService yemekSepetiPushPriceStockService)
        {
            _yemekSepetiPushPriceStockService = yemekSepetiPushPriceStockService;
        }
        #endregion

        #region Metot
        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("YemekSepetiVerifyPriceStockJob started.", _logFolderName);
				await _yemekSepetiPushPriceStockService.VerifyPriceStockAsync(properties);

			}
			catch (Exception ex)
			{
				Logger.Error("YemekSepetiVerifyPriceStockJob Error: {exception}", _logFolderName, ex);
			}
			stopwatch.Stop();
            Logger.Information($"YemekSepetiVerifyPriceStockJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }
        #endregion
    }
}
