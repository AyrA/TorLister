using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TorLister
{
    public class Program
    {
        private static Authority[] Auth = null;
        private static Directory Consensus = null;

        public async static Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] { "/?" };
            }
            var argLC = args.Select(m => m.ToLower()).ToArray();
            if (args.Length == 0 || args.Contains("/?"))
            {
                ShowHelp();
                return;
            }
            await GetConsensus();
            if (argLC[0] == "/flags")
            {
                Console.WriteLine(string.Join("\n", Consensus.KnownFlags));
            }
            else if (argLC[0] == "/dump")
            {
                var Ret = new Dictionary<string, string[]>();
                foreach (var Flag in Consensus.KnownFlags)
                {
                    Ret[Flag] = Consensus.TorNodes
                        .Where(m => m.Services.Contains(Flag))
                        .OrderBy(m => m.IP, IpComparer.Instance)
                        .Select(m => m.IP.ToString())
                        .ToArray();
                }
                Console.WriteLine(JsonConvert.SerializeObject(Ret));
            }
            else if (argLC[0] == "/all")
            {
                var Details = argLC.Contains("/details");
                WriteHeader();
                foreach (var Node in Consensus.TorNodes.OrderBy(m => m.IP, IpComparer.Instance))
                {
                    WriteNode(Node, Details);
                }
            }
            else if (argLC[0] == "/flag" && args.Length > 1)
            {
                var Details = argLC.Contains("/details");
                var UserFlags = args[1].Split(',').Select(m => m.Trim().ToLower()).ToArray();
                var SystemFlags = Consensus.KnownFlags.Select(m => m.ToLower()).ToArray();
                if (UserFlags.All(m => SystemFlags.Contains(m)))
                {
                    WriteHeader();
                    foreach (var Node in Consensus.TorNodes.OrderBy(m => m.IP, IpComparer.Instance))
                    {
                        if (Node.Services.Any(m => UserFlags.Contains(m.ToLower())))
                        {
                            WriteNode(Node, Details);
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
                Console.Error.WriteLine("Invalid arguments");
            }
#if DEBUG
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#endif
        }

        private static void ShowHelp()
        {
            Console.Error.WriteLine(@"TorLister {/dump | /all | /flags | /flag flag[,...]} [/details]
/dump     - Dump all entries as JSON object
/all      - All entries (equal to specifying all flags)
/flags    - List available flags
/flag     - List nodes with any of the given flags.
            Don't add spaces between the entries
/details  - Shows details for /flag or /all (tab separated list)
            Must be the last argument if specified.
            Has no effect on /dump and /flags");
        }

        static async Task GetConsensus()
        {
            var CacheEntry = Cache.Get("authorities", TimeSpan.FromDays(1));

            if (CacheEntry != null)
            {
                Console.Error.WriteLine("Taking auth from cache");
                Auth = Tools.Deserialize<Authority[]>(CacheEntry.Data);
            }
            else
            {
                Console.Error.WriteLine("Taking auth live");
                while (Auth == null || Auth.Length == 0)
                {
                    try
                    {
                        Auth = await Authorities.GetAuthoritiesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Download of Authorities failed");
                        Console.Error.WriteLine("[{0}]: {1}", ex.GetType().Name, ex.Message);
                        Console.Error.WriteLine("Retry...");
                        Thread.Sleep(2000);
                    }
                }
                Cache.Add("authorities", Tools.Serialize(Auth), true);
            }

            CacheEntry = Cache.Get("consensus");
            if (CacheEntry != null)
            {
                Console.Error.WriteLine("Taking consensus from cache");
                Consensus = Tools.Deserialize<Directory>(CacheEntry.Data);
                if (Consensus.ValidUntil < DateTime.UtcNow)
                {
                    Consensus = null;
                    Console.Error.WriteLine("Consensus is outdated. Renewing from random Authority...");
                    while (Consensus == null)
                    {
                        Authority Selected = Auth.Random();
                        try
                        {
                            Consensus = new Directory(await Selected.DownloadNodesAsync());

                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Download failed from authority: {0}", Selected.Name);
                            Console.Error.WriteLine("[{0}]: {1}", ex.GetType().Name, ex.Message);
                            Console.Error.WriteLine("Retry...");
                            Thread.Sleep(1000);
                        }
                    }
                    Cache.Add("consensus", Tools.Serialize(Consensus), true);
                }
                else
                {
                    Console.Error.WriteLine("Consensus valid until {0} UTC", Consensus.ValidUntil);
                }
            }
            else
            {
                Consensus = null;
                Console.Error.WriteLine("Consensus not available. Renewing from random Authority...");
                while (Consensus == null)
                {
                    Authority Selected = Auth.Random();
                    try
                    {
                        Consensus = new Directory(await Selected.DownloadNodesAsync());

                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Download failed from authority: {0}", Selected.Name);
                        Console.Error.WriteLine("[{0}]: {1}", ex.GetType().Name, ex.Message);
                        Console.Error.WriteLine("Retry...");
                        Thread.Sleep(500);
                    }
                }
                Cache.Add("consensus", Tools.Serialize(Consensus), true);
            }
        }

        private static void WriteHeader()
        {
            Console.WriteLine(string.Join("\t", new string[] { "Name", "IP", "OrPort", "OnlineSince", "Flags" }));
        }

        static void WriteNode(TorNode Node, bool Details = false)
        {
            if (Details)
            {
                Console.WriteLine(string.Join("\t", new string[] {
                    Node.Name,
                    Node.IP.ToString(),
                    Node.OrPort.ToString(),
                    Node.OnlineSince.ToString("s"),
                    string.Join(",",Node.Services) })
                );
            }
            else
            {
                Console.WriteLine(Node.IP);
            }
        }
    }
}
