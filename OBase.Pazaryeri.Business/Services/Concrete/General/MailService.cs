using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.Business.Services.Concrete.General
{
	public class MailService : IMailService
	{
		private readonly ICreateDalService _createDalService;


		private readonly IOptions<AppSettings> _options;
		public MailService(ICreateDalService createDalService, IOptions<AppSettings> options)
		{
			_createDalService = createDalService;
			_options = options;
		}

		public async Task SendMailAsync(string subject, string body)
		{
			await _createDalService.AddAsync(new EmailHareket
			{
				Body = body,
				Subject = subject,
				Cc = _options.Value.MailSettings.CC,
				From = _options.Value.MailSettings.From,
				To = _options.Value.MailSettings.To,
				Type = "01"
			});
		}
	}
}
