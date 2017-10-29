using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace TorLister
{
    public class IpComparer : IComparer<IPAddress>
    {
        /// <summary>
        /// Compares two byte arrays as fast as possible
        /// </summary>
        /// <param name="b1">Byte Array 1</param>
        /// <param name="b2">Byte Array 2</param>
        /// <param name="count">Length of the shorter</param>
        /// <returns>Relative Sort Value</returns>
        /// <remarks>
        /// If this returns 0 and the arrays are of different lengths,
        /// return the first unused index from the longer array.
        /// Return the value negative,
        /// if <paramref name="b2"/> is bigger than <paramref name="b1"/>.
        /// In other words, operate as if the shorter array was padded with elements containing 0.
        /// </remarks>
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        /// <summary>
        /// Instance if this Class
        /// </summary>
        public static readonly IpComparer Instance = new IpComparer();

        /// <summary>
        /// Compares two IP Addresses
        /// </summary>
        /// <param name="FirstAddress">First Address</param>
        /// <param name="SecondAddress">Second Address</param>
        /// <returns>Relative Sort Value</returns>
        public int Compare(IPAddress FirstAddress, IPAddress SecondAddress)
        {
            //Map V4 to V6 if the address family differs
            if (FirstAddress.AddressFamily != SecondAddress.AddressFamily)
            {
                if (FirstAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    FirstAddress = FirstAddress.MapToIPv6();
                }
                if (SecondAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    SecondAddress = FirstAddress.MapToIPv6();
                }
            }
            var bx = FirstAddress.GetAddressBytes();
            var by = SecondAddress.GetAddressBytes();

            return memcmp(bx,by,bx.LongLength);

            /*
            for (var i = 0; i < bx.Length; i++)
            {
                if (bx[i] != by[i])
                {
                    return bx[i] - by[i];
                }
            }
            return 0;
            //*/
        }
    }
}
