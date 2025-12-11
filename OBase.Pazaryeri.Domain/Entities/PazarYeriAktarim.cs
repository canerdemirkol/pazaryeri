using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
	public class PazarYeriAktarim : IEntity
	{
		public string PazarYeriNo { get; set; }
		public string MalNo { get; set; }
		public string PazarYeriMalNo { get; set; }
		public string BirimNo { get; set; }
		public string PazarYeriBirimNo { get; set; }
		public decimal? SatisFiyat { get; set; }
		public decimal? IndirimliSatisFiyat { get; set; }
		public decimal? QpFiyat { get; set; }
    }
}
