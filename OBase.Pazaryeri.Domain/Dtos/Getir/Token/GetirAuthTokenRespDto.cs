using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.Token
{
    public class GetirAuthTokenRespDto
    {
        public class Data
        {
            [JsonProperty("token")]
            public string token { get; set; }

            [JsonProperty("password-expire-time")]
            public DateTime passwordexpiretime { get; set; }
        }
        public class Root
        {
            [JsonProperty("meta")]
            public Meta meta { get; set; }

            [JsonProperty("data")]
            public Data data { get; set; }
        }
    }
}
