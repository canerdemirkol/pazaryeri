using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
	public class PazarYeriProductImageView : IEntity
	{
        public string? ProductImage { get; set; }
    }
}
