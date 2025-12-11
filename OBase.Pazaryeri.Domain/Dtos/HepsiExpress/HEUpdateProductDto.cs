using System.Xml.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    [XmlRoot(ElementName = "listing")]
    public class HEUpdateProductDto: ObaseUpdateProductDto
    {    
        [XmlElement(ElementName = "HepsiburadaSku")]
        public string HepsiburadaSku { get; set; }

        [XmlElement(ElementName = "MerchantSku")]
        public string MerchantSku { get; set; }

        [XmlElement(ElementName = "ProductName")]
        public string ProductName { get; set; }

        [XmlElement(ElementName = "Price")]
        public string Price { get; set; }

        [XmlElement(ElementName = "AvailableStock")]
        public int? AvailableStock { get; set; }

        [XmlElement(ElementName = "DispatchTime")]
        public int DispatchTime { get; set; }

        [XmlElement(ElementName = "MaximumPurchasableQuantity")]
        public int MaximumPurchasableQuantity { get; set; }

        [XmlElement(ElementName = "CargoCompany1")]
        public object CargoCompany1 { get; set; }

        [XmlElement(ElementName = "CargoCompany2")]
        public object CargoCompany2 { get; set; }

        [XmlElement(ElementName = "CargoCompany3")]
        public object CargoCompany3 { get; set; }
    }

    [XmlRoot(ElementName = "listings")]
    public class Listings
    {
        [XmlElement(ElementName = "listing")]
        public List<HEUpdateProductDto> Listing { get; set; }
    }
}