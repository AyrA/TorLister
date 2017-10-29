using System;

namespace TorLister
{
    [Serializable]
    public struct RandValue
    {
        public int RandomValue;
        public byte[] RandomNonce;
    }
}
