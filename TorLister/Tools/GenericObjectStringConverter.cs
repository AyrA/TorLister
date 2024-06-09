using System.Text.Json;
using System.Text.Json.Serialization;

namespace TorLister.Tools
{
    public abstract class GenericObjectStringConverter<T> : JsonConverter<T>
    {
        /// <summary>
        /// Convert a JSON string back into an instance of the given type
        /// </summary>
        /// <param name="s">JSON string</param>
        /// <returns>deserialized instance. Null if <paramref name="s"/> is null</returns>
        /// <remarks>This should return null if <paramref name="s"/> is null</remarks>
        protected abstract T? FromString(string? s);
        /// <summary>
        /// Serializes the given instance into a JSON string.
        /// </summary>
        /// <param name="value">Instance</param>
        /// <returns>JSON string. Null if <paramref name="value"/> is null</returns>
        /// <remarks>This should return null if <paramref name="value"/> is null</remarks>
        protected abstract string? ToString(T? value);

        /// <inheritdoc />
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return FromString(reader.GetString());
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var serialized = ToString(value);
            if (serialized == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(serialized);
            }
        }
    }
}
