#region General
using Hangfire;
#endregion

#region Project
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using System.Diagnostics;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
#endregion

namespace OBase.Pazaryeri.Business.BackgroundJobs.HepsiExpress
{
    [DisableConcurrentExecution(0)]
    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class HXVerifyPriceStockJob : IBackgroundJob
	{
		#region Variables
		private readonly IHepsiExpressPushPriceStockService _hepsiExpressPushPriceStockService;
		private readonly string _logFolderName = nameof(PazarYerleri.HepsiExpress);
		#endregion

		#region Ctor
		public HXVerifyPriceStockJob(IHepsiExpressPushPriceStockService hepsiExpressPushPriceStockService)
		{
			_hepsiExpressPushPriceStockService = hepsiExpressPushPriceStockService;
		}
		#endregion

		#region Methods
		public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("HXVerifyPriceStockJob started.", _logFolderName);
				await _hepsiExpressPushPriceStockService.VerifyPriceStockAsync(properties);
			}
			catch (Exception ex)
			{
				Logger.Error("HXVerifyPriceStockJob Error: {exception}", _logFolderName, ex);
			}
			stopwatch.Stop();
            Logger.Information($"HXVerifyPriceStockJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
		}
		#endregion
	}
}