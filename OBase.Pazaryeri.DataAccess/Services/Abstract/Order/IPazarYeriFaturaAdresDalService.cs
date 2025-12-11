using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Order
{
    public interface IPazarYeriFaturaAdresDalService : IBaseDalService
    {
        Task AddInvoiceAddressAsync(PazarYeriFaturaAdres model);
        Task UpdateInvoiceAddressDataAsync(PazarYeriFaturaAdres moddel);
    }
}
