using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSBroadcast.Model
{
    /// <summary>
    /// Class that holds information relating to SMSBroadcast Data
    /// </summary>
    public class SMSBroadcastBalance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SMSBroadcastBalance" /> class.
        /// </summary>
        /// <param name="content"></param>
        public SMSBroadcastBalance(string content)
        {
            Content = content;
            Status = Content.Split(":").First();
            if (Status == "OK")
            {
                IsSuccessful = true;
                Balance = int.Parse(Content.Split(":").Skip(1).First());
            }
            else
                ErrorMessage = Content.Split(":").Skip(1).First();
        }

        private string Content { get; set; }

        /// <summary>
        /// returns true if action was successful
        /// </summary>
        public bool IsSuccessful { get; private set; }

        /// <summary>
        /// Status either OK or ERROR
        /// </summary>
        public string Status { get; private set; }
        /// <summary>
        /// Number of credits remaining
        /// </summary>
        public int? Balance { get; private set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; private set; }
    }
}
