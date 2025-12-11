using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Idefix.Unsupplied
{
    public class UnsuppliedRequest
    {
        [JsonProperty("items")]
        public List<UnsuppliedItem> Items { get; set; } = new List<UnsuppliedItem>();
    }
}
