using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.Return
{
    public class GetirReturnRespDto
    {
        public List<GetirReturnProductDto> Products { get; set; } = new();
        public GetirReturnOrderInfoDto OrderInfo { get; set; }
        public GetirReturnCustomerInfoDto CustomerInfo { get; set; }
        public GetirReturnDeliveryInfoDto DeliveryInfo { get; set; }
        public List<GetirReturnShopRejectReasonsDto> ShopRejectReasons { get; set; } = new();
    }

    public class GetirReturnProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string ReturnReasonId { get; set; }
        public string ReturnReasonDescription { get; set; }
        public int Status { get; set; }
        public string StatusDescription { get; set; }
        public int? RejectReasonId { get; set; }
        public string RejectReasonDescription { get; set; }
        public string VendorCode { get; set; }
    }

    public class GetirReturnOrderInfoDto
    {
        public string OrderDate { get; set; }
        public string OrderId { get; set; }
    }

    public class GetirReturnCustomerInfoDto
    {
        public string Address { get; set; }
        public string Direction { get; set; }
        public string LocalsCallCenterNumber { get; set; }
    }

    public class GetirReturnDeliveryInfoDto
    {
        public string ReturnCode { get; set; }
        public string DeliveryType { get; set; }
        public string SelectedSlotDate { get; set; }
        public string SelectedSlotTime { get; set; }
    }

    public class GetirReturnShopRejectReasonsDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
