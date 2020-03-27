using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NP_API_To_DB.DAOs
{
    class ApiDAO
    {
        private string npsApiKey;
        public ApiDAO(string apiKey)
        {
            npsApiKey = apiKey;
        }
        public IEnumerable<NationalParkServiceJsonData> GetParkData(int querySize, int start)
        {
            NationalParkServiceJsonData[] data = null;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create($"https://api.nps.gov/api/v1/parks?limit=" + querySize.ToString() + "&start=" + start.ToString() +"&api_key=" + npsApiKey);
            request.Timeout = 6000000;
            WebResponse response = request.GetResponse();
            using(var reader = new StreamReader(response.GetResponseStream()))
            {
                using(var jsonReader = new JsonTextReader(reader))
                {
                    data = JsonSerializer.CreateDefault().Deserialize<Rootobject>(jsonReader).data;
                }
            }
            return data;
        }
    }
}
