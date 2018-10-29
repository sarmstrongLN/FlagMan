using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Ini;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FlagMan.Services
{
    public class WebClient
    {
        private IConfiguration _config;
        private HttpClient _web;

        public WebClient(IConfiguration config)
        {
            _web = new HttpClient();
            _config = config;
        }

        public async Task<HttpContent> Get(string url)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get
            };

            var configSection = _config.GetSection("creds");

            request.Headers.Add("Authorization", "token " + configSection.GetValue<string>("accesstoken"));
            request.Headers.UserAgent.ParseAdd("Chrome 66.0.3359.181");

            var response = await _web.SendAsync(request);
            return response.Content;
        }

        public async Task<HttpContent> Put(string url, string json)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Put,
                Content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _web.SendAsync(request);
            return response.Content;
        }

        public async Task<HttpContent> Send(HttpRequestMessage msg)
        {
            var response = await _web.SendAsync(msg);
            return response.Content;
        }
    }
}
