using OBase.Pazaryeri.Domain.Enums;

namespace OBase.Pazaryeri.Domain.Dtos
{
    public class CustomResult
    {
        public long RefId { get; set; }
        public string MerchantId { get; set; }
        public int DetailsCount { get; set; }
        public int SuccessfulThreadCount { get; set; }
        public int FailedThreadCount { get; set; }
        public int NumberofProducts { get; set; }
        public int ThreadId { get; set; }
        public int InternalThreadCount { get; set; }
        public CommonEnums.JobType ExecutionType { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime RequestCompletionDateTime { get; set; }
        public DateTime CompletionDateTime { get; set; }
        public bool HasErrors { get; set; }
        public string ExceptionsString { get; set; }
        public string MerchantNo { get; set; }
        public int DeliveryDuration { get; set; }
        public string DeliveryType { get; set; }
    }
}