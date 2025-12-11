using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.Orders
{
    public class CancelDto
    {
        public string cancelReasonId { get; set; }
        public string cancelNote { get; set; }
        public string[] products { get; set; }
    }
}
