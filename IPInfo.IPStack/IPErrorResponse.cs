using System;
using System.Collections.Generic;
using System.Text;

namespace IPInfo.IPStack
{
    /*
     * {
  "success": false,
  "error": {
    "code": 104,
    "type": "monthly_limit_reached",
    "info": "Your monthly API request volume has been reached. Please upgrade your plan."    
  }
}
     */

    public class IPErrorResponse
    {
        public bool Success { get;set; }

        public IPError Error { get;set; }
    }

    public class IPError
    { 
        public string Code { get;set; }
        
        public string Type { get;set; }

        public string Info { get;set; }
    }
}
