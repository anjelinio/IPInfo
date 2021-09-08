using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IPInfo.IPStack
{
    public class IPStackInfoProvider : IIPInfoProvider
    {
        protected IPStackConfiguration _configuration;

        public IPStackInfoProvider(IPStackConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }        

        public virtual async Task<Result<IPDetails>> GetDetails(string ip, bool forceRefresh = false)
        {
            try 
            {
                using(var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_configuration.ApiBase);                
                    var response = await client.GetAsync($"{ip}?access_key={_configuration.AccessKey}");

                    if(!response.IsSuccessStatusCode)
                        return Result.Failure<IPDetails>(Error.Unavailable($"{response.StatusCode} - {response.ReasonPhrase}"));

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var responseJson = JObject.Parse(responseBody);

                    if(responseJson.ContainsKey("success"))
                    { 
                        var errorInfo = responseJson.ToObject<IPErrorResponse>();
                        var error = errorInfo.ToError();

                        return Result.Failure<IPDetails>(error);
                    }
                    else
                    { 
                        var ipLookupInfo = responseJson.ToObject<IPLookupResponse>();
                        if(!IsValid(ipLookupInfo))
                            return Result.Failure<IPDetails>(Error.NotFound(ip));

                        var ipDetails = ipLookupInfo.ToIpDetails();

                        return Result.Success(ipDetails);
                    }
                }
            }
            catch(Exception e)
            { 
                return Result.Failure<IPDetails>(Error.Exception(e.Message));    
            }
        }

        protected virtual bool IsValid(IPLookupResponse response)
        { 
            return null != response
                    && !string.IsNullOrEmpty(response.continent_name)
                    && !string.IsNullOrEmpty(response.country_name)
                    && null != response.latitude
                    && null != response.longitude
                    ;
        }
    }
}
