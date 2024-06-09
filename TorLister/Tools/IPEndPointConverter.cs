using System.Net;

namespace TorLister.Tools
{
    public class IPEndPointConverter : GenericObjectStringConverter<IPEndPoint>
    {
        /// <inheritdoc />
        protected override IPEndPoint? FromString(string? s)
        {
            return string.IsNullOrEmpty(s) ? null : IPEndPoint.Parse(s);
        }

        /// <inheritdoc />
        protected override string? ToString(IPEndPoint? value)
        {
            return value?.ToString();
        }
    }
}
