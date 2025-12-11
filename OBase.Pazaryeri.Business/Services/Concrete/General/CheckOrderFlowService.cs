using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Domain.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Services.Concrete.General
{
	internal class CheckOrderFlowService : ICheckOrderFlowService
	{
		private readonly IMailService _mailService;
		private readonly IGetDalService _getDalService;
		private readonly IRepository _repository;
        private readonly IOptions<AppSettings> _appSettings;

        public CheckOrderFlowService(IMailService mailService, IGetDalService getDalService, IRepository repository, IOptions<AppSettings> appSettings)
        {
            _mailService = mailService;
            _getDalService = getDalService;
            _repository = repository;
            _appSettings = appSettings;
        }

        public async Task CheckOrderFlowAsync(Dictionary<string, string> properties)
		{
			List<PazarYerleri> pazarYerleri = properties[CommonConstants.PazarYerleri].Split(',').Select(x => (PazarYerleri)Enum.Parse(typeof(PazarYerleri), x)).ToList();
			int checkingPeriod = int.Parse(properties[CommonConstants.CheckingPeriod]);
			int controlHour = int.Parse(properties[CommonConstants.ControlHour]);
			int controlMinute = int.Parse(properties[CommonConstants.ControlMinute]);
			foreach (var merchant in pazarYerleri)
			{
				string merchantName = Enum.GetName(typeof(PazarYerleri), merchant);
				DateTime endDate = DateTime.Now;
				DateTime startDate = endDate.AddMinutes(-checkingPeriod);
				bool isWorking = await _getDalService.GetTable<PazarYeriSiparis>().AnyAsync(x => x.InsertDatetime > startDate && x.PazarYeriNo == merchant.GetMerchantNo());
				if (isWorking)
				{
					Logger.Information("Servis sorunsuz çalışıyor. {startDate} - {endDate} aralığında {merchant}'dan siparişler alınabilmiştir.", merchantName, startDate, endDate, merchantName);
				}
				else
				{
					if (endDate.Hour == controlHour && endDate.Minute == controlMinute)
					{
						await _mailService.SendMailAsync($"{merchantName} UYARI!", $"{endDate}'e kadar {merchantName} pazar yerinden sisteme herhangi bir sipariş akışı olmamıştır. <br>Lütfen servisi kontrol ediniz.");
					}
					else
					{
						await _mailService.SendMailAsync($"{merchantName} UYARI!", $"Son {checkingPeriod} dakikadır {merchantName} pazar yerinden sisteme herhangi bir sipariş akışı olmamıştır. <br>Lütfen servisi kontrol ediniz.");
					}
				}
			}
		}

		public async Task GetTodaysOrdersReportAsync()
		{
			var report = _repository.ExecuteSqlCommand<SiparisGunSonuRaporuView>(_appSettings.Value.RawDatabaseQueries.GunSonuRaporQuery);
			var pazarYerleri = report.Select(x => x.PazarYeri).Distinct();
			StringBuilder reportText = new StringBuilder();
			reportText.AppendLine($"{DateTime.Now.ToString("dd.MM.yyyy")} günü için OBASE sisteminde:");
			foreach (var merchant in pazarYerleri)
			{
				reportText.AppendLine($"<br>{merchant} için:");
				foreach (var item in report.Where(x => x.PazarYeri == merchant))
				{
					reportText.AppendLine($"<br> {item.Adet} adet sipariş {item.Durum}");
				}
				reportText.AppendLine("<br>statusunde kalmıştır.");
			}
			await _mailService.SendMailAsync("Günsonu Servis Raporu", reportText.ToString());
		}
	}
}
