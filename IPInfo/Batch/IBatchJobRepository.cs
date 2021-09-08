using System;
using System.Collections.Generic;
using System.Text;

namespace IPInfo.Batch
{
    public interface IBatchJobRepository
    {
        Guid Add(BatchUpdateRequest request);

        BatchUpdateRequest Get(Guid operationId);

        BatchUpdateRequest GetNext();

        IEnumerable<BatchUpdateRequest> GetFaulted(bool clearCache = true);
    }
}
