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
            var request = new RestRequest();
            request.Resource = "welch";
            request.AddParameter("data", JsonConvert.SerializeObject(data));
            request.AddParameter("time", JsonConvert.SerializeObject(time));
            request.AddParameter("hz", JsonConvert.SerializeObject(hz));
            request.AddParameter("channel", JsonConvert.SerializeObject(channel));
            var response = client.Post(request);
            var arrays = JsonConvert.DeserializeObject<string[]>(response.Content);
            var freqs = JsonConvert.DeserializeObject<double[]>(arrays[0]);
            var psd = JsonConvert.DeserializeObject<double[]>(arrays[1]);
            return (freqs, psd);

        }

        public async Task<(double[], double[])> Welch(double[] data, int time, int hz)
        {
            var request = new RestRequest();
            request.Resource = "welch1d";
            request.AddParameter("data", JsonConvert.SerializeObject(data));
            request.AddParameter("time", JsonConvert.SerializeObject(time));
            request.AddParameter("hz", JsonConvert.SerializeObject(hz));
            var response = client.Post(request);
            var arrays = JsonConvert.DeserializeObject<string[]>(response.Content);
            var freqs = JsonConvert.DeserializeObject<double[]>(arrays[0]);
            var psd = JsonConvert.DeserializeObject<double[]>(arrays[1]);
            return (freqs, psd);
        }

    }
}
