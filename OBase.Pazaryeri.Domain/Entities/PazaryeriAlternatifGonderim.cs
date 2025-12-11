using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
	public class PazaryeriAlternatifGonderim : IEntity
	{
        public string PazarYeriBirimNo { get; set; }
        public string PazarYeriMalNo { get; set; }
        public string CatalogProductId { get; set; }
        public string MenuProductId { get; set; }
        public string ProductImage { get; set; }
        public string OptionId { get; set; }
        public decimal OptionAmount { get; set; }
        public decimal OptionPrice { get; set; }
    }
}
