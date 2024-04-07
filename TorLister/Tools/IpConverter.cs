using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TorLister.Tools
{
    public class IpConverter : JsonConverter<IPEndPoint>
    {
        public override IPEndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (str == null)
            {
                return null;
            }
            return IPEndPoint.Parse(str);
        }

        public override void Write(Utf8JsonWriter writer, IPEndPoint value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
