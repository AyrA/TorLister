using System.Net;
using TorLister.Tools;

namespace TorLister.Tor
{
    [Serializable]
    public struct DirectoryEntry
    {
        public string? Name { get; set; }
        public string? SHA1 { get; set; }
        public IPEndPoint? OnionEP { get; set; }
        public IPEndPoint? HttpEP { get; set; }
        public string? Contact { get; set; }
        public string? Digest { get; set; }

        public DirectoryEntry(string SourceLine)
        {
            if (string.IsNullOrEmpty(SourceLine))
            {
                throw new ArgumentException("Invalid Source Line. Needs 7 Parts");
            }
            var Parts = SourceLine.Split(' ');

            if (Parts.Length == 7 && Parts[0] == "dir-source")
            {
                Parts = Parts.Skip(1).ToArray();
                Name = Parts[0];
                if (Utils.IsSHA1(Parts[1]))
                {
                    SHA1 = Parts[1].ToUpper();
                }
                else
                {
                    throw new ArgumentException("SHA1 Segment is not actually a SHA1 Hash");
                }

                //Ignore Invalid Endpoints for now. Some have DNS names instead of IP Addresses

                try
                {
                    HttpEP = Utils.ParseEP($"{Parts[2]}:{Parts[4]}");
                }
                catch
                {
                    HttpEP = null;
                }
                try
                {
                    OnionEP = Utils.ParseEP($"{Parts[3]}:{Parts[5]}");
                }
                catch
                {
                    OnionEP = null;
                }
            }
            else
            {
                throw new ArgumentException("Invalid Source Line. Needs 7 Parts");
            }
            Contact = null;
            Digest = null;
        }
    }
}
