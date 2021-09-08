using RestSharp;
using SMSBroadcast.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSBroadcast.Client
{
    class SMSBroadcastClient
    {
        private RestClient Client { get; set; }
        public string  Username{get;set;}
        public string Password { get;set;}
        public SMSBroadcastClient()
        {
            Client = new RestClient("http://www.smsbroadcast.com.au/api-adv.php");
        }
        public SMSBroadcastClient(string userName, string password)
        {
            Client = new RestClient("http://www.smsbroadcast.com.au/api-adv.php");
            Username = userName;
            Password = password;
        }

        public async Task<SMSBroadcastResponse> SendSMSAsync(SMSBroadcastRequest broadcastRequest)
        {
            var request = new RestRequest(Method.POST);
            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username cannot be null");
            request.AddParameter("username", Username);

            if (string.IsNullOrWhiteSpace(Password))
                throw new ArgumentException("Password cannot be null");
            request.AddParameter("password", Password);

            request.AddParameter("to", broadcastRequest.To);
            request.AddParameter("message", broadcastRequest.Message);
            request.AddParameter("message", broadcastRequest.Message);
            if (!string.IsNullOrWhiteSpace(broadcastRequest.From))
                request.AddParameter("from", broadcastRequest.From);
            if (!string.IsNullOrWhiteSpace(broadcastRequest.Reference))
                request.AddParameter("ref", broadcastRequest.Reference);
            if (!string.IsNullOrWhiteSpace(broadcastRequest.From))
                request.AddParameter("maxSplit", broadcastRequest.MaxSplit);
            if (!string.IsNullOrWhiteSpace(broadcastRequest.From))
                request.AddParameter("delay", broadcastRequest.Delay);
            var response = await Client.ExecuteAsync<SMSBroadcastResponse>(request);
            return response.Data;
        }
    }
}
