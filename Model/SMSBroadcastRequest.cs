
using System.Text;
using System.Text.Json.Serialization;

namespace SMSBroadcast;
public class SMSBroadcastRequest
{
    /// <summary>
    /// The receiving mobile number(s). The numbers can be in the format:
    /// 04xxxxxxxx(Australian format)
    /// 614xxxxxxxx(International format without a preceding +)
    /// 4xxxxxxxx(missing leading 0)
    /// We recommend using the international format, but your messages will be accepted in any of the above formats.
    /// To send the same message to multiple recipients, the numbers should be separated by a comma.The numbers should contain only numbers, with no spaces or other characters.
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// The sender ID for the messages. Can be a mobile number or letters, up to 11 characters and should not contain punctuation or spaces. 
    /// Leave blank to use a number from the shared number pool.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// The content of the SMS message. Must not be longer than 160 characters unless the maxsplit parameter is used.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Your reference number for the message to help you track the message status. This parameter is optional and can be up to 20 characters.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// Determines the maximum length of your SMS message
    /// </summary>
    public int MaxSplit { get; set; }

    /// <summary>
    /// Number of minutes to delay the message. Use this to schedule messages for later delivery.
    /// </summary>
    public int Delay { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SMSBroadcastRequest" /> class.
    /// </summary>
    /// <param name="message">The content of the SMS message. Must not be longer than 160 characters unless the maxsplit parameter is used.
    /// <param name="to"> The receiving mobile number(s). </param>
    /// <param name="from">The sender ID for the messages. Can be a mobile number or letters, up to 11 characters and should not contain punctuation or spaces.</param>
    /// <param name="reference">Your reference number for the message to help you track the message status. This parameter is optional and can be up to 20 characters.</param>
    /// <param name="delay">Number of minutes to delay the message. Use this to schedule messages for later delivery.</param>
    /// <param name="maxSplit">Determines the maximum length of your SMS message.</param>
    public SMSBroadcastRequest(string message, string to, string? from = default, string? reference = default, int delay = 0, int? maxSplit = 5)
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

        From = from;
        Delay = delay;
        Reference = reference;
    }

    /// <summary>
    /// Returns the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("class SMSBroadcastRequest {\n");
        sb.Append("  Message: ").Append(Message).Append("\n");
        sb.Append("  To: ").Append(To).Append("\n");
        sb.Append("  From: ").Append(From).Append("\n");
        sb.Append("  Reference: ").Append(Reference).Append("\n"); 
        sb.Append("  MaxSplit: ").Append(MaxSplit).Append("\n");
        sb.Append("  Delay: ").Append(Delay).Append("\n");
        sb.Append("}\n");
        return sb.ToString();
    }

}