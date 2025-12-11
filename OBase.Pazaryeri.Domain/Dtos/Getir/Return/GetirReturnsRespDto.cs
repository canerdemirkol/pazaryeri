using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.Return
{
    public class GetirReturnsRespDto
    {
        public List<GetirReturnsItemDto> Returns { get; set; } = new List<GetirReturnsItemDto>();
    }

    public class GetirReturnsItemDto
    {
        public Guid Id { get; set; }
        public string ConfirmationId { get; set; }
        public string ReturnCode { get; set; }
        public string ReturnPrice { get; set; }
        public string CustomerName { get; set; }
        public string DeliveryDate { get; set; }
        public string RequestedDate { get; set; }
        public int Status { get; set; }
        public string StatusMessage { get; set; }
        public string ShopResponseDate { get; set; }

        [JsonIgnore]
        public GetirReturnRespDto Detail = new();
    }

    public class GetirReturnsItemDtoWithShopId
    {
        public string ShopId { get; set; }
        public List<GetirReturnsItemDto> Returns { get; set; } = new();
    }
}
