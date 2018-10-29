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
            var configSection = _config.GetSection("flowdock");
            var flowParams = new FlowdockParams()
            {

                content = message,
                external_user_name = configSection.GetValue<string>("username"),
                thread_id = configSection.GetValue<string>("threadId")
            };

            var json = JsonConvert.SerializeObject(flowParams);

            var flowToken = configSection.GetValue<string>("token");
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://api.flowdock.com/v1/messages/chat/{flowToken}"),
                Method = HttpMethod.Post,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _client.Send(request);

        }
    }
}
