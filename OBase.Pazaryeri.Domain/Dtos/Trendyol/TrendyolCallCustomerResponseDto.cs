using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Trendyol
{
	public class TrendyolCallCustomerResponseDto
	{
		[JsonProperty("additionalInfo")]
		public string AdditionalInfo { get; set; }

		[JsonProperty("statusCode")]
		public int StatusCode { get; set; }

		[JsonProperty("isSuccess")]
		public bool IsSuccess { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }
    }
}
