using FlagMan.DTOs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace FlagMan.Services
{
    public class FlowdockApi
    {
        private IConfiguration _config;
        private WebClient _client;

        public FlowdockApi(IConfiguration config)
        {
            _config = config;
            _client = new WebClient(config);
        }
        public async void alertFlowdock(string message)
        {
            //lets not spam it up during testing
            Console.WriteLine(message);
            return;
            var flowParams = new FlowdockParams()
            {
                content = message,
                external_user_name = "{username}",
                thread_id = "{thread_id}"
            };

            var json = JsonConvert.SerializeObject(flowParams);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://api.flowdock.com/v1/messages/chat/{flowtoken}"),
                Method = HttpMethod.Post,
                Content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _client.Send(request);

        }
    }
}
