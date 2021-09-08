using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IPInfo
{
    public interface IIPInfoBatchUpdateProvider
    {
        Task<Result<Guid>> UpdateDetails(string[] ipAddresses);

        Task<Result<BatchStatusResponse>> GetStatus(Guid operationId);
    }
}
