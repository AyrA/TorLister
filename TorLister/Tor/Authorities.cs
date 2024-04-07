namespace TorLister.Tor
{
    public static class Authorities
    {
        public const string TOR_SOURCE = "https://gitweb.torproject.org/tor.git/plain/src/app/config/auth_dirs.inc";

        public static async Task<Authority[]?> GetAuthoritiesAsync()
        {
            var ret = new List<Authority>();

            using var client = new HttpClient();
            string lines;

            try
            {
                lines = await client.GetStringAsync(new Uri(TOR_SOURCE));
            }
            catch
            {
                return null;
            }

            //remove all inline comments from the source
            while (lines.Contains("/*"))
            {
                lines = lines[..lines.IndexOf("/*")] +
                    lines[(lines.IndexOf("*/", lines.IndexOf("/*")) + 2)..];
            }
            //create parts
            string[] parts = lines.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                //remove unneeded chars
                foreach (string s in new string[] { "\"", "\r", "\n" })
                {
                    parts[i] = parts[i].Replace(s, "");
                }
                //remove unneeded whitespace
                while (parts[i].Contains("  "))
                {
                    parts[i] = parts[i].Replace("  ", " ");
                }
                //remove more whitespace
                parts[i] = parts[i].Trim();
            }
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                try
                {
                    ret.Add(new Authority(part));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Failed to add authority. [{0}]: {1}", ex.GetType().Name, ex.Message);
                }
            }
            if (ret.Count == 0)
            {
                throw new Exception("Failed to obtain any authorities");
            }
            return [.. ret];
        }
    }
}
