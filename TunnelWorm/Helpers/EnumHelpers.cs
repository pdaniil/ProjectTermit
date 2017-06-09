namespace Termit.Helpers
{
    public enum ProxyTypes
    {
        None,
        Http,
        Socks5,
        Socks4,
    }
    public enum TunnelTypes
    {
        Local,
        Remote,
        Dynamic
    }
    public enum ProxyAuthMethod
    {
        None,
        Password,
        PrivateKey
    }

    public enum ConnectionStatus
    {
        Established,
        Reconnecting,
        Connecting,
        Disconnected,
        Failed
    }
}
