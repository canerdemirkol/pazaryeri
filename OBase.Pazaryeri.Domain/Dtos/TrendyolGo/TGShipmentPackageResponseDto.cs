using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
	public class TGShipmentPackageResponseDto
	{

		[JsonProperty("totalElements")]
		public int TotalElements { get; set; }

		[JsonProperty("totalPages")]
		public int TotalPages { get; set; }

		[JsonProperty("page")]
		public int Page { get; set; }

		[JsonProperty("size")]
		public int Size { get; set; }

		[JsonProperty("content")]
		public List<TrendyolGoOrderDto> Content { get; set; }
    }
}