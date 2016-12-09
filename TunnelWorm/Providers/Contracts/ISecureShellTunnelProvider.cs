namespace TunnelWorm.Providers.Contracts
{
    using System.Collections.Generic;

    using Models;

    public interface ISecureShellTunnelProvider
    {
        bool Connect();
        void Disconnect();
        void SetSecureShellSession(SecureShellSettings settings);
        void SetProxySettings(ProxySettings settings);
        void SetPortsSettings(IEnumerable<ForwardedPortModel> settings);
    }
}
