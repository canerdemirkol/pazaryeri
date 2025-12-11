using AutoMapper.Configuration.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
	public class PostProductReturnRequestDto
	{
		public int OrderId { get; set; }
		public string? ReturnId { get; set; }
		public string? Result { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? ReasonCode { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? ReasonDescription { get; set; }
	}
}
