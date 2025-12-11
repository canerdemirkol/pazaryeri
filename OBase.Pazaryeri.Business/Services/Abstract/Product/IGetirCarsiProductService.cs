using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Business.Services.Abstract.Product
{
	public interface IGetirCarsiProductService
	{
		Task GetGetirProductInfos(Dictionary<string, string> properties);
	}
}
