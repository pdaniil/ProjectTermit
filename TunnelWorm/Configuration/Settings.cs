using TunnelWorm.Helpers;

namespace TunnelWorm.Configuration
{
    using System.IO;
    using System.Xml.Serialization;
    using System.Collections.Generic;

    using Models;

    public class Settings
    {
        #region Private Members

        [XmlIgnore]
        private const string FileName = "Settings.xml";

        #endregion

        public SecureShellSettings SecureShellSettings { get; set; } = new SecureShellSettings();
        public ProxySettings ProxySettings { get; set; } = new ProxySettings();
        public List<ForwardedPortModel> Ports { get; set; } = new List<ForwardedPortModel>();
        
        public void Load()
        {
            if (!File.Exists(FileName))
                return;

            var serializer = new XmlSerializer(typeof(Settings));

            Settings settings;
            using (var file = File.OpenText(FileName))
            {
                settings = (Settings)serializer.Deserialize(file);
            }

            SecureShellSettings = settings.SecureShellSettings;
            ProxySettings = settings.ProxySettings;
            Ports = settings.Ports;

            if (settings.SecureShellSettings.ProtectedHostPassword != null)
                settings.SecureShellSettings.HostPassword =settings.SecureShellSettings.ProtectedHostPassword.Unprotect();

            if (settings.ProxySettings.ProtectedProxyPasswd != null)
                settings.ProxySettings.ProxyPasswd = settings.ProxySettings.ProtectedProxyPasswd.Unprotect();
        }
        public void Save()
        {
            if (SecureShellSettings.HostPassword != null)
                SecureShellSettings.ProtectedHostPassword = SecureShellSettings.HostPassword.Protect();

            if (ProxySettings.ProxyPasswd != null)
                ProxySettings.ProtectedProxyPasswd = ProxySettings.ProxyPasswd.Protect();

            var x = new XmlSerializer(typeof(Settings));
            using (var writer = new StreamWriter(FileName))
            {
                x.Serialize(writer, this);
            }
        }
    }
}
