using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Order
{
    public interface IPazarYeriKargoAdresDalService:IBaseDalService
    {
        Task AddShipmentAddressAsync(PazarYeriKargoAdres model);
        Task UpdateShipmentAddressDataAsync(PazarYeriKargoAdres moddel);
    }
}
