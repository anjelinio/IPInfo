using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPInfo.Batch
{
    public class InMemoryBatchJobRepository : IBatchJobRepository
    {
        static Queue<BatchUpdateRequest> _requests = new Queue<BatchUpdateRequest>();

        static List<BatchUpdateRequest> _faulted = new List<BatchUpdateRequest>();


        static BatchRequestStatus?[] terminalStatuces = new BatchRequestStatus?[]
        {
            BatchRequestStatus.Completed,
            BatchRequestStatus.Faulted
        };


        static object _requests_lock = new object();

        public Guid Add(BatchUpdateRequest request)
        {
            lock(_requests_lock)
            {
                _requests.Enqueue(request);
            }
            
            return request.Id;
        }

        public BatchUpdateRequest Get(Guid operationId)
        {
            lock (_requests_lock)
            {
                return _requests.FirstOrDefault(r => r.Id == operationId)
                    ?? _faulted.FirstOrDefault(r => r.Id == operationId);
            }
        }  
        
        public BatchUpdateRequest GetNext()
        {
            lock (_requests_lock)
            {
                if(0 == _requests.Count)
                    return null;

                var currentStatus = _requests.Peek().Status;

                if(terminalStatuces.Contains(currentStatus))
                {   
                    var delReq = _requests.Dequeue(); 

                    if(BatchRequestStatus.Faulted == delReq.Status)
                        _faulted.Add(delReq);
                }

                return 0 == _requests.Count ? null : _requests.Peek();
            }
        }

        public IEnumerable<BatchUpdateRequest> GetFaulted(bool clearCache = true)
        {
            lock(_requests_lock)
            { 
                var retVal = _faulted.ToArray();
                
                if(clearCache)
                    _faulted.Clear();

                return retVal;
            }
        }
    }
}
