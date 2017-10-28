using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TorLister
{
    public class Directory
    {
        public string VoteStatus
        { get; private set; }

        public int ConsensusMethod
        { get; private set; }

        public DateTime ValidAfter
        { get; private set; }

        public DateTime FreshUntil
        { get; private set; }

        public DateTime ValidUntil
        { get; private set; }

        public int[] VotingDelays
        { get; private set; }

        public string[] ClientVersions
        { get; private set; }

        public string[] ServerVersions
        { get; private set; }

        public string[] KnownFlags
        { get; private set; }

        public Dictionary<string, Version> RecommendedClientVersions
        { get; private set; }

        public Dictionary<string, Version> RecommendedRelayVersions
        { get; private set; }

        public Dictionary<string, Version> RequiredClientVersions
        { get; private set; }

        public Dictionary<string, Version> RequiredRelayVersions
        { get; private set; }

        public Dictionary<string, int> Params
        { get; private set; }

        public RandValue SharedRandPreviousValue
        { get; private set; }

        public RandValue SharedRandCurrentValue
        { get; private set; }

        public Dictionary<string, int> BandwidthWeights
        { get; private set; }

        public DirectoryEntry[] DirectorySources
        { get; private set; }

        public TorNode[] TorNodes
        { get; private set; }

        public DirectorySignature[] Signatures
        { get; private set; }

        /// <summary>
        /// Parses a Network Consensus
        /// </summary>
        /// <param name="DirectorySource">Network Consensus</param>
        public Directory(string DirectorySource)
        {
            var Sources = new List<DirectoryEntry>();
            var Nodes = new List<TorNode>();
            var Sig = new List<DirectorySignature>();

            using (var MS = new MemoryStream(Encoding.UTF8.GetBytes(DirectorySource.Replace("\n", "\r\n")), false))
            {
                using (var SR = new StreamReader(MS))
                {
                    //Verify Version
                    if (SR.ReadLine() != "network-status-version 3 microdesc")
                    {
                        throw new ArgumentException("Directory Source doesn't looks like a Version 3 Microdescriptor");
                    }
                    while (!SR.EndOfStream)
                    {
                        var Line = SR.ReadLine();
                        var Segments = Line.Split(' ');
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
                                VotingDelays = new int[] { int.Parse(Segments[1]), int.Parse(Segments[2]) };
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
                                RecommendedClientVersions = new Dictionary<string, Version>();
                                AddVersions(RecommendedClientVersions, Segments.Skip(1));
                                break;
                            case "recommended-relay-protocols":
                                RecommendedRelayVersions = new Dictionary<string, Version>();
                                AddVersions(RecommendedRelayVersions, Segments.Skip(1));
                                break;
                            case "required-client-protocols":
                                RequiredClientVersions = new Dictionary<string, Version>();
                                AddVersions(RequiredClientVersions, Segments.Skip(1));
                                break;
                            case "required-relay-protocols":
                                RequiredRelayVersions = new Dictionary<string, Version>();
                                AddVersions(RequiredRelayVersions, Segments.Skip(1));
                                break;
                            case "params":
                                Params = new Dictionary<string, int>();
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
                                Sources.Add(new DirectoryEntry(Line)
                                {
                                    Contact = SR.ReadLine().Substring(8),
                                    Digest = SR.ReadLine().Substring(12)
                                });
                                break;
                            case "bandwidth-weights":
                                BandwidthWeights = new Dictionary<string, int>();
                                foreach (var Weight in Segments.Skip(1))
                                {
                                    BandwidthWeights.Add(Weight.Split('=')[0], int.Parse(Weight.Split('=')[1]));
                                }
                                break;
                            case "directory-signature":
                                Sig.Add(LoadSig(SR, Line));
                                break;
                            case "r":
                                Nodes.Add(ReadNode(SR, Line));
                                break;
                            case "directory-footer":
                                //Don't care
                                break;
                        }
                    }
                }
            }

            DirectorySources = Sources.ToArray();
            TorNodes = Nodes.ToArray();
        }

        private DirectorySignature LoadSig(StreamReader SR, string Line)
        {
            var Sig = new DirectorySignature(Line.Split(' ').Skip(1).ToArray());
            if (SR.ReadLine() == "-----BEGIN SIGNATURE-----")
            {
                Sig.Signature = "";
                while (!SR.EndOfStream)
                {
                    var SigLine = SR.ReadLine();
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

        private TorNode ReadNode(StreamReader SR, string Line)
        {
            var Node = new TorNode(Line);
            for (var i = 0; i < 5; i++)
            {
                Node.SetLine(SR.ReadLine());
            }
            return Node;
        }

        private void AddVersions(Dictionary<string, Version> Dict, IEnumerable<string> Versions)
        {
            foreach (string s in Versions)
            {
                Dict.Add(s.Split('=')[0], new Version(s.Split('=')[1]));
            }
        }
    }
}
