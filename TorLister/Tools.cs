﻿using System.Net;
using System.Text.RegularExpressions;

namespace TorLister
{
    /// <summary>
    /// Generic Tools
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Checks if the given string is a SHA1 hash
        /// </summary>
        /// <param name="Hash">SHA1 Hash</param>
        /// <returns>true if valid Hash</returns>
        public static bool IsSHA1(string Hash)
        {
            return !string.IsNullOrEmpty(Hash) && Regex.IsMatch(Hash, @"[A-Fa-f\d]{40}");
        }

        /// <summary>
        /// Parses IP:Port into an IP Endpoint class
        /// </summary>
        /// <param name="S">IP Endpoint String</param>
        /// <returns>IP Endpoint class</returns>
        public static IPEndPoint ParseEP(string S)
        {
            var IP = S.Substring(0, S.LastIndexOf(':')).Trim('[', ']');
            var Port = S.Substring(S.LastIndexOf(':') + 1);
            return new IPEndPoint(IPAddress.Parse(IP), int.Parse(Port));
        }

        /// <summary>
        /// Checks if the given string is a valid IPv4 Endpoint
        /// </summary>
        /// <param name="IpEntry">IPv4 Endpoint string</param>
        /// <remarks>This will not make connection attempts</remarks>
        /// <returns>true if valid IPv4 Endpoint</returns>
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

        /// <summary>
        /// Checks if the given string is a Key Part
        /// </summary>
        /// <param name="KeyPart">Key Part string</param>
        /// <returns>true if valid Key Part</returns>
        public static bool IsKeyPart(string KeyPart)
        {
            return !string.IsNullOrEmpty(KeyPart) && Regex.IsMatch(KeyPart, @"[A-F\d]{4}");
        }

        /// <summary>
        /// Checks if the given String is a valid Port Number
        /// </summary>
        /// <param name="Port">Port Number</param>
        /// <remarks>This allows 0</remarks>
        /// <returns>true if Port Number</returns>
        public static bool IsPort(string Port)
        {
            ushort p = 0;
            return !string.IsNullOrEmpty(Port) && ushort.TryParse(Port, out p);
        }
    }
}
