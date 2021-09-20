using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SMSBroadcast.Model
{
    /// <summary>
    /// The interface for any messaging functions
    /// </summary>
    public class SMSBroadcastMessage
    {
        /// <summary>
        /// The sender ID for the messages. Can be a mobile number or letters, up to 11 characters and should not contain punctuation or spaces. 
        /// Leave blank to use a number from the shared number pool.
        /// </summary>
        public string? From { get; set; }
        /// <summary>
        /// The receiving mobile number(s). The numbers can be in the format:
        /// 04xxxxxxxx(Australian format)
        /// 614xxxxxxxx(International format without a preceding +)
        /// 4xxxxxxxx(missing leading 0)
        /// We recommend using the international format, but your messages will be accepted in any of the above formats.
        /// To send the same message to multiple recipients, the numbers should be separated by a comma.The numbers should contain only numbers, with no spaces or other characters.
        /// </summary>
        public string? To { get; set; }


        /// <summary>
        /// The content of the SMS message. Must not be longer than 160 characters unless the MaxSplit parameter is used.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Your reference number for the message to help you track the message status. This parameter is optional and can be up to 20 characters.
        /// </summary>
        public string? Reference { get; set; }


        /// <summary>
        /// Initalises the base class
        /// </summary>
        /// <param name="to">Message sent to</param>
        /// <param name="from">Message sent by</param>
        /// <param name="message">Message</param>
        /// <param name="reference">Reference</param>
        public SMSBroadcastMessage(string to, string? from, string? message, string? reference)
        {
            // to ensure "message" has content
            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidDataException("Message is a required property for SMSBroadcastRequest and cannot be null");
            else
                Message = message;
            if (string.IsNullOrWhiteSpace(to))
                throw new InvalidDataException("To is a required property for SMSBroadcastRequest and cannot be null");
            else
                To = to;
            if (from?.Length > 11)
                throw new InvalidDataException("from needs to be less than 12 characters");
            else if (from != null && from.Any(x => char.IsPunctuation(x)))
                throw new InvalidDataException("from cannot contain punctuation");
            if (string.IsNullOrWhiteSpace(from))
                From = null;
            else
                From = from;
            if (string.IsNullOrWhiteSpace(reference))
                Reference = null;
            else
                Reference = reference;
        }

        /// <summary>
        /// Initalises object based on Query String
        /// </summary>
        /// <param name="queryString"></param>
        public SMSBroadcastMessage(string queryString)
        {
            var parsed = HttpUtility.ParseQueryString(queryString);  
            if (parsed == null)
                throw new ArgumentNullException(nameof(queryString));

            To = parsed["to"];
            From = parsed["from"];
            Message = parsed["message"];
            Reference = parsed["reference"];
        }
    }
}
