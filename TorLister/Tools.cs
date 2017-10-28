using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TorLister
{
    public static class Tools
    {
        public static bool IsSHA1(string Hash)
        {
            return !string.IsNullOrEmpty(Hash) && Regex.IsMatch(Hash, @"[A-Fa-f\d]{40}");
        }

        public static IPEndPoint ParseEP(string S)
        {
            var IP = S.Substring(0, S.LastIndexOf(':')).Trim('[', ']');
            var Port = S.Substring(S.LastIndexOf(':') + 1);
            return new IPEndPoint(IPAddress.Parse(IP), int.Parse(Port));
        }

        public static bool IsIpV4Entry(string IpEntry)
        {
            IPAddress Addr = IPAddress.Any;
            ushort Port = 0;
            return
                !string.IsNullOrEmpty(IpEntry) &&
                //string has IP:Port
                IpEntry.Split(':').Length == 2 &&
                //Port is valid port number
                ushort.TryParse(IpEntry.Split(':')[1], out Port) &&
                //IP Address is valid IP
                IPAddress.TryParse(IpEntry.Split(':')[0], out Addr) &&
                //Port is not 0
                Port > 0 &&
                //IP is not 0.0.0.0
                Addr != IPAddress.Any &&
                //IP is IPv4
                Addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }

        public static bool IsKeyPart(string KeyPart)
        {
            return !string.IsNullOrEmpty(KeyPart) && Regex.IsMatch(KeyPart, @"[A-F\d]{4}");
        }

        public static bool IsPort(string Port)
        {
            ushort p = 0;
            return !string.IsNullOrEmpty(Port) && ushort.TryParse(Port, out p);
        }
    }
}
