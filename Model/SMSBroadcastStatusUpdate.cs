using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSBroadcast.Model
{
    /// <summary>
    /// Status Update
    /// </summary>
    public class SMSBroadcastStatusUpdate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SMSBroadcastStatusUpdate" /> class.
        /// </summary>
    
        public SMSBroadcastStatusUpdate()
        {
            throw new NotImplementedException();
        }

        ///// <summary>
        ///// Was the SMS sent successfully
        ///// </summary>
        //public bool IsSuccessful => Status == "OK";

        ///// <summary>
        ///// Returns either OK or BAD depending on result of SMS being sent
        ///// </summary>
        //public string Status => Content.Split(':').First();

        ///// <summary>
        ///// The number that was sent the message
        ///// </summary>
        //public string ReceivingNumber => Content.Split(':').Skip(1).First();

        ///// <summary>
        ///// SMS Broadcasts reference number
        ///// </summary>
        //public string? SMSRef => Status == "OK" ? Content.Split(':').Skip(2).FirstOrDefault()?.Replace("\n", "") : null;

        ///// <summary>
        ///// Error message such as Invalid Number
        ///// </summary>
        //public string? ErrorMessage => Status == "BAD" ? Content.Split(':').Skip(2).FirstOrDefault()?.Replace("\n", "") : null;
    }
}
