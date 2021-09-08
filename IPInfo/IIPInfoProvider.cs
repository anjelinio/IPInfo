using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPInfo
{
    public interface IIPInfoProvider
    {
        Task<Result<IPDetails>> GetDetails(string ip, bool forceRefresh = false);
    }
}
