using System;

namespace TorLister
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var Auth in Authorities.GetAuthorities())
            {
                Console.WriteLine(Auth);
            }
            Console.WriteLine("#END");
            Console.ReadKey(true);
        }
    }
}
