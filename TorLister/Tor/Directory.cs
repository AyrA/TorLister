using System.Diagnostics;
using System.Text;

namespace TorLister.Tor
{
    [Serializable]
    public class Directory
    {
        /// <summary>
        /// Gets the Vote Status
        /// </summary>
        /// <remarks>This should always be "consensus"</remarks>
        public string VoteStatus
        { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the Consensus Method
        /// </summary>
        /// <remarks>This should always be 26</remarks>
        public int ConsensusMethod
        { get; private set; }

        /// <summary>
        /// Gets the Date after which this consensus is valid
        /// </summary>
        public DateTime ValidAfter
        { get; private set; }

        /// <summary>
        /// Gets the Date until this Consensus is considered fresh
        /// </summary>
        /// <remarks>Use this to figure out when to refresh the cache</remarks>
        public DateTime FreshUntil
        { get; private set; }

        /// <summary>
        /// Gets the Date after which this Consensus is considered outdated and invalid
        /// </summary>
        /// <remarks>Unless no Network Connection is available you should really reload after this time</remarks>
        public DateTime ValidUntil
        { get; private set; }

        /// <summary>
        /// Gets the two voting Delay Values
        /// </summary>
        public int[] VotingDelays
        { get; private set; } = [];

        /// <summary>
        /// Gets all Supported Client Versions
        /// </summary>
        public string[] ClientVersions
        { get; private set; } = [];

        /// <summary>
        /// Gets all Supported Server Versions
        /// </summary>
        public string[] ServerVersions
        { get; private set; } = [];

        /// <summary>
        /// Gets all known Service Flags
        /// </summary>
        public string[] KnownFlags
        { get; private set; } = [];

        /// <summary>
        /// Gets the Recommended Client Versions for Services
        /// </summary>
        public Dictionary<string, ProtocolVersion> RecommendedClientVersions
        { get; private set; } = [];

        /// <summary>
        /// Gets the Recommended Relay Versions for Services
        /// </summary>
        public Dictionary<string, ProtocolVersion> RecommendedRelayVersions
        { get; private set; } = [];

        /// <summary>
        /// Gets the Required Client Versions for Services
        /// </summary>
        public Dictionary<string, ProtocolVersion> RequiredClientVersions
        { get; private set; } = [];

        /// <summary>
        /// Gets the Required Relay Versions for Services
        /// </summary>
        public Dictionary<string, ProtocolVersion> RequiredRelayVersions
        { get; private set; } = [];

        /// <summary>
        /// Gets the Parameters
        /// </summary>
        public Dictionary<string, int> Params
        { get; private set; } = [];

        /// <summary>
        /// Gets the previous shared Random Value
        /// </summary>
        public RandValue SharedRandPreviousValue
        { get; private set; } = new();

        /// <summary>
        /// Gets the current shared Random Value
        /// </summary>
        public RandValue SharedRandCurrentValue
        { get; private set; } = new();

        /// <summary>
        /// Gets the Various Bandwidth Weights
        /// </summary>
        public Dictionary<string, int> BandwidthWeights
        { get; private set; } = [];

        /// <summary>
        /// Gets a List of Authorities that can produce this Consensus
        /// </summary>
        public DirectoryEntry[] DirectorySources
        { get; private set; } = [];

        /// <summary>
        /// Gets a List of all Tor Nodes
        /// </summary>
        public TorNode[] TorNodes
        { get; private set; } = [];

        /// <summary>
        /// Gets all signatures
        /// </summary>
        public DirectorySignature[] Signatures
        { get; private set; } = [];

        /// <summary>
        /// Parses a Network Consensus
        /// </summary>
        /// <param name="DirectorySource">Network Consensus</param>
        public Directory(string DirectorySource)
        {
            var sources = new List<DirectoryEntry>();
            var nodes = new List<TorNode>();
            var sig = new List<DirectorySignature>();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(DirectorySource.Replace("\n", "\r\n")), false))
            {
                using var reader = new StreamReader(ms);
                //Verify Version
                if (reader.ReadLine() != "network-status-version 3 microdesc")
                {
                    throw new ArgumentException("Directory Source doesn't looks like a Version 3 Microdescriptor");
                }
                while (!reader.EndOfStream)
                {
                    Console.Write('.');
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    var Segments = line.Split(' ');
                    try
                    {
                        switch (Segments[0])
                        {
                            case "vote-status":
                                VoteStatus = Segments[1];
                                break;
                            case "consensus-method":
                                ConsensusMethod = int.Parse(Segments[1]);
                                break;
                            case "valid-after":
                                ValidAfter = DateTime.Parse($"{Segments[1]}T{Segments[2]}Z");
                                break;
                            case "valid-until":
                                ValidUntil = DateTime.Parse($"{Segments[1]}T{Segments[2]}Z");
                                break;
                            case "fresh-until":
                                FreshUntil = DateTime.Parse($"{Segments[1]}T{Segments[2]}Z");
                                break;
                            case "voting-delay":
                                VotingDelays = [int.Parse(Segments[1]), int.Parse(Segments[2])];
                                break;
                            case "client-versions":
                                ClientVersions = Segments[1].Split(',');
                                break;
                            case "server-versions":
                                ServerVersions = Segments[1].Split(',');
                                break;
                            case "known-flags":
                                KnownFlags = Segments.Skip(1).ToArray();
                                break;
                            case "recommended-client-protocols":
                                RecommendedClientVersions = [];
                                AddVersions(RecommendedClientVersions, Segments.Skip(1));
                                break;
                            case "recommended-relay-protocols":
                                RecommendedRelayVersions = [];
                                AddVersions(RecommendedRelayVersions, Segments.Skip(1));
                                break;
                            case "required-client-protocols":
                                RequiredClientVersions = [];
                                AddVersions(RequiredClientVersions, Segments.Skip(1));
                                break;
                            case "required-relay-protocols":
                                RequiredRelayVersions = [];
                                AddVersions(RequiredRelayVersions, Segments.Skip(1));
                                break;
                            case "params":
                                Params = [];
                                foreach (var P in Segments.Skip(1))
                                {
                                    Params.Add(P.Split('=')[0], int.Parse(P.Split('=')[1]));
                                }
                                break;
                            case "shared-rand-previous-value":
                                SharedRandPreviousValue = new RandValue()
                                {
                                    RandomValue = int.Parse(Segments[1]),
                                    RandomNonce = Convert.FromBase64String(Segments[2])
                                };
                                break;
                            case "shared-rand-current-value":
                                SharedRandCurrentValue = new RandValue()
                                {
                                    RandomValue = int.Parse(Segments[1]),
                                    RandomNonce = Convert.FromBase64String(Segments[2])
                                };
                                break;
                            case "dir-source":
                                sources.Add(new DirectoryEntry(line)
                                {
                                    Contact = ReadLine(reader)[8..],
                                    Digest = ReadLine(reader)[12..]
                                });
                                break;
                            case "bandwidth-weights":
                                BandwidthWeights = [];
                                foreach (var Weight in Segments.Skip(1))
                                {
                                    BandwidthWeights.Add(Weight.Split('=')[0], int.Parse(Weight.Split('=')[1]));
                                }
                                break;
                            case "directory-signature":
                                sig.Add(LoadSig(reader, line));
                                break;
                            case "r":
                                nodes.Add(ReadNode(reader, line));
                                break;
                            case "directory-footer":
                                //Don't care
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Print("Failed to parse '{0}' as valid line.", line);
                        Debug.Print("[{0}]: {1}", ex.GetType().Name, ex.Message);
                    }
                }
            }

            DirectorySources = [.. sources];
            TorNodes = [.. nodes];
            Signatures = [.. sig];
        }

        /// <summary>
        /// For deserialization purposes
        /// </summary>
        public Directory() { }

        private static DirectorySignature LoadSig(StreamReader reader, string line)
        {
            var Sig = new DirectorySignature(line.Split(' ').Skip(1).ToArray());
            if (reader.ReadLine() == "-----BEGIN SIGNATURE-----")
            {
                Sig.Signature = "";
                while (!reader.EndOfStream)
                {
                    var SigLine = reader.ReadLine();
                    if (SigLine == "-----END SIGNATURE-----")
                    {
                        return Sig;
                    }
                    else
                    {
                        Sig.Signature += SigLine;
                    }
                }
            }
            throw new Exception("Directory Signature misses Signature Block");
        }

        private static TorNode ReadNode(StreamReader reader, string line)
        {
            var Node = new TorNode(line);
            for (var i = 0; i < 5; i++)
            {
                Node.SetLine(ReadLine(reader));
            }
            return Node;
        }

        private static void AddVersions(Dictionary<string, ProtocolVersion> dict, IEnumerable<string> versions)
        {
            foreach (string s in versions)
            {
                try
                {
                    dict.Add(s.Split('=')[0], new ProtocolVersion(s.Split('=')[1]));
                }
                catch
                {
                    //Invalid version string. Skip
                }
            }
        }

        private static string ReadLine(StreamReader reader)
        {
            return reader.ReadLine() ?? throw new IOException("Unexpected EOF");
        }
    }
}
