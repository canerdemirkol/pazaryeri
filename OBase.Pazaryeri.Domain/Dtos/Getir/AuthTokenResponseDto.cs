using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Getir
{
    public class AuthTokenResponseDto
    {
        public Meta meta { get; set; }
        public Data data { get; set; }

    }
    public class Data
    {
        public string token { get; set; }

       [JsonProperty("password-expire-time")]
        public string passwordExpireTime { get; set; }
    }
}
