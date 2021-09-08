using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPInfo.IPStack
{
    public static class Extensions
    {
        public static IPDetails ToIpDetails(this IPLookupResponse response)
        { 
            return new IPDetails() 
            {
                Ip = response.Ip,
                City = response.city, 
                Continent = response.continent_name, 
                Country = response.country_name, 
                Latitude = response.latitude ?? default(double), 
                Longitude = response.longitude ?? default(double)
            };    
        }

        public static Error ToError(this IPErrorResponse response)
        {
            return Error.Unavailable($"{response.Error.Code} - {response.Error.Info}");
        }        
    }
}
