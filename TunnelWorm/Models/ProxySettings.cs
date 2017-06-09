using System.Security;
using System.Xml.Serialization;
using Caliburn.Micro;

namespace Termit.Models
{
    using Helpers;

    public class ProxySettings : PropertyChangedBase
    {
        public ProxySettings()
        {
            ProxyAuthMethod = ProxyAuthMethod.Password; //ToDo: implement key auth
        }
        public bool UseProxy { get; set; }
        public ProxyAuthMethod ProxyAuthMethod { get; set; }
        public ProxyTypes ProxyType { get; set; }
        public string ProxyHostName { get; set; }

        private int _proxyPort;
        public int ProxyPort
        {
            get { return _proxyPort; }
            set
            {
                if (value.ToString().IsPort())
                {
                    _proxyPort = value;
                }
            }
        }
        public string ProxyUsername { get; set; }


        [XmlIgnore]
        public SecureString ProxyPasswd { get; set; }

        [XmlElement("HostPassword")]
        public string ProtectedProxyPasswd { get; set; }
    }
}
