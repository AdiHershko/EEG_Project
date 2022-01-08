using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EEG_Project.Services
{
    public class HttpService : IHttpService
    {
        private RestClient client;
        public HttpService()
        {
            client = new RestClient("http://127.0.0.1:5000");
        }

        public async Task<(double[], double[])> Welch(double[,] data, int channel, int time, int hz)
        {
            try
            {
                var request = new RestRequest();
                request.Resource = "welch";
                request.AddParameter("data", JsonConvert.SerializeObject(data));
                request.AddParameter("time", JsonConvert.SerializeObject(time));
                request.AddParameter("hz", JsonConvert.SerializeObject(hz));
                request.AddParameter("channel", JsonConvert.SerializeObject(channel));
                var response = client.Post(request);
                var arrays = JsonConvert.DeserializeObject<string[]>(response.Content);
                if (arrays != null)
                {
                    var freqs = JsonConvert.DeserializeObject<double[]>(arrays[0]);
                    var psd = JsonConvert.DeserializeObject<double[]>(arrays[1]);

                    return (freqs, psd);
                }
            }
            catch
            {
                return (null,null);
            }
            return (null, null);
        }

        public async Task<(double[], double[])> Welch(double[] data, int time, int hz)
        {
            try
            {

                var request = new RestRequest();
                request.Resource = "welch1d";
                request.AddParameter("data", JsonConvert.SerializeObject(data));
                request.AddParameter("time", JsonConvert.SerializeObject(time));
                request.AddParameter("hz", JsonConvert.SerializeObject(hz));
                var response = client.Post(request);
                var arrays = JsonConvert.DeserializeObject<string[]>(response.Content);
                if (arrays != null)
                {
                    var freqs = JsonConvert.DeserializeObject<double[]>(arrays[0]);
                    var psd = JsonConvert.DeserializeObject<double[]>(arrays[1]);
                    return (freqs, psd);
                }
            }
            catch
            {
                return (null, null);
            }
            return (null, null);
        }

        public async Task Train(int numberOfParts)
        {
            var request = new RestRequest();
            request.Resource = "training";
            request.AddParameter("numberOfParts", numberOfParts);
            var response = client.Post(request);
        }

        public Task<string> Predict(double[] data)
        {
            var request = new RestRequest();
            request.Resource = "classify";
            request.AddParameter("data", JsonConvert.SerializeObject(data));
            var response = client.Post(request);
            var result = JsonConvert.DeserializeObject<string>(response.Content);
            var res = float.Parse(result) == 1.0 ? "ADHD" : "NO ADHD";
            return Task.FromResult(res);
        }
    }
}
