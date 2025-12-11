using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.QuickPick
{
	public class ProductDetailDto
	{
		public string ProductCode { get; set; }
		public string ProductModelName { get; set; }
		public string ImageMedium { get; set; }
		public string ImageSmall { get; set; }
		public string Unit { get; set; }
		public string CategoryId { get; set; }
		public string CategoryName { get; set; }
		public decimal Price { get; set; }
		public decimal? StockQuantity { get; set; }
		public decimal? VAT { get; set; }
		public bool Frozen { get; set; }
		public List<ProductBarcodeDto> Barcodes { get; set; }
	}
}
