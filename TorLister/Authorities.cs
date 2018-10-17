using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TorLister
{
    public static class Authorities
    {
        public const string TOR_SOURCE = "https://gitweb.torproject.org/tor.git/plain/src/app/config/auth_dirs.inc";

        public static async Task<Authority[]> GetAuthoritiesAsync()
        {
            List<Authority> Ret = new List<Authority>();

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
                    foreach (string s in new string[] { "\"", "\r", "\n" })
                    {
                        Parts[i] = Parts[i].Replace(s, "");
                    }
                    //remove unneeded whitespace
                    while (Parts[i].Contains("  "))
                    {
                        Parts[i] = Parts[i].Replace("  ", " ");
                    }
                    //remove more whitespace
                    Parts[i] = Parts[i].Trim();
                }
                return Parts.Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => new Authority(m)).ToArray();
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
