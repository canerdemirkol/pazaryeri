using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
    public class TGMarkAlternativeRequestDto
    {
        [JsonProperty("collectedItemIdList")]
        public string[] CollectedItemIdList { get; set; }

        [JsonProperty("alternativeItemIdList")]
        public string[] AlternativeItemIdList { get; set; }
    }
}