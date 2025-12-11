using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Business.Services.Abstract.Promotion
{
    public interface IYemekSepetiPromotionService
    {
        Task PromotionAsync(Dictionary<string, string> properties);
    }
}
