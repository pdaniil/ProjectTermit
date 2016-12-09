using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using TunnelWorm.Configuration;
using TunnelWorm.Helpers;
using TunnelWorm.Models;
using TunnelWorm.Providers;
using TunnelWorm.Providers.Contracts;

namespace TunnelWorm.ViewModels
{
    public class MainWindowViewModel : Screen
    {
        private readonly ILoggerProvider _loggerProvider;

        #region Private Members

        private readonly SecureShellTunnelProvider _secureShellTunnelProvider;
        private BindableCollection<ForwardedPortModel> _forwardedPorts;
        private ForwardedPortModel _selectedPort;

        private bool _httpRadio;
        private bool _socks4Radio;
        private bool _socks5Radio;
        private bool _localRadio;
        private bool _remoteRadio;
        private bool _dynamicRadio;

        private string _forwardedAddress;
        private string _forwardedPort;
        private bool _enableProxy;

        private string _connectionStatus;
        private string _connectionToggle;
        private string _title;
        private ConnectionStatus ConStat;

        #endregion

        public Settings Settings { get; set; } = new Settings();
        public SecureShellSettings SecureShellSettings => Settings?.SecureShellSettings;
        public ProxySettings ProxySettings => Settings?.ProxySettings;

        public bool ProxyBoxEnabled { get; set; }
        public bool RemoteBoxEnabled { get; set; }

        public string ConnectionToggle
        {
            get { return _connectionToggle; }
            set
            {
                _connectionToggle = value;
                NotifyOfPropertyChange();
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                _connectionStatus = value;
                NotifyOfPropertyChange();
            }
        }

        public bool EnableProxy
        {
            get { return _enableProxy; }
            set
            {
                _enableProxy = value;
                ProxyBoxEnabled = _enableProxy;
                ProxySettings.UseProxy = _enableProxy;
                NotifyOfPropertyChange(nameof(ProxyBoxEnabled));
            }
        }

        public bool HttpRadio
        {
            get { return _httpRadio; }
            set
            {
                _httpRadio = value;
                if (value)
                    ProxySettings.ProxyType = ProxyTypes.Http;
            }
        }

        public bool Socks4Radio
        {
            get { return _socks4Radio; }
            set
            {
                _socks4Radio = value;
                if (value)
                    ProxySettings.ProxyType = ProxyTypes.Socks4;
            }
        }

        public bool Socks5Radio
        {
            get { return _socks5Radio; }
            set
            {
                _socks5Radio = value;
                if (value)
                    ProxySettings.ProxyType = ProxyTypes.Socks5;
            }
        }

        public bool LocalRadio
        {
            get { return _localRadio; }
            set
            {
                _localRadio = value;
                NotifyOfPropertyChange();
            }
        }

        public bool RemoteRadio
        {
            get { return _remoteRadio; }
            set
            {
                _remoteRadio = value;
                RemoteBoxEnabled = _remoteRadio;
                NotifyOfPropertyChange(nameof(RemoteBoxEnabled));
            }
        }

        public bool DynamicRadio
        {
            get { return _dynamicRadio; }
            set
            {
                _dynamicRadio = value;
                NotifyOfPropertyChange();
            }
        }

        public string ForwardedAddress
        {
            get { return _forwardedAddress; }
            set
            {
                _forwardedAddress = value;
                NotifyOfPropertyChange();
            }
        }

        public string ForwardedPort
        {
            get { return _forwardedPort; }
            set
            {
                _forwardedPort = value;
                NotifyOfPropertyChange();
            }
        }

        public string RemoteAddress { get; set; }
        public string RemotePort { get; set; }

        public ForwardedPortModel SelectedForwardedPort
        {
            get { return _selectedPort; }
            set
            {
                _selectedPort = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<ForwardedPortModel> ForwardedPorts
        {
            get { return _forwardedPorts; }
            set
            {
                _forwardedPorts = value;
                NotifyOfPropertyChange();
            }
        }

        public MainWindowViewModel(ISecureShellTunnelProvider secureShellTunnelProvider, ILoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;

            _loggerProvider.Write("App started.");
            _secureShellTunnelProvider = secureShellTunnelProvider as SecureShellTunnelProvider;
            if (_secureShellTunnelProvider != null)
                _secureShellTunnelProvider.ConnectionStatusChanged += SecureShellTunnelProviderOnConnectionStatusChanged;

            //Set defaults
            HttpRadio = true;
            LocalRadio = true;

            //Try open config
            Settings.Load();

            ForwardedPorts = Settings.Ports.ToBindableCollection();

            EnableProxy = Settings.ProxySettings.UseProxy;

            ConnectionToggle = "Connect";
            ConnectionStatus = Helpers.ConnectionStatus.Disconnected.ToString();
            ConStat = Helpers.ConnectionStatus.Disconnected;

            Title = "Tunnel Worm";
        }

        private void SecureShellTunnelProviderOnConnectionStatusChanged(ConnectionStatus stat)
        {
            ConnectionStatus = stat.ToString();
            ConStat = stat;
            switch (stat)
            {
                case Helpers.ConnectionStatus.Established:
                    ConnectionToggle = "Disconnect";
                    break;
                case Helpers.ConnectionStatus.Failed:
                    ConnectionToggle = "Stop";
                    break;
                case Helpers.ConnectionStatus.Reconnecting:
                    ConnectionToggle = "Stop";
                    break;
                case Helpers.ConnectionStatus.Connecting:
                    ConnectionToggle = "Disconnect";
                    break;
                case Helpers.ConnectionStatus.Disconnected:
                    ConnectionToggle = "Connect";
                    break;
            }
        }

        public void AddPort()
        {
            if (RemoteRadio)
            {
                if (ValidateFields(
                    Pairing.Of("Remote port", RemotePort),
                    Pairing.Of("Remote addres", RemoteAddress),
                    Pairing.Of("Forwarded port", ForwardedPort),
                    Pairing.Of("Forwarded address", ForwardedAddress)))
                {
                    var p = new ForwardedPortModel(TunnelTypes.Remote, uint.Parse(ForwardedPort), ForwardedAddress,
                        uint.Parse(RemotePort), RemoteAddress);
                    ForwardedPorts.Add(p);
                }
            }
            else if (DynamicRadio)
            {
                if (ValidateFields(Pairing.Of("Forwarded port", ForwardedPort)))
                {
                    var p = new ForwardedPortModel(TunnelTypes.Dynamic, uint.Parse(ForwardedPort));
                    ForwardedPorts.Add(p);
                }
            }
            else if (LocalRadio)
            {
                if (ValidateFields(
                    Pairing.Of("Forwarded port", ForwardedPort),
                    Pairing.Of("Forwarded address", ForwardedAddress)))
                {
                    var p = new ForwardedPortModel(TunnelTypes.Local, uint.Parse(ForwardedPort));
                    ForwardedPorts.Add(p);
                }
            }
        }

        public void RemovePort()
        {
            if (ConStat == Helpers.ConnectionStatus.Disconnected)
            {
                ForwardedPorts.Remove(SelectedForwardedPort);

                _secureShellTunnelProvider.SetPortsSettings(ForwardedPorts);
                Settings.Ports = ForwardedPorts.ToList();

                SaveSettings();
            }
        }

        public void Connect()
        {
            switch (ConStat)
            {
                case Helpers.ConnectionStatus.Disconnected:
                    StartConnection();
                    break;
                default:
                    StopConnection();
                    break;
            }
        }

        private void StartConnection()
        {
            if (ValidateFields(
                Pairing.Of("Host name", SecureShellSettings.HostName),
                Pairing.Of("Host username", SecureShellSettings.HostUsername),
                Pairing.Of("Host port", SecureShellSettings.HostPort.ToString())))
            {
                _secureShellTunnelProvider.SetSecureShellSession(SecureShellSettings);

                if (EnableProxy && ValidateFields(
                    Pairing.Of("Proxy address", ProxySettings.ProxyHostName),
                    Pairing.Of("Proxy port", ProxySettings.ProxyPort.ToString())))
                {
                    _secureShellTunnelProvider.SetProxySettings(ProxySettings);
                }
                _secureShellTunnelProvider.SetPortsSettings(ForwardedPorts);

                Settings.Ports = ForwardedPorts.ToList();

                SaveSettings();

                _secureShellTunnelProvider.Connect();
            }
        }

        private void StopConnection()
        {
            _secureShellTunnelProvider.Disconnect();
        }

        private void SaveSettings()
        {
            Settings.Save();
        }

        private bool ValidateFields(params KeyValuePair<string, string>[] fields)
        {
            var message = new StringBuilder("Please, check fields: \n");
            var res = true;

            foreach (var f in fields)
            {
                if (string.IsNullOrWhiteSpace(f.Value))
                {
                    message.AppendLine(f.Key);
                    res = false;
                }
            }
            if (!res)
            {
                //ToDo: show message boxwith error
            }
            return res;
        }
    }
}
