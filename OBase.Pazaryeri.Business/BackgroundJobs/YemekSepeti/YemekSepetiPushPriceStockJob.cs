#region General
using Hangfire;
using OBase.Pazaryeri.Business.LogHelper;

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
    public class YemekSepetiPushPriceStockJob : IBackgroundJob
    {
        #region Private
        private readonly IYemekSepetiPushPriceStockService _yemekSepetiPushPriceStockService;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.YemekSepeti);

		#endregion

		#region Const
		public YemekSepetiPushPriceStockJob(IYemekSepetiPushPriceStockService yemekSepetiPushPriceStockService)
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
				Logger.Information("YemekSepetiPushPriceStockJob started.", _logFolderName);
				await _yemekSepetiPushPriceStockService.PushPriceStockAsync(properties);

			}
			catch (Exception ex)
			{
				Logger.Error("YemekSepetiPushPriceStockJob Error: {exception}", _logFolderName, ex);
			}
			stopwatch.Stop();
            Logger.Information($"YemekSepetiPushPriceStockJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }
        #endregion

    }
}