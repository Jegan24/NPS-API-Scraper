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
            //data = JsonConvert.DeserializeObject(response.GetResponseStream()
            //JsonReader jsonReader = 
            //using (HttpClient client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri("https://api.nps.gov/api/v1/");
            //    client.Timeout.Add(TimeSpan.FromMinutes(10));
                               
            //    Task<HttpResponseMessage> responseTask = client.GetAsync("parks?limit=1000&start=1&api_key=" + npsApiKey);
            //    Thread newThread = new Thread(new ThreadStart(responseTask.Wait));
            //    newThread.Start();
            //    HttpResponseMessage result = responseTask.Result;

            //    if (result.IsSuccessStatusCode)
            //    {
            //        string content = await result.Content.ReadAsStringAsync();
            //        data = JsonConvert.DeserializeObject<Rootobject>(content).data;
            //    }
            //}

            return data;
        }
    }
}
