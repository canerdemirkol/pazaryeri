using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{
    public class YemekSepetiVerifyResponseProductDto
    {
        public string Sku { get; set; }
        public string Code { get; set; }
        public string State { get; set; }
        public string Errors { get; set; }
        public int RowNumber { get; set; }
        public string PieceBarcode { get; set; }

        #region Json Ignore
        [JsonIgnore]
        public long RefId { get; set; }
        [JsonIgnore]
        public long DetailId { get; set; }
        [JsonIgnore]
        public int ThreadNo { get; set; }
        #endregion
    }
}
