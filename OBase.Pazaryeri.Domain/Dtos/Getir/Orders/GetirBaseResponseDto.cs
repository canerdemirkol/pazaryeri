using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.Orders
{
    public class GetirBaseResponseDto
    {
        public class Data
        {
            public string batchRequestId { get; set; }
        }

        public class Root
        {
            public Meta meta { get; set; }
            public Data data { get; set; }
        }
    }
}
