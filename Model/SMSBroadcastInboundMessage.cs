using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSBroadcast.Model
{
    /// <summary>
    /// Inbound Message
    /// </summary>
   public class SMSBroadcastInboundMessage : SMSBroadcastMessage
    {
        /// <summary>
        /// Inbound Message
        /// </summary>
        /// <param name="content"></param>
        public SMSBroadcastInboundMessage(string content) : base(content)
        {
        }
      
    }
}
