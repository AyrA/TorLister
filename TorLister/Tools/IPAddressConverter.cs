using System.Net;

namespace TorLister.Tools
{
    public class IPAddressConverter : GenericObjectStringConverter<IPAddress>
    {
        /// <inheritdoc />
        protected override IPAddress? FromString(string? s)
        {
            return s == null ? null : IPAddress.Parse(s);
        }

        /// <inheritdoc />
        protected override string? ToString(IPAddress? value)
        {
            return value?.ToString();
        }
    }
}
