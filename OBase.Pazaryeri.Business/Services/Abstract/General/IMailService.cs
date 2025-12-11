using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Business.Services.Abstract.General
{
	public interface IMailService
	{
		public Task SendMailAsync(string subject, string body);
	}
}
