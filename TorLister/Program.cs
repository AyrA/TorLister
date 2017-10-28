using System;
using System.Linq;

namespace TorLister
{
    class Program
    {
        static void Main(string[] args)
        {
            var Auth = Authorities.GetAuthorities();
            var Consensus = new Directory(Auth.First().DownloadNodes());

            Console.WriteLine("#END");
            Console.ReadKey(true);
        }
    }
}
