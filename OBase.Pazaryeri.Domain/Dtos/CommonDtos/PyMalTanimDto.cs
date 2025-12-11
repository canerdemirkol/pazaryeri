using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.CommonDtos
{
	public class PyMalTanimDto
	{
		public string MalNo { get; set; }
		public string PyMalNo { get; set; }
		public string PyUrunSatisBirim { get; set; }
		public double PyUrunSatisDeger { get; set; }
	}
}
