using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;

namespace OBase.Pazaryeri.Business.Factories
{
    public interface IOrderConverterFactory
    {
        IOrderConverter<TOrder> GetConverter<TOrder>() where TOrder : BaseOrderDto;
    }
}