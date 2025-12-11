using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PazarYeriJobResult:IEntity
    {
        public long RefId { get; set; }
        public string JobType { get; set; }
        public decimal? ObaseLogId { get; set; }
        public decimal? ThreadSize { get; set; }
        public decimal? NumberOfThreads { get; set; }
        public string HasSent { get; set; }
        public string HasErrors { get; set; }
        public DateTime InsertDatetime { get; set; }
        //public string PazarYeriNo { get; set; }

        public virtual ICollection<PazarYeriJobResultDetails>? PazarYeriJobResultDetails { get; set; }
    }
}