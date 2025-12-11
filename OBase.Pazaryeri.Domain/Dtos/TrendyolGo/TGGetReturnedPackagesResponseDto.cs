using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
	public class TGGetReturnedPackagesResponseDto
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
		public List<ClaimContent> Content { get; set; }
	}
	public class ClaimItem
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("orderLineItemId")]
		public string OrderLineItemId { get; set; }
		[JsonProperty("note")]
		public string Note { get; set; }
		[JsonProperty("customerNote")]
		public string CustomerNote { get; set; }
		[JsonProperty("claimItemStatus")]
		public ClaimItemStatus ClaimItemStatus { get; set; }
		[JsonProperty("customerClaimItemReason")]
		public CustomerClaimItemReason CustomerClaimItemReason { get; set; }
		[JsonProperty("trendyolClaimItemReason")]
		public TrendyolClaimItemReason TrendyolClaimItemReason { get; set; }
		[JsonProperty("resolved")]
		public bool Resolved { get; set; }
		[JsonProperty("imageUrls")]
		public string[] ClaimImageUrls { get; set; }
    }
	public class ClaimItemStatus
	{
		[JsonProperty("name")]
		public string Name { get; set; }
	}

	public class ClaimContent
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("orderNumber")]
		public string OrderNumber { get; set; }
		[JsonProperty("customerFirstName")]
		public string CustomerFirstName { get; set; }
		[JsonProperty("customerLastName")]
		public string CustomerLastName { get; set; }
		[JsonProperty("orderShipmentPackageId")]
		public long OrderShipmentPackageId { get; set; }
		[JsonProperty("claimItems")]
		public List<ClaimItem> ClaimItems { get; set; }
		[JsonProperty("claimDate")]
		public long ClaimDate { get; set; }
		[JsonProperty("orderDate")]
		public long OrderDate { get; set; }
		[JsonProperty("returnedSeller")]
		public bool ReturnedSeller { get; set; }
	}

	public class ClaimItemReason
	{
		[JsonProperty("id")]
		public int Id { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("code")]
		public string Code { get; set; }
	}
	public class CustomerClaimItemReason : ClaimItemReason { }
	public class TrendyolClaimItemReason : ClaimItemReason { }
}
