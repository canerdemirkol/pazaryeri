using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
	public class EmailHareket : IEntity
	{
        public string Type { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public string? Cc { get; set; }
		public string? Subject { get; set; }
		public string? Body { get; set; }
    }
}
