using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IPInfo.Persistent
{
    public class PersistentInfoProvider : IIPInfoProvider
    {
        protected IPDetailsDataContext _dbCtx;

        protected IIPInfoProvider _parent;

        public PersistentInfoProvider(PersistentConfiguration configuration, IIPInfoProvider parent, IPDetailsDataContext dbCtx)
        {
            _dbCtx = dbCtx ?? throw new ArgumentNullException(nameof(dbCtx));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public virtual async Task<Result<IPDetails>> GetDetails(string ip, bool forceRefresh = false)
        {
            return await Result.Of(async () => 
            { 
                var item = await _dbCtx.Details.FindAsync(ip);

                if (item != null && !forceRefresh)
                    return Result.Success(item);

                var fetch = await _parent.GetDetails(ip, forceRefresh);
                if (!fetch.IsSuccess)
                    return fetch;                

                if(item == null)
                    _dbCtx.Details.Add(fetch.Value);
                else
                { 
                    item.City = fetch.Value.City;
                    item.Continent = fetch.Value.Continent;
                    item.Country = fetch.Value.Country;
                    item.Latitude = fetch.Value.Latitude;
                    item.Longitude = fetch.Value.Longitude;
                }

                _dbCtx.SaveChanges();

                return fetch;
            });
        }
    }
}
