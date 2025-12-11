using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.Product
{
	public class GetirProductDataPaged
	{
		public int TotalCount { get; set; }
        public List<GetirProductData> Data { get; set; }
    }
	public class GetirProductData
	{
		public string CatalogProductId { get; set; }
		public string MenuProductId { get; set; }
		public List<MenuOption> MenuOptions { get; set; }
        public List<string> Barcodes { get; set; }
        public List<string> Images { get; set; }
        public string VendorId { get; set; }

    }
	public class MenuOption
	{
		public string OptionId { get; set; }
		public decimal Amount { get; set; }
		public decimal Price { get; set; }
	}
}
