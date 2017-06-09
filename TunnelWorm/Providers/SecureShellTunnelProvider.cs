using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;
using Termit.Helpers;
using Termit.Models;
using Termit.Providers.Contracts;
using ProxyTypes = Renci.SshNet.ProxyTypes;

namespace Termit.Providers
{
    public delegate void ConnectionStatusHandler(ConnectionStatus stat);

    public class SecureShellTunnelProvider
        : ISecureShellTunnelProvider
    {
        #region Private Members

        private readonly ILoggerProvider _loggerProvider;

        private SshClient _client;
        private ConnectionInfo _connectInfo;

        private ProxySettings _proxy;
        private SecureShellSettings _secureShell;
        private List<ForwardedPortModel> _ports;

        private const int NUMBERS_OF_RETRIES = 999;

        #endregion

        private event ConnectionStatusHandler ConnectionStatusChanged;

        public SecureShellTunnelProvider()
        {
            
        }
        public SecureShellTunnelProvider(ILoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
        }

        public bool Connect()
        {
            _loggerProvider.Write("Connecting...");
            ConnectionStatusChanged?.Invoke(ConnectionStatus.Connecting);
            _connectInfo = PrepareConnectionInfo();

            _client = new SshClient(_connectInfo);
            _client.ErrorOccurred += OnErrorHandler;

            try
            {
                _client.Connect();

                foreach (var p in _ports)
                {
                    p.Init();
                    _client.AddForwardedPort(p.NativePort);
                    p.Start();
                }
            }
            catch (Exception e)
            {
                _loggerProvider.Write("Connection error: " + e);
                return false;
            }

            _loggerProvider.Write("Connection established.");
            ConnectionStatusChanged?.Invoke(ConnectionStatus.Established);

            return _client.IsConnected;
        }

        public void Disconnect()
        {
            foreach (var port in _client.ForwardedPorts)
                port.Stop();

            _ports.Clear();

            _client.ErrorOccurred -= OnErrorHandler;
            _client.Disconnect();
            _loggerProvider.Write("Disconnected.");
            ConnectionStatusChanged?.Invoke(ConnectionStatus.Disconnected);
        }

        public void SetSecureShellSession(SecureShellSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (_client != null && _client.IsConnected)
                throw new UnauthorizedAccessException(
                    "Secure shell connection already established, settings changin not allowed");

            _secureShell = settings;
        }

        public void SetProxySettings(ProxySettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (_client != null && _client.IsConnected)
                throw new UnauthorizedAccessException(
                    "Secure shell connection already established, settings changin not allowed");

            _proxy = settings;
        }

        public void SetPortsSettings(IEnumerable<ForwardedPortModel> settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (_client != null && _client.IsConnected)
                throw new UnauthorizedAccessException(
                    "Secure shell connection already established, settings changin not allowed");

            _ports = settings.ToList();
        }

        private ConnectionInfo PrepareConnectionInfo()
        {
            ConnectionInfo connectionInfo;

            if (_proxy != null)
            {
                var proxyType = ProxyTypes.None;
                switch (_proxy.ProxyType)
                {
                    case Helpers.ProxyTypes.Http:
                        proxyType = ProxyTypes.Http;
                        break;
                    case Helpers.ProxyTypes.Socks4:
                        proxyType = ProxyTypes.Socks4;
                        break;
                    case Helpers.ProxyTypes.Socks5:
                        proxyType = ProxyTypes.Socks5;
                        break;
                }

                if (_proxy.ProxyAuthMethod == ProxyAuthMethod.Password)
                {
                    connectionInfo = new PasswordConnectionInfo(
                        _secureShell.HostName,
                        _secureShell.HostUsername,
                        _secureShell.HostPassword.GetString(),
                        proxyType,
                        _proxy.ProxyHostName,
                        _proxy.ProxyPort,
                        _proxy.ProxyUsername,
                        _proxy.ProxyPasswd?.GetString()
                        );
                }
                else //if (_proxy.ProxyAuthMethod == ProxyAuthMethod.PrivateKey)
                {
                    connectionInfo = new PrivateKeyConnectionInfo(
                        _secureShell.HostName,
                        _secureShell.HostPort,
                        _secureShell.HostUsername,
                        proxyType,
                        _proxy.ProxyHostName,
                        _proxy.ProxyPort, new PrivateKeyFile("")
// ToDo:
                        );
                }
            }
            else
            {
                connectionInfo = new ConnectionInfo(
                    _secureShell.HostName,
                    _secureShell.HostPort,
                    _secureShell.HostUsername,
                    new PasswordAuthenticationMethod(_secureShell.HostUsername, _secureShell.HostPassword.GetString())
                    );
            }

            return connectionInfo;
        }

        private void OnErrorHandler(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            if (exceptionEventArgs.Exception.InnerException != null)
                _loggerProvider.Write(exceptionEventArgs.Exception.InnerException.ToString());

            if (!_client.IsConnected)
                Reconnect();
        }

        private bool Reconnect()
        {
            _client.ErrorOccurred -= OnErrorHandler;
            
            var retryCount = NUMBERS_OF_RETRIES;

            foreach (var port in _ports)
            {
                _client.RemoveForwardedPort(port.NativePort);
            }

            while (!_client.IsConnected && retryCount-- > 0)
            {
                Thread.Sleep(1500);
                _loggerProvider.Write("Reconnect... Attempt: " + (NUMBERS_OF_RETRIES - retryCount));

                try
                {
                    ConnectionStatusChanged?.Invoke(ConnectionStatus.Reconnecting);
                    _client.Connect();
                }
                catch (Exception e)
                {
                    _loggerProvider.Write("Reconnect exception: " + e);
                    ConnectionStatusChanged?.Invoke(ConnectionStatus.Failed);
                }

                if (_client.IsConnected)
                {
                    _loggerProvider.Write("Connection established.");
                    ConnectionStatusChanged?.Invoke(ConnectionStatus.Established);

                    foreach (var p in _ports)
                    {
                        p.Init();
                        _client.AddForwardedPort(p.NativePort);
                        p.Start();
                    }

                    _client.ErrorOccurred += OnErrorHandler;
                }
            }
            return _client.IsConnected;
        }

        #region Implementation of ISecureShellTunnelProvider

        public void SubscribeToConnectionStatus(ConnectionStatusHandler handler)
        {
            ConnectionStatusChanged += handler;
        }

        #endregion
    }
}
