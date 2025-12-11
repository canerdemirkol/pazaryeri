namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{
    public class YemekSepetiUpdateOrderResponseDto
    {
        public string accepted_for { get; set; }
        public Cancellation cancellation { get; set; }
        public Client client { get; set; }
        public string comment { get; set; }
        public Customers customer { get; set; }
        public string encrypted_customer { get; set; }
        public string external_order_id { get; set; }
        public bool isPreorder { get; set; }
        public Item[] items { get; set; }
        public string order_code { get; set; }
        public string order_id { get; set; }
        public string order_type { get; set; }
        public Payment payment { get; set; }
        public string promised_for { get; set; }
        public string status { get; set; }
        public Sys sys { get; set; }
        public string transport_type { get; set; }
    }  

    public class Delivery_Address
    {
    }  
    public class Additional_Fees
    {
    }  
}