using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Order
{
    public class BirimTanimDalService : BaseDalService, IBirimTanimDalService
    {
        public BirimTanimDalService(IRepository repository) : base(repository){}
    }
}