using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos
{
    public class MerchantVerify
    {
        public long DETAIL_ID { get; set; }
        public long REF_ID { get; set; }
        public string BARKOD { get; set; }
        public string MAL_NO { get; set; }
        public string PAZAR_YERI_BIRIM_NO { get; set; }
        public string PAZAR_YERI_MAL_NO { get; set; }
        public int THREAD_NO { get; set; }
        public string GUID { get; set; }
    }
}
