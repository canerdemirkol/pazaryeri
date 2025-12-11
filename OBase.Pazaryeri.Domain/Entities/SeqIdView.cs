using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
	public class SeqIdView : IEntity
	{
        public long SeqId { get; set; }
    }
}
