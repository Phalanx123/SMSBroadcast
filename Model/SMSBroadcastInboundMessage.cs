using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSBroadcast.Model
{
   public class SMSBroadcastInboundMessage : SMSBroadcastMessage
    {
        public SMSBroadcastInboundMessage(string message, string to, string? from, string? reference) :base(to,from,message,reference)
        { 
        }
    }
}
