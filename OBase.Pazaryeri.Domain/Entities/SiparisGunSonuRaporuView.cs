using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
	public class SiparisGunSonuRaporuView : IEntity
	{
        public string PazarYeri { get; set; }
        public string Durum { get; set; }
        public int Adet { get; set; }
    }
}
