namespace OBase.Pazaryeri.Domain.Dtos
{
    public class ThreadLog
    {
        public long RefId { get; set; }
        public string MerchantId { get; set; }
        public int ThreadId { get; set; }
        public int TotalCount { get; set; }
        public int ThreadSize { get; set; }
        public int ThreadCount { get; set; }
        public DateTime? StartedDateTime { get; set; }
        public DateTime? CompletionDateTime { get; set; }
    }
}
