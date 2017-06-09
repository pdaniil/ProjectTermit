namespace Termit.ViewModels
{
    using System.Linq;
    using System.ComponentModel;

    using Caliburn.Micro;

    using Models;
    using Helpers;
    using Configuration;
    using Providers.Contracts;

    public class MainWindowViewModel
        : Screen, IDataErrorInfo
    {
        #region Private Members

        private readonly Settings _settings;
        private readonly ILoggerProvider _loggerProvider;
        private readonly ISecureShellTunnelProvider _secureShellTunnelProvider;

        private BindableCollection<ForwardedPortModel> _forwardedPorts;
        private ForwardedPortModel _selectedPort;

        private string _forwardedAddress;
        private string _forwardedPort;
        private bool _enableProxy;

        private string _connectionStatus;

        private ConnectionStatus _connectionStatusType;
        private TunnelTypes _selectedTunnel;

        private string _remoteAddress;
        private string _remotePort;

        #endregion

        public SecureShellSettings SecureShellSettings => _settings?.SecureShellSettings;
        public ProxySettings ProxySettings => _settings?.ProxySettings;

        public bool ProxyBoxEnabled { get; set; }
        public bool RemoteBoxEnabled { get; set; }

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

        public ProxyTypes SelectedProxy
        {
            get { return ProxySettings.ProxyType; }
            set
            {
                ProxySettings.ProxyType = value;
                NotifyOfPropertyChange();
            }
        }
        public TunnelTypes SelectedTunnel
        {
            get { return _selectedTunnel; }
            set
            {
                _selectedTunnel = value;
                NotifyOfPropertyChange();
            }
        }

        #region Forwarded

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

        #endregion

        #region Remote

        public string RemoteAddress
        {
            get { return _remoteAddress; }
            set
            {
                _remoteAddress = value;
                NotifyOfPropertyChange();
            }
        }
        public string RemotePort
        {
            get { return _remotePort; }
            set
            {
                _remotePort = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

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

        public MainWindowViewModel(ISecureShellTunnelProvider secureShellTunnelProvider, ILoggerProvider loggerProvider, Settings settings)
        {
            _settings = settings;
            _loggerProvider = loggerProvider;
            
            _secureShellTunnelProvider = secureShellTunnelProvider;

            _loggerProvider.Write("App started.");
            _secureShellTunnelProvider.SubscribeToConnectionStatus(ConnectionStatusChanged);

            SelectedProxy = ProxyTypes.None;
            SelectedTunnel = TunnelTypes.Local;

            //Try open config
            _settings.Load();

            ForwardedPorts = _settings.Ports.ToBindableCollection();

            EnableProxy = _settings.ProxySettings.UseProxy;

            ConnectionStatus = Helpers.ConnectionStatus.Disconnected.ToString();
            _connectionStatusType = Helpers.ConnectionStatus.Disconnected;
        }

        public void AddPort()
        {
            switch (SelectedTunnel)
            {
                case TunnelTypes.Remote:
                    var remote = new ForwardedPortModel(TunnelTypes.Remote, uint.Parse(ForwardedPort), ForwardedAddress, uint.Parse(RemotePort), RemoteAddress);
                    ForwardedPorts.Add(remote);
                    break;
                case TunnelTypes.Dynamic:
                    var dynamic = new ForwardedPortModel(TunnelTypes.Dynamic, uint.Parse(ForwardedPort));
                    ForwardedPorts.Add(dynamic);
                    break;
                case TunnelTypes.Local:
                    var local = new ForwardedPortModel(TunnelTypes.Local, uint.Parse(ForwardedPort));
                    ForwardedPorts.Add(local);
                    break;
            }
        }
        public void RemovePort()
        {
            if (_connectionStatusType == Helpers.ConnectionStatus.Disconnected)
            {
                ForwardedPorts.Remove(SelectedForwardedPort);

                _secureShellTunnelProvider.SetPortsSettings(ForwardedPorts);
                _settings.Ports = ForwardedPorts.ToList();

                SaveSettings();
            }
        }

        public void Connect()
        {
            if(_connectionStatusType == Helpers.ConnectionStatus.Disconnected)
                EstablishConnection();
        }
        public void Disconnect()
        {
            if(_connectionStatusType == Helpers.ConnectionStatus.Established)
                BreakConnection();
        }

        private void EstablishConnection()
        {
            _secureShellTunnelProvider.SetSecureShellSession(SecureShellSettings);

            _secureShellTunnelProvider.SetProxySettings(ProxySettings);

            _secureShellTunnelProvider.SetPortsSettings(ForwardedPorts);

            _settings.Ports = ForwardedPorts.ToList();

            SaveSettings();

            _secureShellTunnelProvider.Connect();

        }
        private void BreakConnection()
        {
            _secureShellTunnelProvider.Disconnect();
        }

        private void SaveSettings()
        {
            _settings.Save();
        }

        private void ConnectionStatusChanged(ConnectionStatus connectionStatusType)
        {
            _connectionStatusType = connectionStatusType;

            switch (connectionStatusType)
            {
                case Helpers.ConnectionStatus.Established:
                    ConnectionStatus = "Established";
                    break;
                case Helpers.ConnectionStatus.Failed:
                    ConnectionStatus = "Failed";
                    break;
                case Helpers.ConnectionStatus.Reconnecting:
                    ConnectionStatus = "Reconnecting";
                    break;
                case Helpers.ConnectionStatus.Connecting:
                    ConnectionStatus = "Connecting";
                    break;
                case Helpers.ConnectionStatus.Disconnected:
                    ConnectionStatus = "Disconnected";
                    break;
            }
        }

        #region Implementation of IDataErrorInfo

        public string this[string name]
        {
            get
            {
                string result = null;

                if (name == "ForwardedPort")
                {
                    if (ForwardedPort != null && !ForwardedPort.IsPort())
                        result = "Invalid port format";
                }

                return result;
            }
        }

        public string Error => null;

        #endregion
    }
}
