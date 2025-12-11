using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.Return
{
    public class GetirPostReturnReqBody
    {
        public string returnId { get; set; }
        public List<GetirPostReturnReqProducts> products { get; set; }
    }

    public class GetirPostReturnReqProducts
    {
        public string id { get; set; }
        public List<GetirPostReturnReqProductResponse> responses { get; set; }
    }

    public class GetirPostReturnReqProductResponse
    {
        public int status { get; set; }
        public int? rejectReasonId { get; set; }
    }
}
