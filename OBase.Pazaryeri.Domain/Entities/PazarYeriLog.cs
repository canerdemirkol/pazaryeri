using OBase.Pazaryeri.Core.Abstract.Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PazarYeriLog : IEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long RefId { get; set; }
        public string PazarYeriNo { get; set; }
        public string? PazarYeriBirimNo { get; set; }
        public int? ThreadNo { get; set; }
        public string? LogType { get; set; }
        public string? Request { get; set; }
        public string? Response { get; set; }
        public string HasErrors { get; set; }
        public string? ExecutionType { get; set; }
        public double? DetailId { get; set; }
        public string? Guid { get; set; }
    }
}