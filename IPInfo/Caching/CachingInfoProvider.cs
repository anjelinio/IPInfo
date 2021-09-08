using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace IPInfo.IPStack
{
    public class CachingInfoProvider : IIPInfoProvider
    {
        protected CachingConfiguration _configuration;

        protected ObjectCache _cache;

        protected IIPInfoProvider _parent;

        public CachingInfoProvider(CachingConfiguration configuration, IIPInfoProvider parent, ObjectCache cache) 
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public virtual async Task<Result<IPDetails>> GetDetails(string ip, bool forceRefresh = false)
        {
            return await Result.Of(async () => 
            { 
                var configuration = _configuration as CachingConfiguration;

                if (!forceRefresh && _cache.Contains(ip))
                    return (Result<IPDetails>) _cache.Get(ip);

                var fetch = await _parent.GetDetails(ip, forceRefresh);

                if(fetch.IsSuccess)
                    _cache.Set(new CacheItem(ip, fetch), new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, configuration.ExpirationMinutes, 0) });

                return fetch;
            });
        }
    }
}
