#region General
using Hangfire;
#endregion

#region Project
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using System.Diagnostics;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using OBase.Pazaryeri.Business.LogHelper;
#endregion
namespace OBase.Pazaryeri.Business.BackgroundJobs.Pazarama
{
    [DisableConcurrentExecution(0)]
    public class PZPushPriceStockAllJob : IBackgroundJob
    {
        #region Private
        private readonly IPazaramaPushPriceStockService _pazaramaPushPriceStockService;
		private readonly string _logFolderName = nameof(PazarYerleri.Pazarama);
		#endregion

		#region Const
		public PZPushPriceStockAllJob(IPazaramaPushPriceStockService pazaramaPushPriceStockService)
        {
            _pazaramaPushPriceStockService = pazaramaPushPriceStockService;
        }
        #endregion

        #region Metot
        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("PZPushPriceStockAllJob started.", _logFolderName);
				await _pazaramaPushPriceStockService.PushPriceStockAsync(properties, CommonEnums.JobType.All);
			}
			catch (Exception ex)
			{
				Logger.Error("PZPushPriceStockAllJob Error: {exception}", _logFolderName, ex);
			}
			stopwatch.Stop();
            Logger.Information($"PZPushPriceStockAllJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
		}
        #endregion
    }
}