using System;
using System.Linq;

namespace TorLister
{
    class Program
    {
        static void Main(string[] args)
        {
            Version V = new Version("1-8,20,30,31,32,33,35,37,40-50");

            Console.WriteLine(string.Join(",", V.Versions.Select(m => m.ToString())));
            Console.WriteLine(V);

            /*
            foreach (var Auth in Authorities.GetAuthorities())
            {
                Console.WriteLine(Auth);
            }
            //*/
            Console.WriteLine("#END");
            Console.ReadKey(true);
        }
    }
}
