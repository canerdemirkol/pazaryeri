using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Order
{
    public class PazarYeriFaturaAdresDalService : BaseDalService, IPazarYeriFaturaAdresDalService
    {      
        public PazarYeriFaturaAdresDalService(IRepository repository) : base(repository) { }

        public async Task AddInvoiceAddressAsync(PazarYeriFaturaAdres model)
        {
                await _repository.AddAsync(model);
        }

        public async Task UpdateInvoiceAddressDataAsync(PazarYeriFaturaAdres moddel)
        {
            await _repository.UpdateAsync(moddel);
        }
    }
}