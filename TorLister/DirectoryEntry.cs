using System;
using System.Net;

namespace TorLister
{
    public struct DirectoryEntry
    {
        public string Name;
        public string SHA1;
        public IPEndPoint OnionEP;
        public IPEndPoint HttpEP;
        public string Contact;
        public string Digest;

        public DirectoryEntry(string SourceLine)
        {
            if (string.IsNullOrEmpty(SourceLine))
            {
                throw new ArgumentException("Invalid Source Line. Needs 6 Parts");
            }
            var Parts = SourceLine.Split(' ');

            if (Parts.Length == 6)
            {
                Name = Parts[0];
                if (Tools.IsSHA1(Parts[1]))
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
                    HttpEP = Tools.ParseEP($"{Parts[2]}:{Parts[4]}");
                }
                catch
                {
                    HttpEP = null;
                }
                try
                {
                    OnionEP = Tools.ParseEP($"{Parts[3]}:{Parts[5]}");
                }
                catch
                {
                    OnionEP = null;
                }
            }
            else
            {
                throw new ArgumentException("Invalid Source Line. Needs 6 Parts");
            }


            Contact = null;
            Digest = null;
        }
    }
}
