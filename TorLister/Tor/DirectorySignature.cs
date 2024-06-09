namespace TorLister.Tor
{
    [Serializable]
    public class DirectorySignature
    {
        public string SignatureType { get; set; }
        public string Hash1 { get; set; }
        public string Hash2 { get; set; }
        public string? Signature { get; set; } = null;

        public DirectorySignature(string[] segments)
        {
            SignatureType = segments[0];
            Hash1 = segments[1];
            Hash2 = segments[2];
        }

        public DirectorySignature()
        {
            SignatureType = Hash1 = Hash2 = string.Empty;
        }
    }
}
