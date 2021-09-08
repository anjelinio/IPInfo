using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IPInfo.Batch
{
    public class BatchUpdateProvider : IIPInfoBatchUpdateProvider
    {
        protected IBatchJobRepository _batchJobs;

        public BatchUpdateProvider(IBatchJobRepository batchJobs)
        {
            _batchJobs = batchJobs ?? throw new ArgumentNullException(nameof(batchJobs));
        }

        public Task<Result<BatchStatusResponse>> GetStatus(Guid operationId)
        {
            var result = Result.Of(() => 
            {
                var details = _batchJobs.Get(operationId);
                
                return (null == details) ?
                        Result.Failure<BatchStatusResponse>(Error.NotFound(operationId.ToString()))
                        : Result.Success(BatchStatusResponse.Create(details));
            });

            return Task.FromResult(result);
        }

        public Task<Result<Guid>> UpdateDetails(string[] ipAddresses)
        {
            var result = Result.Of(() => 
                _batchJobs.Add(new BatchUpdateRequest(ipAddresses)) 
            );

            return Task.FromResult(result);
        }
    }
}
