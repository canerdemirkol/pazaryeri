using Hangfire;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.BackgroundJobs.General
{
	public class EndOfTheDayOrderReportJob : IBackgroundJob
	{
		#region Private
		private readonly ICheckOrderFlowService _checkOrderFlowService;
		private readonly string _logFolderName = CommonConstants.GeneralLogFile;
		#endregion

		#region Const
		public EndOfTheDayOrderReportJob(ICheckOrderFlowService checkOrderFlowService)
		{
			_checkOrderFlowService = checkOrderFlowService;
		}
		#endregion

		#region Metot
		public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				Logger.Information("EndOfTheDayOrderReportJob started.", _logFolderName);
				await _checkOrderFlowService.GetTodaysOrdersReportAsync();
			}
			catch (Exception ex)
			{
				Logger.Error("EndOfTheDayOrderReportJob çalışırken bir hata alındı: {exception}", _logFolderName, ex);
			}
			stopwatch.Stop();
			Logger.Information("EndOfTheDayOrderReportJob finished in {elapsedTime}.", _logFolderName, stopwatch.ElapsedMilliseconds);
		}
		#endregion
	}
}
