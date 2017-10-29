using System;
using System.Linq;

namespace TorLister
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Authority[] Auth;
            Directory Consensus;

            var CacheEntry = Cache.Get("authorities", TimeSpan.FromDays(1));

            if (CacheEntry != null)
            {
                Console.Error.WriteLine("Taking auth from cache");
                Auth = Tools.Deserialize<Authority[]>(CacheEntry.Data);
            }
            else
            {
                Console.Error.WriteLine("Taking auth live");
                Auth = Authorities.GetAuthorities();
                Cache.Add("authorities", Tools.Serialize(Auth), true);
            }

            CacheEntry = Cache.Get("consensus");
            if (CacheEntry != null)
            {
                Console.Error.WriteLine("Taking consensus from cache");
                Consensus = Tools.Deserialize<Directory>(CacheEntry.Data);
                if (Consensus.ValidUntil < DateTime.UtcNow)
                {
                    Console.Error.WriteLine("Consensus is outdated. Renewing from random Authority");
                    Consensus = new Directory(Auth.Random().DownloadNodes());
                    Cache.Add("consensus", Tools.Serialize(Consensus), true);
                }
                else
                {
                    Console.Error.WriteLine("Consensus valid until {0} UTC", Consensus.ValidUntil);
                }
            }
            else
            {
                Console.Error.WriteLine("Taking consensus from random Authority");
                Consensus = new Directory(Auth.Random().DownloadNodes());
                Cache.Add("consensus", Tools.Serialize(Consensus), true);
            }

            if (args.Length == 0)
            {
                args = new string[] { "/?" };
            }
            if (args[0].ToLower() == "/flags")
            {
                Console.WriteLine(string.Join("\n", Consensus.KnownFlags));
            }
            else if (args[0].ToLower() == "/all")
            {
                Console.WriteLine(string.Join("\n", Consensus.TorNodes.Select(m => m.IP.ToString())));
            }
            else if (args[0].ToLower() == "/flag" && args.Length > 1)
            {

                var UserFlags = args[1].Split(',').Select(m => m.Trim().ToLower()).ToArray();
                var SystemFlags = Consensus.KnownFlags.Select(m => m.ToLower()).ToArray();
                if (UserFlags.All(m => SystemFlags.Contains(m)))
                {
                    foreach (var Node in Consensus.TorNodes)
                    {
                        if (Node.Services.Any(m => UserFlags.Contains(m.ToLower())))
                        {
                            Console.Error.WriteLine(Node.IP);
                        }
                    }
                }
                else
                {
                    Console.Error.WriteLine("Invalid Flag specified");
                }
            }
            else
            {
                Console.Error.WriteLine("TorLister /all | /flags | /flag flag[,...]");
            }
#if DEBUG
            Console.WriteLine("#END");
            Console.ReadKey(true);
#endif
        }
    }
}
