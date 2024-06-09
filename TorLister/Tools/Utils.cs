using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TorLister.Tools
{
    /// <summary>
    /// Generic Tools
    /// </summary>
    public static partial class Utils
    {
        private static readonly string appPath = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly JsonSerializerOptions jsonOpt;

        public static string AppPath => appPath;

        static Utils()
        {
            jsonOpt = new(JsonSerializerOptions.Default);
            jsonOpt.Converters.Add(new IPEndPointConverter());
            jsonOpt.Converters.Add(new IPAddressConverter());
        }


        /// <summary>
        /// Checks if the given string is a SHA1 hash
        /// </summary>
        /// <param name="Hash">SHA1 Hash</param>
        /// <returns>true if valid Hash</returns>
        public static bool IsSHA1(string Hash)
        {
            return !string.IsNullOrEmpty(Hash) && HashMatcher().IsMatch(Hash);
        }

        /// <summary>
        /// Parses IP:Port into an IP Endpoint class
        /// </summary>
        /// <param name="S">IP Endpoint String</param>
        /// <returns>IP Endpoint class</returns>
        public static IPEndPoint ParseEP(string S)
        {
            var IP = S[..S.LastIndexOf(':')].Trim('[', ']');
            var Port = S[(S.LastIndexOf(':') + 1)..];
            return new IPEndPoint(IPAddress.Parse(IP), int.Parse(Port));
        }

        /// <summary>
        /// Checks if the given string is a valid IPv4 Endpoint
        /// </summary>
        /// <param name="ipEntry">IPv4 Endpoint string</param>
        /// <remarks>This will not make connection attempts</remarks>
        /// <returns>true if valid IPv4 Endpoint</returns>
        public static bool IsIpV4Entry(string ipEntry)
        {
            if (string.IsNullOrWhiteSpace(ipEntry))
            {
                return false;
            }
            var parts = ipEntry.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            return
                //Port is valid port number
                ushort.TryParse(parts[1], out ushort Port) &&
                //IP Address is valid IP
                IPAddress.TryParse(parts[0], out IPAddress? addr) &&
                //Port is not 0
                Port > 0 &&
                //IP is not 0.0.0.0
                addr != IPAddress.Any &&
                //IP is IPv4
                addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }

        /// <summary>
        /// Deserializes a Byte Array into a <see cref="SerializableAttribute"/> marked Object
        /// </summary>
        /// <typeparam name="T">Object to Deserialize</typeparam>
        /// <param name="Data">Data to Deserialize</param>
        /// <returns>Deserialized Object</returns>
        public static T? Deserialize<T>(byte[] Data)
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(Data), jsonOpt);
        }

        /// <summary>
        /// Serializes <see cref="SerializableAttribute"/> marked objects into byte arrays
        /// </summary>
        /// <param name="Data">Object to Serialize</param>
        /// <returns>Serialized Object</returns>
        public static byte[] Serialize<T>(T Data)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Data, jsonOpt));
        }

        /// <summary>
        /// Checks if the given string is a Key Part
        /// </summary>
        /// <param name="keyPart">Key Part string</param>
        /// <returns>true if valid Key Part</returns>
        public static bool IsKeyPart(string keyPart)
        {
            return !string.IsNullOrEmpty(keyPart) && KeyPartMatcher().IsMatch(keyPart);
        }

        /// <summary>
        /// Checks if the given String is a valid Port Number
        /// </summary>
        /// <param name="port">Port Number</param>
        /// <remarks>This allows 0</remarks>
        /// <returns>true if Port Number</returns>
        public static bool IsPort(string port)
        {
            return !string.IsNullOrEmpty(port) && ushort.TryParse(port, out _);
        }

        [GeneratedRegex(@"^[A-Fa-f\d]{40}$")]
        private static partial Regex HashMatcher();
        [GeneratedRegex(@"[A-F\d]{4}")]
        private static partial Regex KeyPartMatcher();
    }
}
