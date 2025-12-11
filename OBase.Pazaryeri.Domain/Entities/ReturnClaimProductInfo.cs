using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class ReturnClaimProductInfo : IEntity
    {
        public string? PazarYeriMalAdi { get; set; }
        public string? ObaseMalNo { get; set; }
        public string? LineItemId { get; set; }
        public int Miktar { get; set; }
    }
}
