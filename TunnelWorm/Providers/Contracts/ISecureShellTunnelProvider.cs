namespace Termit.Providers.Contracts
{
    using System.Collections.Generic;

    using Models;
    using Providers;

    public interface ISecureShellTunnelProvider
    {
        bool Connect();
        void Disconnect();

        void SetSecureShellSession(SecureShellSettings settings);
        void SetProxySettings(ProxySettings settings);
        void SetPortsSettings(IEnumerable<ForwardedPortModel> settings);

        void SubscribeToConnectionStatus(ConnectionStatusHandler handler);
    }
}
