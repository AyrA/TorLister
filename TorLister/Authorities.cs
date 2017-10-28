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

            using (var WC = new WebClient())
            {
                string Lines;
#if DEBUG
                if (!File.Exists("tor_config.c"))
                {
                    Console.Error.WriteLine("Downloading source from repository...");
                    try
                    {
                        await WC.DownloadFileTaskAsync(new Uri(TOR_SOURCE), "tor_config.c");
                    }
                    catch
                    {
                        if (File.Exists("tor_config.c"))
                        {
                            File.Delete("tor_config.c");
                        }
                        return null;
                    }
                }
                else
                {
                    Console.Error.WriteLine("Loading Source from cache...");
                }
                Lines = File.ReadAllText("tor_config.c");
#else
                try
                {
                    Lines = await WC.DownloadStringTaskAsync(new Uri(TOR_SOURCE));
                }
                catch
                {
                    return null;
                }
#endif

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
                return Parts.Select(m => new Authority(m)).ToArray();
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
