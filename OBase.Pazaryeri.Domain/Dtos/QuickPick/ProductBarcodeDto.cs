using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.QuickPick
{
	public class ProductBarcodeDto
	{
		public string Barcode { get; set; }
		public bool IsPriority { get; set; }
	}
}
