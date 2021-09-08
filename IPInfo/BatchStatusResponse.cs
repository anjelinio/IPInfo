using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPInfo
{
    public class BatchStatusResponse
    {
        public BatchRequestStatus Status { get; private set; }

        public double PercentComplete { get; private set; }

        public static BatchStatusResponse Create(BatchUpdateRequest request)
        {
            if(null == request) throw new ArgumentNullException(nameof(request));

            var complete = request.Requests.Where(r => r.Status == BatchRequestStatus.Completed).Count();
            var faulted = request.Requests.Where(r => r.Status == BatchRequestStatus.Faulted).Count();
            // var waiting = request.Requests.Where(r => r.Status == BatchRequestStatus.Created || r.Status == BatchRequestStatus.Started).Count();

            return new BatchStatusResponse()
            { 
                Status = request.Status,
                PercentComplete = Math.Round((double)(complete+faulted) / request.Requests.Length, 2)
            };
        }
    }
}
