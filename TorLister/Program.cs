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

            Console.WriteLine(string.Join("\r\n",Consensus.TorNodes.Select(m => m.IP.ToString())));

            Console.WriteLine("#END");
            Console.ReadKey(true);
        }
    }
}
