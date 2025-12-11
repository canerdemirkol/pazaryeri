using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.Business.Services.Abstract
{
    public interface IBaseService
    {
        IEnumerable<PazarYeriJobResultDetails> CalculateThreads(int totalCnt, int threadSize, List<PazarYeriJobResultDetails> details, long RefId, string MerchantId, int ThreadId);
    }
}
