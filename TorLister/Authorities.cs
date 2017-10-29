using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TorLister
{
    public static class Authorities
    {
        public const string TOR_SOURCE = "https://gitweb.torproject.org/tor.git/plain/src/or/config.c";

        public static async Task<Authority[]> GetAuthoritiesAsync()
        {
            List<Authority> Ret = new List<Authority>();

            var Entry = Cache.Get("tor-source", TimeSpan.FromDays(1));

            if (Entry != null)
            {
                return Tools.Deserialize<Authority[]>(Entry.Data);
            }

            using (var WC = new WebClient())
            {
                string Lines;

                try
                {
                    Lines = await WC.DownloadStringTaskAsync(new Uri(TOR_SOURCE));
                }
                catch
                {
                    return null;
                }

                //Trim the source file to the part, that contains the authorities
                Lines = Lines.Substring(Lines.IndexOf("/** List of default directory authorities */"));
                Lines = Lines.Substring(Lines.IndexOf('"'));
                Lines = Lines.Substring(0, Lines.IndexOf("NULL") - 1).Trim(' ', '\r', '\n', ',', '\t');
                //remove all inline comments from the source
                while (Lines.Contains("/*"))
                {
                    Lines = Lines.Substring(0, Lines.IndexOf("/*")) +
                        Lines.Substring(Lines.IndexOf("*/", Lines.IndexOf("/*")) + 2);
                }
                //create parts
                string[] Parts = Lines.Split(',');
                for (int i = 0; i < Parts.Length; i++)
                {
                    //remove unneeded chars
                    foreach (char c in "\"\r\n")
                    {
                        Parts[i] = Parts[i].Replace(c.ToString(), "");
                    }
                    //remove unneeded whitespace
                    while (Parts[i].Contains("  "))
                    {
                        Parts[i] = Parts[i].Replace("  ", " ");
                    }
                    //remove more whitespace
                    Parts[i] = Parts[i].Trim();
                }
                var Authorities = Parts.Select(m => new Authority(m)).ToArray();
                Cache.Add("tor-source", Tools.Serialize(Authorities), true);
                return Authorities;
            }
        }

        public static Authority[] GetAuthorities()
        {
            try
            {
                return GetAuthoritiesAsync().Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
