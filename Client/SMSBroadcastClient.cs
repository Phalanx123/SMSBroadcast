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
        /// <param name="broadcastRequest">The SMS Object to send</param>
        /// <returns></returns>
        public async Task<SMSBroadcastResponse> SendSMSAsync(SMSBroadcastOutboundMessage broadcastRequest)
        {
            var request = CreateDefaultRequest(Method.POST);
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
            return new SMSBroadcastResponse(response.Content);
        }

        /// <summary>
        /// Returns SMS Broadcast Balance
        /// </summary>
        /// <returns>Object containing the remaining balance</returns>
        public async Task<SMSBroadcastBalance> GetSMSBroadcastBalanaceAsync()
        {
            var request = CreateDefaultRequest(Method.GET);
            request.AddParameter("action", "balance");
            var response = await Client.ExecuteAsync<SMSBroadcastBalance>(request);
            return new SMSBroadcastBalance(response.Content);
        }

        private RestRequest CreateDefaultRequest(Method method)
        {
            var request = new RestRequest(method);
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
