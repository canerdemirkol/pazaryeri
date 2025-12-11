using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
	public class PazarYeriMalTanim : IEntity
	{
		public string PazarYeriNo { get; set; }
		public string MalNo { get; set; }
		public string? AnaMalNo { get; set; }
		public string PazarYeriMalNo { get; set; }
		public string? PazarYeriMalAdi { get; set; }
		public string? PyUrunSatisBirim { get; set; }
		public int? SepeteEklenebilirMiktar { get; set; }
		public decimal? PyUrunSatisDeger { get; set; }
        public string? ImageUrl { get; set; }

    }
}
