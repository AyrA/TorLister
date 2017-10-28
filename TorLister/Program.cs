using System;
using System.Linq;

namespace TorLister
{
    class Program
    {
        static void Main(string[] args)
        {
            var Auth = Authorities.GetAuthorities();
            
            var Consensus = new Directory(Auth.Random().DownloadNodes());

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
