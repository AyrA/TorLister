namespace TorLister
{
    public struct DirectorySignature
    {
        public string SignatureType;
        public string Hash1;
        public string Hash2;
        public string Certificate;

        public DirectorySignature(string[] Segments)
        {
            SignatureType = Segments[0];
            Hash1 = Segments[1];
            Hash2 = Segments[2];
            Certificate = null;
        }
    }
}
