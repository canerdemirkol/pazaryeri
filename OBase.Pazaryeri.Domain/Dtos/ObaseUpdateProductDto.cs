using System.Xml.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos
{
    public class ObaseUpdateProductDto
    {
        #region Xml Ignore
        [XmlIgnore]
        public long RefId { get; set; }

        [XmlIgnore]
        public long DetailId { get; set; }

        [XmlIgnore]
        public int ThreadNo { get; set; }
        #endregion 
    }
}