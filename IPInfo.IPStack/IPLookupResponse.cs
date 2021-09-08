using System;
using System.Collections.Generic;
using System.Text;

namespace IPInfo.IPStack
{
    public class IPLookupResponse
    {
        public string Ip { get; set; } //   "ip": "134.201.250.155",
        public string type { get; set; } // "type": "ipv4",

        public string continent_code { get; set; } // "continent_code": "NA",
        public string continent_name { get; set; } // "continent_name": "North America",

        public string country_code { get; set; } // "country_code": "US",
        public string country_name { get; set; } // "country_name": "United States",

        public string city { get; set; } // "city": "Los Angeles",

        public double? latitude { get; set; } // "latitude": 34.0453,
        public double? longitude { get; set; } // "longitude": -118.2413

    }
}
