using SMSBroadcast.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSBroadcast.Extensions
{
    /// <summary>
    /// Extensions that help
    /// </summary>
    public static class SMSBroadcastMessagingExtensions
    {

        /// <summary>
        /// returns the number of credits that will be used if you send these messages
        /// </summary>
        /// <param name="outboundMessages">The enumerable to get credits</param>
        /// <returns></returns>
        public static int GetCreditCost(this IEnumerable<SMSBroadcastOutboundMessage> outboundMessages)
        {
            return outboundMessages.Select(x => x.Message).Sum(x => x.Length > 160 ? (int) Math.Round((double)x.Length / 153,0) : 1);
        }

    }
}
