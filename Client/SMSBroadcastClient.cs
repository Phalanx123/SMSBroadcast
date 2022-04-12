using RestSharp;
using SMSBroadcast.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSBroadcast.Client
{
    /// <summary>
    /// SMSBroadcast Client
    /// </summary>
    public class SMSBroadcastClient
    {
        private RestClient Client { get; set; }
        /// <summary>
        /// SMSBroadcast Username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// SMSBroadcast Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSBroadcastClient" /> class.
        /// </summary>
        /// <param name="userName">Username</param>
        /// <param name="password">Password</param>
        public SMSBroadcastClient(string userName, string password)
        {
            Client = new RestClient("http://www.smsbroadcast.com.au/api-adv.php");
            Username = userName;
            Password = password;
        }

        /// <summary>
        /// Attempts to send a single message
        /// </summary>
        /// <param name="outboundMessage">The SMS Object to send</param>
        /// <returns></returns>
        public async Task<SMSBroadcastResponse> SendSMSAsync(SMSBroadcastOutboundMessage outboundMessage)
        {
            var request = CreateDefaultRequest(Method.Post);
            request.AddParameter("to", outboundMessage.To);
            request.AddParameter("message", outboundMessage.Message);
            request.AddParameter("message", outboundMessage.Message);
            if (!string.IsNullOrWhiteSpace(outboundMessage.From))
                request.AddParameter("from", outboundMessage.From);
            if (!string.IsNullOrWhiteSpace(outboundMessage.Reference))
                request.AddParameter("ref", outboundMessage.Reference);
            if (outboundMessage.MaxSplit!=null)
                request.AddQueryParameter("maxSplit", outboundMessage.MaxSplit.ToString());
            if (outboundMessage.Delay != null)
                request.AddQueryParameter("delay", outboundMessage.Delay.ToString());
            var response = await Client.ExecuteAsync<SMSBroadcastResponse>(request);
            return new SMSBroadcastResponse(response.Content);
        }

        /// <summary>
        /// Attempts to send multiple messages
        /// </summary>
        /// <param name="outboundMessages">List of Outbound Messages to send</param>
        /// <returns></returns>
        public async Task<IEnumerable<SMSBroadcastResponse>> SendSMSAsync(IEnumerable<SMSBroadcastOutboundMessage> outboundMessages)
        {
                 SMSBroadcastResponse[] responses = new SMSBroadcastResponse[outboundMessages.Count()];
            int count = 0;
            foreach(var outboundMessage in outboundMessages)
            { 
            var request = CreateDefaultRequest(Method.Post);
            request.AddParameter("to", outboundMessage.To);
            request.AddParameter("message", outboundMessage.Message);
            request.AddParameter("message", outboundMessage.Message);
            if (!string.IsNullOrWhiteSpace(outboundMessage.From))
                request.AddParameter("from", outboundMessage.From);
            if (!string.IsNullOrWhiteSpace(outboundMessage.Reference))
                request.AddParameter("ref", outboundMessage.Reference);
            if (outboundMessage.MaxSplit != null)
                request.AddQueryParameter("maxsplit", outboundMessage.MaxSplit.ToString());
            if (outboundMessage.Delay != null)
                request.AddQueryParameter("delay", outboundMessage.Delay.ToString());
           var response = await Client.ExecuteAsync<SMSBroadcastResponse>(request);
                responses[count++] = new SMSBroadcastResponse(response.Content);
            }
            return responses;

        }

        /// <summary>
        /// Returns SMS Broadcast Balance
        /// </summary>
        /// <returns>Object containing the remaining balance</returns>
        public async Task<SMSBroadcastBalance> GetSMSBroadcastBalanaceAsync()
        {
            var request = CreateDefaultRequest(Method.Get);
            request.AddParameter("action", "balance");
            var response = await Client.ExecuteAsync<SMSBroadcastBalance>(request);
            return new SMSBroadcastBalance(response.Content);
        }

        private RestRequest CreateDefaultRequest(Method method)
        {
            var request = new RestRequest("http://www.smsbroadcast.com.au/api-adv.php",method);
            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username cannot be null");
            request.AddParameter("username", Username);

            if (string.IsNullOrWhiteSpace(Password))
                throw new ArgumentException("Password cannot be null");
            request.AddParameter("password", Password);
            return request;
        }
    }
}
