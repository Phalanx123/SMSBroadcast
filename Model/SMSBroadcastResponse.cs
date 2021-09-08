using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSBroadcast.Model
{
	public class SMSBroadcastResponse
    {
        public string To { get; set; }
        public string Ref { get; set; }
        public string SMSRef { get; set; }
        public string Status { get; set; }
    }
}
