using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TorLister.Tools;

namespace TorLister.Tor
{
    /// <summary>
    /// Represents a TOR Authority
    /// </summary>
    [Serializable]
    public partial class Authority
    {
        /// <summary>
        /// HTTP Path to get all Nodes
        /// </summary>
        public const string TOR_DIR = "/tor/status/all.z";

        /// <summary>
        /// HTTP Path to get All Nodes in a condensed Format
        /// </summary>
        public const string TOR_CONSENSUS = "/tor/status-vote/current/consensus-microdesc.z";

        /// <summary>
        /// Gets or Sets the Authority Name
        /// </summary>
        public string? Name
        { get; set; }

        /// <summary>
        /// Gets or Sets the Port for TOR Connections
        /// </summary>
        public int Port
        { get; set; }

        /// <summary>
        /// Gets or Sets the V3 Identity SHA1 Hash
        /// </summary>
        public string? Identity
        { get; set; }

        /// <summary>
        /// Gets or Sets if this is a Bridge
        /// </summary>
        public bool Bridge
        { get; set; }

        /// <summary>
        /// Gets or Sets the IPv4 Endpoint for HTTP Connections
        /// </summary>
        public IPEndPoint? IPv4Endpoint
        { get; set; }

        /// <summary>
        /// Gets or Sets the IPv6 Endpoint for HTTP Connections
        /// </summary>
        public IPEndPoint? IPv6Endpoint
        { get; set; }

        /// <summary>
        /// Gets or Sets the SHA1 Key
        /// </summary>
        public string? Key
        { get; set; }

        /// <summary>
        /// Create empty Authority
        /// </summary>
        public Authority()
        {
        }

        /// <summary>
        /// Parse Line from tor source into Authority
        /// </summary>
        /// <param name="ConfigLine">Line from Source code</param>
        public Authority(string ConfigLine)
        {
            var Segments = ConfigLine.Split(' ');
            List<string> KeySegments = [];

            foreach (var Segment in Segments)
            {
                //First entry is name
                if (Name == null)
                {
                    Name = Segment;
                }
                else if (Segment.Contains('='))
                {
                    var Key = Segment[..Segment.IndexOf('=')];
                    var Value = Segment[(Key.Length + 1)..];
                    switch (Key.ToLower())
                    {
                        case "orport":
                            if (Utils.IsPort(Value))
                            {
                                Port = int.Parse(Value);
                            }
                            break;
                        case "v3ident":
                            if (Utils.IsSHA1(Value))
                            {
                                Identity = Value.ToUpper();
                            }
                            break;
                        case "ipv6":
                            try
                            {
                                IPv6Endpoint = Utils.ParseEP(Value);

                            }
                            catch (Exception ex)
                            {
                                //Throw proper Error message
                                throw new Exception($"Invalid IPv6 Endpoint segment: {Value}", ex);
                            }
                            break;
                        default:
                            //Unsupported entry type. We should eventually throw exceptions here
                            break;
                    }
                }
                else if (Segment == "bridge")
                {
                    Bridge = true;
                }
                else if (Utils.IsIpV4Entry(Segment))
                {
                    IPv4Endpoint = Utils.ParseEP(Segment);
                }
                else if (Utils.IsKeyPart(Segment))
                {
                    KeySegments.Add(Segment);
                }
            }
            Key = string.Join(" ", KeySegments).ToUpper();
            Key = WhitespaceFilter().Replace(Key, "");
            //Throw Exception if invalid ID
            Validate(true);
        }

        /// <summary>
        /// Downloads List of all Tor nodes
        /// </summary>
        /// <returns>Node File</returns>
        public async Task<string> DownloadNodesAsync()
        {
            Validate(true);

            if (IPv6Endpoint != null)
            {
                using var WC = new HttpClient();
                try
                {
                    return Decompress(await WC.GetByteArrayAsync($"http://{IPv6Endpoint}{TOR_CONSENSUS}"));
                }
                catch
                {
                    //Unable to Download, try IPv4
                }
            }
            if (IPv4Endpoint != null)
            {
                using var WC = new HttpClient();
                return Decompress(await WC.GetByteArrayAsync($"http://{IPv4Endpoint}{TOR_CONSENSUS}"));
            }
            throw new Exception("Can't download from either IPv4 or IPv6");
        }

        /// <summary>
        /// Downloads List of all TOR Nodes
        /// </summary>
        /// <returns>Node File</returns>
        public string DownloadNodes()
        {
            var T = DownloadNodesAsync();
            try
            {
                T.Wait();
            }
            catch
            {
            }
            if (T.IsFaulted)
            {
                throw T.Exception.InnerExceptions.First();
            }
            return T.Result;
        }

        /// <summary>
        /// Validates the Authority Entry
        /// </summary>
        /// <param name="Throw">Throw first exception on validation error</param>
        /// <returns>true if valid</returns>
        [MemberNotNullWhen(true, nameof(Name), nameof(Key))]
        public bool Validate(bool Throw = false)
        {
            if (Throw)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    throw new Exception("Name is not defined");
                }
                if (Port <= 0 || Port > IPEndPoint.MaxPort)
                {
                    throw new Exception("orport is outside of valid bounds");
                }
                if (IPv4Endpoint == null && IPv6Endpoint == null)
                {
                    throw new Exception("IPv4 Endpoint and IPv6 Endpoint are both not defined.");
                }
                if (string.IsNullOrEmpty(Key))
                {
                    throw new Exception("Key is not defined");
                }
                if (Key.Length != 40 || Key.Split(' ').Any(m => !Utils.IsSHA1(m)))
                {
                    throw new Exception("Key is invalid. See inner exception for Details", new FormatException("Expected format: 10 groups of 4 hex digits separated by spaces"));
                }
            }
            return !string.IsNullOrEmpty(Name) &&
                Port > 0 &&
                Port <= IPEndPoint.MaxPort &&
                (IPv4Endpoint != null || IPv6Endpoint != null) &&
                !string.IsNullOrEmpty(Key);
        }

        /// <summary>
        /// Decompresses zlib compressed String Data
        /// </summary>
        /// <param name="Data">Data</param>
        /// <returns>Decompressed String</returns>
        private static string Decompress(byte[] Data)
        {
            using var IN = new MemoryStream(Data, false);
            using var OUT = new MemoryStream();
            using var Comp = new System.IO.Compression.ZLibStream(IN, System.IO.Compression.CompressionMode.Decompress);
            Comp.CopyTo(OUT);
            return Encoding.UTF8.GetString(OUT.ToArray());
        }

        /// <summary>
        /// Converts this instance to an authority line
        /// </summary>
        /// <remarks>This is essentially serializing</remarks>
        /// <returns>Authority line</returns>
        public override string ToString()
        {
            if (!Validate(true))
            {
                throw new InvalidOperationException("Validation failed");
            }

            List<string> Segments = [Name, $"orport={Port}"];

            if (Bridge)
            {
                Segments.Add("bridge");
            }
            if (!string.IsNullOrEmpty(Identity))
            {
                Segments.Add($"v3ident={Identity}");
            }
            if (IPv6Endpoint != null)
            {
                Segments.Add($"ipv6={IPv6Endpoint}");
            }
            if (IPv4Endpoint != null)
            {
                Segments.Add(IPv4Endpoint.ToString());
            }

            Segments.AddRange(Key.ToUpper().Split(' '));

            return string.Join(" ", Segments);
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceFilter();
    }
}