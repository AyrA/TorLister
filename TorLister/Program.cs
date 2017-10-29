using System;
using System.Linq;

namespace TorLister
{
    class Program
    {
        static void Main(string[] args)
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
            }
            else
            {
                Console.Error.WriteLine("Taking consensus from random Authority");
                Consensus = new Directory(Auth.Random().DownloadNodes());
                Cache.Add("consensus", Tools.Serialize(Consensus), true);
            }

            Console.WriteLine("Flags Count:");
            foreach (var S in Consensus.KnownFlags)
            {
                Console.WriteLine("{0}={1}", S, Consensus.TorNodes.Where(m => m.Services.Contains(S)).Distinct().Count());
            }

            Console.WriteLine("#END");
            Console.ReadKey(true);
        }
    }
}
