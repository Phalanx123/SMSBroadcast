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
