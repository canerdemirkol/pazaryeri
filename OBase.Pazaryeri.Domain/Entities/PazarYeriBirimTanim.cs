using AutoMapper.Configuration.Annotations;
using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PazarYeriBirimTanim:IEntity
    {
        public string PazarYeriNo { get; set; }
        public string BirimNo { get; set; }
        public string PazarYeriBirimNo { get; set; }
        public virtual BirimTanim Birim { get; set; }
        public string BirimAdi { get; set; }
        public string? AktifPasif { get; set; }

    }
}
