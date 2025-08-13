namespace SmsBroadcast.Abstractions;

/// <summary>
/// Resolves an ISmsBroadcastClient for the default or a named configuration.
/// </summary>
public interface ISmsBroadcastClientFactory
{
    /// <summary>Gets the default client registered via AddSmsBroadcast(Action or IConfigurationSection).</summary>
    ISmsBroadcastClient GetClient();

    /// <summary>Gets a client for a named registration created via AddSmsBroadcast(name, Action).</summary>
    ISmsBroadcastClient GetClient(string name);
}