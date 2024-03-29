﻿using System.Linq;

namespace SMSBroadcast.Model
{
    /// <summary>
    /// Represents a response from SMS Broadcast
    /// </summary>
    public class SMSBroadcastResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SMSBroadcastResponse" /> class.
        /// </summary>
        /// <param name="content">The returned content from SMS Broadcast</param>
        public SMSBroadcastResponse(string content)
        {
            Content = content;
        }

        /// <summary>
        /// Unmodified response
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Was the SMS sent successfully
        /// </summary>
        public bool IsSuccessful => Status == "OK";

        /// <summary>
        /// Returns either OK or BAD depending on result of SMS being sent
        /// </summary>
        public string Status => Content.Split(':').First();

        /// <summary>
        /// The number that was sent the message
        /// </summary>
        public string ReceivingNumber => Content.Split(':').Skip(1).First();

        /// <summary>
        /// SMS Broadcasts reference number
        /// </summary>
        public string? SMSRef => Status == "OK" ? Content.Split(':').Skip(2).FirstOrDefault()?.Replace("\n", "") : null;

        /// <summary>
        /// Error message such as Invalid Number
        /// </summary>
        public string? ErrorMessage => Status == "BAD" ? Content.Split(':').Skip(2).FirstOrDefault()?.Replace("\n", "") : null;

    }
}
