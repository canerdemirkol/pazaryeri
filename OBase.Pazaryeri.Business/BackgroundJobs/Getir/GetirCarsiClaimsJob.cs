using Hangfire;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Return;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Enums;
using System.Diagnostics;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.BackgroundJobs.Getir
{
	[DisableConcurrentExecution(0)]
	internal class GetirCarsiClaimsJob : IBackgroundJob, ILogable
	{
		#region Private
		private readonly IGetirCarsiReturnService _getirReturnService;
		private readonly AppSettings _appSettings;
		private readonly IMailService _mailService;
		private readonly string getirLogFile;
		#endregion

		#region Const
		public GetirCarsiClaimsJob(IGetirCarsiReturnService getirReturnService, IOptionsSnapshot<AppSettings> appSettings, IMailService mailService)
		{
			_getirReturnService = getirReturnService;
			getirLogFile = nameof(CommonEnums.PazarYerleri.GetirCarsi);
			_appSettings = appSettings.Value;
			_mailService = mailService;
		}
		#endregion

		#region Metot
		public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			Logger.Information("GetirCarsiClaimsJob started.", getirLogFile);
			StringBuilder errorMessages = new StringBuilder("");
			try
			{
				List<string> itemStatus = new()
				{
				GetirConstants.ReturnsStatus.DirectlyToShop
				};
				bool sendToQp = bool.Parse(properties[GetirConstants.Parameters.SendToQp]);
				foreach (var statusItem in itemStatus)
				{
					DateTime dateTimeNow = DateTime.Now.ToLocalTime();
					DateTime myTime = new(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);

					int dayCount = int.Parse(properties[GetirConstants.Parameters.GetClaimsDayCount]);
					DateTime startDate = myTime.AddDays(-dayCount);

					var result = await _getirReturnService.GetGetirClaimsAsync(statusItem, new() { startDate = startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), endDate = myTime.ToString("yyyy-MM-ddTHH:mm:ssZ") });

					if (result.Any())
					{
						foreach (var item in result)
						{
							try
							{
								if (await _getirReturnService.SaveClaimToDbAsync(item) && sendToQp)
								{
									_getirReturnService.SendClaimToQPAsync(item).GetAwaiter().GetResult();
								}
							}
							catch (Exception e)
							{
								errorMessages.AppendLine("<br/>" + getirLogFile + $"Iadeler işlenirken bir hata oluştu. Hata = {e.Message} Detay: {e.InnerException?.Message ?? ""}");
								Logger.Error("Claim işlenirken bir hata oluştu. Hata: {exception} \r\n Hata alınan claim: {claim}", fileName: getirLogFile, e, item);
								continue;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				errorMessages.AppendLine("<br/>" + getirLogFile + $"Iadeler işlenirken bir hata oluştu. Hata = {e.Message} Detay: {e.InnerException?.Message ?? ""}");
				Logger.Error("GetirCarsiClaimsJob > RunJobAsync Servis çalışırken bir hata oluştu. Hata {exceptione}", getirLogFile, e);
			}
			finally
			{
				if ((_appSettings.MailSettings?.MailEnabled?? false) && errorMessages.Length > 0)
				{
					await _mailService.SendMailAsync(getirLogFile + $" Iadeler işlenirken Hata!.", errorMessages.ToString());
				}
				stopwatch.Stop();
				Logger.Information("GetirCarsiClaimsJob finished in {elapsedTime}ms.", getirLogFile, stopwatch.ElapsedMilliseconds);
			}
		}
		#endregion
	}
}