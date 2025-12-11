using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class BirimTanim:IEntity
    {
        public  string BirimNo { get; set; }
        public  string BirimAdi { get; set; }
        public virtual IList<PazarYeriBirimTanim>? PazarYeriBirims { get; set; }
    }
}
