using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPInfo
{
    public class BatchUpdateRequest
    {
        public static readonly BatchRequestStatus[] TerminalStatuces = new BatchRequestStatus[] { BatchRequestStatus.Faulted, BatchRequestStatus.Completed };

        public BatchUpdateRequest(string[] ipAddresses)
        {
            Status = BatchRequestStatus.Created;

            Requests = ipAddresses.Where(p => !string.IsNullOrEmpty(p))
                                  .Select(p => new IPUpdateRequest(p))
                                  .ToArray();

            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public IPUpdateRequest[] Requests { get; private set; }

        public BatchRequestStatus Status { get; set; }
    }

    public class IPUpdateRequest
    {
        public IPUpdateRequest(string ipAddress)
        {
            Ip = ipAddress;
            Status = BatchRequestStatus.Created;
        }

        public string Ip { get; private set; }

        public BatchRequestStatus Status { get; set; }
    }
}
