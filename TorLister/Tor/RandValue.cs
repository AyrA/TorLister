namespace TorLister.Tor
{
    [Serializable]
    public class RandValue
    {
        public int RandomValue { get; set; }
        public byte[]? RandomNonce { get; set; }
    }
}
