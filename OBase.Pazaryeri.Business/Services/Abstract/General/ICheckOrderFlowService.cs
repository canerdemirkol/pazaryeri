using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Services.Abstract.General
{
	public interface ICheckOrderFlowService
	{
		public Task CheckOrderFlowAsync(Dictionary<string,string> properties);
		public Task GetTodaysOrdersReportAsync();
	}
}
