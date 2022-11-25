
using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace SMSBroadcast.Model;

/// <summary>
/// Represents a SMS Broadcast Request
/// </summary>
public class SMSBroadcastOutboundMessage : SMSBroadcastMessage
{


    /// <summary>
    /// Determines the maximum length of your SMS message
    /// </summary>
    public int? MaxSplit { get; set; }

    /// <summary>
    /// Number of minutes to delay the message. Use this to schedule messages for later delivery.
    /// </summary>
    public int? Delay { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SMSBroadcastOutboundMessage" /> class.
    /// </summary>
    /// <param name="message">The content of the SMS message. Must not be longer than 160 characters unless the maxsplit parameter is used.</param>
    /// <param name="to"> The receiving mobile number(s). Can be comma seperated values </param>
    /// <param name="from">The sender ID for the messages. Can be a mobile number or letters, up to 11 characters and should not contain punctuation or spaces.</param>
    /// <param name="reference">Your reference number for the message to help you track the message status. This parameter is optional and can be up to 20 characters.</param>
    /// <param name="delay">Number of minutes to delay the message. Use this to schedule messages for later delivery.</param>
    /// <param name="maxSplit">Determines the maximum length of your SMS message.</param>
    public SMSBroadcastOutboundMessage(string message, string to, string? from, string? reference = null, int delay = 0, int? maxSplit = 5) : base(to, from, message, reference)
    {
        Delay = delay;
        Reference = reference;
        MaxSplit = maxSplit;
    }

    /// <summary>
    /// Returns the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("class SMSBroadcastRequest {\n");
        sb.Append("  Message: ").Append(Message).Append('\n');
        sb.Append("  To: ").Append(To).Append('\n');
        sb.Append("  From: ").Append(From).Append('\n');
        sb.Append("  Reference: ").Append(Reference).Append('\n');
        sb.Append("  MaxSplit: ").Append(MaxSplit).Append('\n');
        sb.Append("  Delay: ").Append(Delay).Append('\n');
        sb.Append("}\n");
        return sb.ToString();
    }

    /// <summary>
    /// Gets the max possible number of credits that will be used if message is sent
    /// </summary>
    /// <returns>If the message sends successfully for all receipients, the number of credits</returns>
    public int GetCreditCost()
    {
        var numberOfRecipients = To!.Split(",").Length;
        
            return (Message!.Length > 160 ? (int)Math.Round((double)Message.Length / 153, 0) : 1) * numberOfRecipients;
       
    }
}
