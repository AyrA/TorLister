namespace TorLister.Tor
{
    [Serializable]
    public struct DirectorySignature(string[] Segments)
    {
        public string SignatureType = Segments[0];
        public string Hash1 = Segments[1];
        public string Hash2 = Segments[2];
        public string? Signature = null;
    }
}
