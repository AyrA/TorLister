using System.Net;
using System.Text.Json;
using TorLister.Tools;
using TorLister.Tools.Tools;
using TorLister.Tor;
using Directory = TorLister.Tor.Directory;

namespace TorLister
{
    internal class Program
    {
        private static Authority[]? authorities = null;
        private static Directory? consensus = null;

        public async static Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = ["/?"];
            }
            var argLC = args.Select(m => m.ToLower()).ToArray();
            if (args.Length == 0 || args.Contains("/?"))
            {
                ShowHelp();
                return;
            }
            consensus = await GetConsensus();
            if (argLC[0] == "/flags")
            {
                Console.WriteLine(string.Join("\n", consensus.KnownFlags));
            }
            else if (argLC[0] == "/dump")
            {
                var grouped = new Dictionary<string, string[]>();
                foreach (var Flag in consensus.KnownFlags)
                {
                    grouped[Flag] = consensus.TorNodes
                        .Where(m => m.Services.Contains(Flag))
                        .OrderBy(m => m.IP, IpComparer.Instance)
                        .Select(m => m.IP?.ToString() ?? throw null!)
                        .ToArray();
                }
                Console.WriteLine(JsonSerializer.Serialize(grouped));
            }
            else if (argLC[0] == "/all")
            {
                var details = argLC.Contains("/details");
                WriteHeader();
                foreach (var Node in consensus.TorNodes.OrderBy(m => m.IP, IpComparer.Instance))
                {
                    WriteNode(Node, details);
                }
            }
            else if (argLC[0] == "/flag" && args.Length > 1)
            {
                var details = argLC.Contains("/details");
                var userFlags = args[1].Split(',').Select(m => m.Trim().ToLower()).ToArray();
                var systemFlags = consensus.KnownFlags.Select(m => m.ToLower()).ToArray();
                if (userFlags.All(m => systemFlags.Contains(m)))
                {
                    WriteHeader();
                    foreach (var Node in consensus.TorNodes.OrderBy(m => m.IP, IpComparer.Instance))
                    {
                        if (Node.Services.Any(m => userFlags.Contains(m.ToLower())))
                        {
                            WriteNode(Node, details);
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

        static async Task<Directory> GetConsensus()
        {
            var cacheEntry = Cache.Get("authorities", TimeSpan.FromDays(1));

            if (cacheEntry != null)
            {
                Console.Error.WriteLine("Taking auth from cache");
                authorities = JsonSerializer.Deserialize<Authority[]>(cacheEntry.Data, Utils.jsonOpt);
            }
            else
            {
                Console.Error.WriteLine("Taking auth live");
                while (authorities == null || authorities.Length == 0)
                {
                    try
                    {
                        authorities = await Authorities.GetAuthoritiesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Download of Authorities failed");
                        Console.Error.WriteLine("[{0}]: {1}", ex.GetType().Name, ex.Message);
                        Console.Error.WriteLine("Retry...");
                        Thread.Sleep(2000);
                    }
                }
                Cache.Add("authorities", Utils.Serialize(authorities), true);
            }
            if (authorities == null)
            {
                throw new InvalidOperationException("Authorities still null after trying to get them");
            }

            cacheEntry = Cache.Get("consensus");
            Directory? consensus;
            if (cacheEntry != null)
            {
                Console.Error.WriteLine("Taking consensus from cache");
                consensus = Utils.Deserialize<Directory>(cacheEntry.Data)
                    ?? throw new InvalidOperationException("Possible cache damage. Deserialization failed");
                if (consensus.ValidUntil < DateTime.UtcNow)
                {
                    consensus = null;
                    Console.Error.WriteLine("Consensus is outdated. Renewing from random Authority...");
                    while (consensus == null)
                    {
                        Authority Selected = Random.Shared.GetItems(authorities, 1)[0];
                        try
                        {
                            consensus = new Directory(await Selected.DownloadNodesAsync());

                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Download failed from authority: {0}", Selected.Name);
                            Console.Error.WriteLine("[{0}]: {1}", ex.GetType().Name, ex.Message);
                            Console.Error.WriteLine("Retry...");
                            Thread.Sleep(1000);
                        }
                    }
                    Cache.Add("consensus", Utils.Serialize(consensus), true);
                }
                else
                {
                    Console.Error.WriteLine("Consensus valid until {0} UTC", consensus.ValidUntil);
                }
            }
            else
            {
                consensus = null;
                Console.Error.WriteLine("Consensus not available. Renewing from random Authority...");
                while (consensus == null)
                {
                    Authority Selected = Random.Shared.GetItems(authorities, 1)[0];
                    try
                    {
                        consensus = new Directory(await Selected.DownloadNodesAsync());
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Download failed from authority: {0}", Selected.Name);
                        Console.Error.WriteLine("[{0}]: {1}", ex.GetType().Name, ex.Message);
                        Console.Error.WriteLine("Retry...");
                        Thread.Sleep(500);
                    }
                }
                Cache.Add("consensus", Utils.Serialize(consensus), true);
            }
            return consensus;
        }

        private static void WriteHeader()
        {
            Console.WriteLine(string.Join("\t", ["Name", "IP", "OrPort", "OnlineSince", "Flags"]));
        }

        static void WriteNode(TorNode Node, bool Details = false)
        {
            if (Details)
            {
                Console.WriteLine(string.Join("\t", [
                    Node.Name,
                    Node.IP?.ToString() ?? IPAddress.Any.ToString(),
                    Node.OrPort.ToString(),
                    Node.OnlineSince.ToString("s"),
                    string.Join(",",Node.Services) ])
                );
            }
            else
            {
                Console.WriteLine(Node.IP);
            }
        }
    }
}
