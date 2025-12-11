using Hangfire;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Product;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Domain.Enums;
using System.Diagnostics;

namespace OBase.Pazaryeri.Business.BackgroundJobs.YemekSepeti
{

    [DisableConcurrentExecution(0)]
    [Obsolete("Yemeksepeti artık ürün resimlerini de gönderdiği için bu servise gerek kalmadı.")]
    public class YemekSepetiProductJob : IBackgroundJob
    {
        #region Private
        private readonly IYemekSepetiProdcutService _yemekSepetiProdcutService;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.YemekSepeti);

        #endregion

        #region Const
        public YemekSepetiProductJob(IYemekSepetiProdcutService yemekSepetiProdcutService)
        {
            _yemekSepetiProdcutService = yemekSepetiProdcutService;
        }
        #endregion

        #region Metot
        public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                Logger.Information("YemekSepetiProductJob started.", _logFolderName);
                await _yemekSepetiProdcutService.UpdatePYProductImageAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("YemekSepetiProductJob Error: {exception}", _logFolderName, ex);
            }
            stopwatch.Stop();
            Logger.Information($"YemekSepetiProductJob finished in {stopwatch.ElapsedMilliseconds}ms. {stopwatch.Elapsed.Minutes}min {stopwatch.Elapsed.Seconds}seconds ", _logFolderName);
        }
        #endregion
    }
}