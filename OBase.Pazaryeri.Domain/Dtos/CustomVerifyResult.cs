using OBase.Pazaryeri.Domain.Enums;

namespace OBase.Pazaryeri.Domain.Dtos
{
    public class CustomVerifyResult
    {
        public string Guid { get; set; }
        public string MerchantId { get; set; }
        public int DetailsCount { get; set; }
        public int SuccessfulCount { get; set; }
        public int ProcessingCount { get; set; }
        public int FailedCount { get; set; }
        public CommonEnums.JobType ExecutionType { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime RequestCompletionDateTime { get; set; }
        public DateTime CompletionDateTime { get; set; }
        public bool HasErrors { get; set; }
        public string ExceptionString { get; set; }
        public List<MerchantVerify> MerchantVerifies { get; set; }
    }
}