using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.Orders
{
	public class PrepareDto
	{
		[JsonProperty("updatedProducts")]
		public List<UpdatedProduct> UpdatedProducts { get; set; } = new List<UpdatedProduct>();

		[JsonProperty("alternativeProducts", NullValueHandling = NullValueHandling.Ignore)]
		public List<AlternativeProduct> AlternativeProducts { get; set; }

		[JsonProperty("packagingUpdate", NullValueHandling = NullValueHandling.Ignore)]
		public PackagingUpdate PackagingUpdate { get; set; }
	}

	public class PackagingUpdate
	{
		[JsonProperty("newBagCount", NullValueHandling = NullValueHandling.Ignore)]
		public int NewBagCount { get; set; }

		[JsonProperty("newTotalPackagingPrice", NullValueHandling = NullValueHandling.Ignore)]
		public decimal NewTotalPackagingPrice { get; set; }
	}


	public class UpdatedProduct
	{
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		[JsonProperty("newPrice", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? NewPrice { get; set; } //Bunu gönderince getir hata dönüyor??

		[JsonProperty("newCount", NullValueHandling = NullValueHandling.Ignore)]
		public int? NewCount { get; set; }

		[JsonProperty("newTotalWeight", NullValueHandling = NullValueHandling.Ignore)]
		public int? NewTotalWeight { get; set; }
	}
	public class AlternativeProduct
	{
		[JsonProperty("sourceId", NullValueHandling = NullValueHandling.Ignore)]
		public string SourceId { get; set; }

		[JsonProperty("productId", NullValueHandling = NullValueHandling.Ignore)]
		public string ProductId { get; set; }

		[JsonProperty("selectedAmount", NullValueHandling = NullValueHandling.Ignore)]
		public SelectedAmount SelectedAmount { get; set; }
	}

	public class SelectedAmount
	{
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		[JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? Price { get; set; }

		[JsonProperty("newWeight", NullValueHandling = NullValueHandling.Ignore)]
		public int? NewWeight { get; set; }

		[JsonProperty("newPrice", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? NewPrice { get; set; }

		[JsonProperty("newCount", NullValueHandling = NullValueHandling.Ignore)]
		public int? NewCount { get; set; }
	}
}