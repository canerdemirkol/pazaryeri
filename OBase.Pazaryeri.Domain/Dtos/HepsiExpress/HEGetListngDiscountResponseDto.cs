using System.Xml.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEGetListngDiscountResponseDto
    {
        [XmlRoot(ElementName = "IncludedSkus")]
        public class IncludedSkus
        {

            [XmlElement(ElementName = "string")]
            public string String { get; set; }
        }

        [XmlRoot(ElementName = "IncludedMerchants")]
        public class IncludedMerchants
        {

            [XmlElement(ElementName = "guid")]
            public string Guid { get; set; }
        }

        [XmlRoot(ElementName = "MaximumPurchasableQuantity")]
        public class MaximumPurchasableQuantity
        {

            [XmlAttribute(AttributeName = "nil")]
            public bool Nil { get; set; }
        }

        [XmlRoot(ElementName = "Result")]
        public class Result
        {

            [XmlElement(ElementName = "Id")]
            public string Id { get; set; }

            [XmlElement(ElementName = "Status")]
            public int Status { get; set; }

            [XmlElement(ElementName = "IncludedSkus")]
            public List<string> IncludedSkus { get; set; }

            [XmlElement(ElementName = "Amount")]
            public double Amount { get; set; }

            [XmlElement(ElementName = "StartDate")]
            public DateTime StartDate { get; set; }

            [XmlElement(ElementName = "EndTime")]
            public DateTime EndTime { get; set; }

            [XmlElement(ElementName = "ShortDescription")]
            public string ShortDescription { get; set; }

            [XmlElement(ElementName = "DiscountName")]
            public string DiscountName { get; set; }

            [XmlElement(ElementName = "CreatedAt")]
            public DateTime CreatedAt { get; set; }

            [XmlElement(ElementName = "AffectedListingCount")]
            public int AffectedListingCount { get; set; }

            [XmlElement(ElementName = "CampaignId")]
            public int CampaignId { get; set; }

            [XmlElement(ElementName = "IncludedMerchants")]
            public List<string> IncludedMerchants { get; set; }

            [XmlElement(ElementName = "MaximumPurchasableQuantity")]
            public MaximumPurchasableQuantity MaximumPurchasableQuantity { get; set; }
        }

        [XmlRoot(ElementName = "Discounts")]
        public class Discounts
        {

            [XmlElement(ElementName = "Result")]
            public List<Result> Result { get; set; }
        }

        [XmlRoot(ElementName = "DiscountsResult")]
        public class DiscountsResult
        {

            [XmlElement(ElementName = "Limit")]
            public int Limit { get; set; }

            [XmlElement(ElementName = "Offset")]
            public int Offset { get; set; }

            [XmlElement(ElementName = "Discounts")]
            public List<Result> Discounts { get; set; }

            [XmlAttribute(AttributeName = "xsi")]
            public string Xsi { get; set; }

            [XmlAttribute(AttributeName = "xsd")]
            public string Xsd { get; set; }

            [XmlText]
            public string Text { get; set; }
        }
    }
}