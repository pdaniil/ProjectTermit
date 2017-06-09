using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Renci.SshNet;
using Termit.Helpers;

namespace Termit.Models
{
    public class ForwardedPortModel
    {
        public ForwardedPortModel()
        {
            
        }

        public ForwardedPortModel(TunnelTypes type, uint localPort, string localhost = "", uint remotePort = 0, string remoteHost = "")
        {
            Type = type;
            ForwardedPort = localPort;
            ForwardedAddress = localhost;
            RemotePort = remotePort;
            RemoteAddress = remoteHost;
        }

        public TunnelTypes Type { get; set; }
        public uint ForwardedPort { get; set; }
        public uint RemotePort { get; set; }
        public string ForwardedAddress { get; set; }
        public string RemoteAddress { get; set; }

        [XmlIgnore]
        public ForwardedPort NativePort { get; set; }

        public void Start()
        {
            NativePort.Start();
        }

        public void Init()
        {
            if (Type == TunnelTypes.Dynamic)
            {
                NativePort = string.IsNullOrEmpty(ForwardedAddress) ? new ForwardedPortDynamic(ForwardedPort) : new ForwardedPortDynamic(ForwardedAddress, ForwardedPort);
            }
            else if (Type == TunnelTypes.Remote)
            {
                var remoteIp = IPAddress.Parse(RemoteAddress);
                var localIp = IPAddress.Parse(ForwardedAddress);
                NativePort = new ForwardedPortRemote(localIp, ForwardedPort, remoteIp, RemotePort);
            }
            else
            {
                NativePort = new ForwardedPortLocal(ForwardedPort, RemoteAddress, ForwardedPort);
            }
        }

        public void Stop()
        {
            NativePort.Stop();
        }
    }
}
