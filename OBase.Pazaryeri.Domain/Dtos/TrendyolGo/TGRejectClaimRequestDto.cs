using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
	public class TGRejectClaimRequestDto
	{
		[JsonIgnore]
		public string ClaimId { get; set; }
		[JsonProperty("claimItemIds")]
		public string[] ClaimItemIds { get; set; }
		[JsonProperty("reasonId")]
		public int ReasonId { get; set; }
		[JsonProperty("description")]
		public string Description { get; set; }
	}
}
