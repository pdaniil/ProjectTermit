using System.Security;
using System.Xml;
using System.Xml.Serialization;
using Caliburn.Micro;

namespace Termit.Models
{
    using Helpers;

    public class SecureShellSettings : PropertyChangedBase
    {
        private int _hostPort;

        public string Name { get; set; }
        public string Description { get; set; }

        public string HostName { get; set; }
        public int HostPort
        {
            get { return _hostPort; }
            set
            {
                if (value.ToString().IsPort())
                    _hostPort = value;
            }
        }
        public string HostUsername { get; set; }

        [XmlIgnore]
        public SecureString HostPassword { get; set; }
        
        [XmlElement("HostPassword")]
        public string ProtectedHostPassword { get; set; }

        public bool Reconnect { get; set; }
    }
}