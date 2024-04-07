using System.Diagnostics;
using System.Net;

namespace TorLister.Tor
{
    [Serializable]
    public struct TorNode
    {
        public string? Name;
        public byte[]? Hash;
        public DateTime OnlineSince = DateTime.MinValue;
        public IPAddress? IP = IPAddress.Any;
        public ushort OrPort;
        public ushort HttpPort;
        public string? m;
        public string[] Services = [];
        public string? TorVersion;
        public Dictionary<string, ProtocolVersion> Protocols = [];
        public int Bandwidth;

        public TorNode(string FirstLine)
        {
            RelayLine(FirstLine);
        }

        public void SetLine(string Line)
        {
            if (string.IsNullOrEmpty(Line) || !Line.Contains(' '))
            {
                throw new ArgumentException("Invalid Descriptor Line. Lacks a Code");
            }
            switch (Line[..Line.IndexOf(' ')])
            {
                case "r":
                    RelayLine(Line);
                    break;
                case "m":
                    m = Line[2..];
                    break;
                case "s":
                    Services = Line.Split(' ').Skip(1).ToArray();
                    break;
                case "v":
                    TorVersion = Line[2..];
                    break;
                case "pr":
                    ProtocolLine(Line);
                    break;
                case "w":
                    BandWidthLine(Line);
                    break;
                default:
                    break;
            }
        }

        public void RelayLine(string Line)
        {
            if (string.IsNullOrEmpty(Line) || !Line.StartsWith("r "))
            {
                throw new ArgumentException("Invalid TOR Node Line. Should Start with 'r' and a space");
            }
            var Segments = Line.Split(' ');
            if (Segments.Length == 8)
            {
                Name = Segments[1];
                Hash = Convert.FromBase64String(Segments[2] + "=");
                OnlineSince = DateTime.Parse($"{Segments[3]}T{Segments[4]}Z");
                IP = IPAddress.Parse(Segments[5]);
                OrPort = ushort.Parse(Segments[6]);
                HttpPort = ushort.Parse(Segments[7]);
            }
            else
            {
                throw new ArgumentException("Invalid TOR Node Line. Needs 8 Parts");
            }
        }

        public void ProtocolLine(string Line)
        {
            if (string.IsNullOrEmpty(Line) || !Line.StartsWith("pr "))
            {
                throw new ArgumentException("Invalid TOR Node Line. Should Start with 'pr' and a space");
            }

            Protocols = [];
            foreach (string s in Line[3..].Split(' '))
            {
                if (s.Contains('='))
                {
                    var Segments = s.Split('=');
                    if (Segments.Length == 2)
                    {
                        try
                        {
                            Protocols.Add(Segments[0], new ProtocolVersion(Segments[1]));
                        }
                        catch
                        {
                            Protocols.Add(Segments[0], new ProtocolVersion());
                        }
                    }
                }
            }

        }

        public void BandWidthLine(string Line)
        {
            if (string.IsNullOrEmpty(Line) || !Line.StartsWith("w "))
            {
                throw new ArgumentException("Invalid Bandwidth Line. Should Start with 'w' and a space");
            }
            foreach (string s in Line[2..].Split(' '))
            {
                if (s.Contains('='))
                {
                    var Segments = s.Split('=');
                    if (Segments[0] == "Bandwidth")
                    {
                        Bandwidth = int.Parse(Segments[1]);
                    }
                }
            }
        }
    }

    public class ProtocolVersion
    {
        public int[] Versions { get; set; }

        public ProtocolVersion()
        {
            Versions = [];
        }

        public ProtocolVersion(string versionString)
        {
            List<int> v = [];
            var parts = versionString.Split(',');
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }
                try
                {
                    //Add range
                    if (part.Contains('-'))
                    {
                        var range = part.Split('-');
                        var from = int.Parse(range[0]);
                        var to = int.Parse(range[1]);
                        for (var i = from; i <= to; i++)
                        {
                            v.Add(i);
                        }
                    }
                    else //Add single number
                    {
                        v.Add(int.Parse(part));
                    }
                }
                catch
                {
                    Debug.Print($"Failed to parse {part} as version or version range");
                }
            }
            Versions = [.. v.Distinct().Order()];
        }
    }
}
