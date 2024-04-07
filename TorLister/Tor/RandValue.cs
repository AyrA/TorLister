using System;

namespace TorLister.Tor
{
    [Serializable]
    public struct RandValue
    {
        public int RandomValue;
        public byte[] RandomNonce;
    }
}
