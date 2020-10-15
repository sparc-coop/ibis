using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kuvio.Kernel.Core
{
    public interface IChangeFeedRepository<T>
    {
        Task<List<T>> GetChangesAsync(string datasetId, string checkpointId, int? limit, List<string> documentIds);
        Task<List<T>> GetChangesAsync(string datasetId, string checkpointId, int? limit);
    }
}
