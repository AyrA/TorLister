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
                Auth = Tools.Deserialize<Authority[]>(CacheEntry.Data);
            }
            else
            {
                Auth = Authorities.GetAuthorities();
                Cache.Add("authorities", Tools.Serialize(Auth), true);
            }

            CacheEntry = Cache.Get("consensus");
            if (CacheEntry != null)
            {
                Consensus = Tools.Deserialize<Directory>(CacheEntry.Data);
                if (Consensus.ValidUntil < DateTime.UtcNow)
                {
                    Consensus = new Directory(Auth.Random().DownloadNodes());
                    Cache.Add("consensus", Tools.Serialize(Consensus), true);
                }
            }
            else
            {
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
