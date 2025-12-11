#region General
using Hangfire;
#endregion

#region Project
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Core.Abstract.BackgroundJob;
using OBase.Pazaryeri.Business.Services.Abstract.Return;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using System.Text;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using System.Diagnostics;
#endregion
namespace OBase.Pazaryeri.Business.BackgroundJobs.TrendyolGo
{
    [DisableConcurrentExecution(0)]
    internal class TGClaimsJob : IBackgroundJob, ILogable
    {
        #region Private
        private readonly ITrendyolGoReturnService _tyGoReturnService;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IMailService _mailService;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.TrendyolGo);
		#endregion

		#region Const
		public TGClaimsJob(ITrendyolGoReturnService tyGoReturnService, IOptions<AppSettings> appSettings, IMailService mailService)
		{
			_tyGoReturnService = tyGoReturnService;
			_appSettings = appSettings;
			_mailService = mailService;
		}
		#endregion

		#region Metot
		public async Task RunJobAsync(Dictionary<string, string> properties, IJobCancellationToken cancellationToken)
        {
            StringBuilder errorMessages = new StringBuilder("");
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
                Logger.Information("TyGoClaimsJob started.", fileName: _logFolderName);
                List<string> itemStatus = new List<string> {
                "Created","WaitingInAction"
                };
                bool sendToQp = bool.Parse(properties[TyGoConstants.Parameters.SendToQp]);
                foreach (var statusItem in itemStatus)
                {
                    int page = 0;
                    DateTime dateTimeNow = DateTime.Now.ToLocalTime();
                    DateTime myTime = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);

                    long endDate = new DateTimeOffset(myTime).ToUnixTimeMilliseconds();
                    int dayCount = int.Parse(properties[TyGoConstants.Parameters.GetClaimsDayCount]);
                    long startDate = new DateTimeOffset(myTime.AddDays(0 - dayCount)).ToUnixTimeMilliseconds();

                    var result = await _tyGoReturnService.GetTyGoClaimsAsync(startDate.ToString(), statusItem, endDate.ToString(), page.ToString());

                    if (result.ResponseMessage.IsSuccessStatusCode)
                    {
                        var claimList = result?.GetContent()?.Content;
                        if (claimList.Any())
                        {
                            foreach (var item in claimList)
                            {
                                try
                                {
                                    if (await _tyGoReturnService.SaveClaimToDbAsync(item) && sendToQp)
                                    {
                                        await _tyGoReturnService.SendClaimToQPAsync(item);
                                    }
                                }
                                catch (Exception e)
                                {
                                    errorMessages.AppendLine(_logFolderName + $"Iadeler işlenirken bir hata oluştu. Hata = {e.Message} Detay: {e.InnerException?.Message ?? ""}");
                                    Logger.Error("Claim işlenirken bir hata oluştu. Hata: {exception} \r\n Hata alınan claim: {@response}", fileName: _logFolderName, e, item);
                                    continue;
                                }
                            }
                        }

                        for (int i = 1; i < result.GetContent().TotalPages; i++)
                        {
                            result = await _tyGoReturnService.GetTyGoClaimsAsync(startDate.ToString(), statusItem, endDate.ToString(), i.ToString());

                            if (result.ResponseMessage.IsSuccessStatusCode)
                            {
                                var claimListPaged = result?.GetContent()?.Content;
                                if (claimListPaged.Any())
                                {
                                    foreach (var item in claimListPaged)
                                    {
                                        try
                                        {
                                            if (await _tyGoReturnService.SaveClaimToDbAsync(item) && sendToQp)
                                            {
                                                await _tyGoReturnService.SendClaimToQPAsync(item);
                                            }

                                        }
                                        catch (Exception e)
                                        {
											errorMessages.AppendLine(_logFolderName + $"Iadeler işlenirken bir hata oluştu. Hata = {e.Message} Detay: {e.InnerException?.Message ?? ""}");
											Logger.Error("Claim işlenirken bir hata oluştu. Hata {exception} Hata alınan claim: {@response}", _logFolderName, e, item);
                                            continue;
                                        }
                                    }
                                }
                            }
                            else
                            {
								errorMessages.AppendLine(_logFolderName + $"Iadeler çekilirken bir hata oluştu. Hata = {result.StringContent}");
								Logger.Error("Trendyol Go API Request Error with Http Status Code: {statusCode}, API Response: {@response}", _logFolderName, result.ResponseMessage.StatusCode, result?.GetContent());
                            }
                        }
                    }
                    else
                    {
						errorMessages.AppendLine(_logFolderName + $"Iadeler çekilirken bir hata oluştu. Hata = {result.StringContent}");
						Logger.Error("Trendyol Go API Request Error with Http Status Code: {statusCode}, API Response: {@response}", _logFolderName, result.ResponseMessage.StatusCode, result?.GetContent());
                    }
                }
				var claimsToSendQp = await _tyGoReturnService.GetClaimsToSendQpAsync(PazarYeri.TrendyolGo);
				foreach (var item in claimsToSendQp)
				{
					await _tyGoReturnService.SendIadeToQPAsync(item);
					await _tyGoReturnService.UpdateClaimsTryCountAsync(item);
				}
            }
            catch (Exception e)
            {
				errorMessages.AppendLine(_logFolderName + $"Iadeler işlenirken bir hata oluştu. Hata = {e.Message} Detay: {e.InnerException?.Message ?? ""}");
				Logger.Error("TyGoClaimsJob  Error: {exception}", _logFolderName, e);

			}
            finally
            {
				if ((_appSettings.Value.MailSettings?.MailEnabled ?? false) && errorMessages.Length > 0)
				{
					await _mailService.SendMailAsync(_logFolderName + $" Iadeler işlenirken Hata!.", errorMessages.ToString());
				}
				stopwatch.Stop();
				Logger.Information("TyGoClaimsJob finished in {elapsedTime}ms.", _logFolderName, stopwatch.ElapsedMilliseconds);
			}
        }
        #endregion

    }
}