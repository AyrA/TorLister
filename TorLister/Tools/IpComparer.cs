using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace TorLister.Tools.Tools
{
    /// <summary>
    /// Compares IP addresses
    /// </summary>
    /// <remarks>This has the ability to work with IP addresses that </remarks>
    public partial class IpComparer : IComparer<IPAddress?>
    {
        /// <summary>
        /// Compares two byte arrays as fast as possible
        /// </summary>
        /// <param name="b1">Byte array 1</param>
        /// <param name="b2">Byte array 2</param>
        /// <param name="count">Length of the shorter</param>
        /// <returns>Relative sort value</returns>
        /// <remarks>
        /// If this returns 0 and the arrays are of different lengths,
        /// return the first unused index from the longer array.
        /// Return the value negative,
        /// if <paramref name="b2"/> is bigger than <paramref name="b1"/>.
        /// In other words, operate as if the shorter array was padded with elements containing 0.
        /// </remarks>
#if OS_WINDOWS
        [LibraryImport("msvcrt.dll", EntryPoint = "memcmp")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        private static partial int CompareBytes([In] byte[] b1, [In] byte[] b2, long count);
#else
        private static int CompareBytes([In] byte[] b1, [In] byte[] b2, long count)
        {
            for (long i = 0; i < count; i++)
            {
                if (b1[i] != b2[i])
                {
                    return b1[i].CompareTo(b2[i]);
                }
            }
            return 0;
        }
#endif
        /// <summary>
        /// Instance of this Class
        /// </summary>
        public static readonly IpComparer Instance = new();

        /// <summary>
        /// Compares two IP addresses
        /// </summary>
        /// <param name="firstAddress">First address</param>
        /// <param name="secondAddress">Second address</param>
        /// <returns>Relative sort value</returns>
        public int Compare(IPAddress? firstAddress, IPAddress? secondAddress)
        {
            if (firstAddress == null && secondAddress == null)
            {
                return 0;
            }
            ArgumentNullException.ThrowIfNull(firstAddress);
            ArgumentNullException.ThrowIfNull(secondAddress);

            //Map V4 to V6 if the address family differs
            if (firstAddress.AddressFamily != secondAddress.AddressFamily)
            {
                if (firstAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    firstAddress = firstAddress.MapToIPv6();
                }
                if (secondAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    secondAddress = secondAddress.MapToIPv6();
                }
            }
            var bx = firstAddress.GetAddressBytes();
            var by = secondAddress.GetAddressBytes();

            return CompareBytes(bx, by, Math.Min(bx.LongLength, by.LongLength));
        }
    }
}
