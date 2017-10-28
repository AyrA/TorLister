using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace TorLister
{
    public struct TorNode
    {
        public string Name;
        public byte[] Hash;
        public DateTime OnlineSince;
        public IPAddress IP;
        public ushort OrPort;
        public ushort HttpPort;
        public string m;
        public string[] Services;
        public string TorVersion;
        public Dictionary<string, Version> Protocols;
        public int Bandwidth;

        public TorNode(string FirstLine)
        {
            Name = null;
            Hash = null;
            OnlineSince = DateTime.MinValue;
            IP = IPAddress.Any;
            OrPort = 0;
            HttpPort = 0;
            m = null;
            Services = new string[0];
            TorVersion = null;
            Protocols = new Dictionary<string, Version>();
            Bandwidth = 0;
            RelayLine(FirstLine);
        }

        public void SetLine(string Line)
        {
            if (string.IsNullOrEmpty(Line) || !Line.Contains(" "))
            {
                throw new ArgumentException("Invalid Descriptor Line. Lacks a Code");
            }
            switch (Line.Substring(0, Line.IndexOf(' ')))
            {
                case "r":
                    RelayLine(Line);
                    break;
                case "m":
                    m = Line.Substring(2);
                    break;
                case "s":
                    Services = Line.Split(' ').Skip(1).ToArray();
                    break;
                case "v":
                    TorVersion = Line.Substring(2);
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
                Hash = Convert.FromBase64String(Segments[2]);
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

            Protocols = new Dictionary<string, Version>();
            foreach (string s in Line.Substring(3).Split(' '))
            {
                if (s.Contains("="))
                {
                    var Segments = s.Split('=');
                    if (Segments.Length == 2)
                    {
                        Protocols.Add(Segments[0], new Version(Segments[1]));
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
            foreach (string s in Line.Substring(2).Split(' '))
            {
                if (s.Contains("="))
                {
                    var Segments = s.Split('=');
                    if (Segments[0] == "Bandwidth")
                    {
                        Bandwidth=int.Parse(Segments[1]);
                    }
                }
            }
        }
    }
}
