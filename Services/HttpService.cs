﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EEG_Project.Services
{
    public class HttpService
    {
        private RestClient client;
        public HttpService()
        {
            client = new RestClient("http://127.0.0.1:5000");
        }

        public async Task<(double[], double[])> Welch(double[,] data, int time, int hz)
        {
            var request = new RestRequest();
            request.Resource = "welch";
            request.AddParameter("data", JsonConvert.SerializeObject(data));
            request.AddParameter("time", JsonConvert.SerializeObject(time));
            request.AddParameter("hz", JsonConvert.SerializeObject(hz));
            var response = client.Get(request);
            var arrays = JsonConvert.DeserializeObject<string[]>(response.Content);
            var freqs = JsonConvert.DeserializeObject<double[]>(arrays[0]);
            var psd = JsonConvert.DeserializeObject<double[]>(arrays[1]);
            return (freqs, psd);

        }
    }
}