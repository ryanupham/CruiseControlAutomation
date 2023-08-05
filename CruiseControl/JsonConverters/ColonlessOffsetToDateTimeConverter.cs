using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CruiseControl.JsonConverters;
internal class ColonlessOffsetToDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var dateTimeWithoutOffset = reader.GetString()!.Split('+')[0];
        return DateTime.ParseExact(
            dateTimeWithoutOffset,
            "yyyy-MM-ddTHH:mm:ss.fff",
            CultureInfo.InvariantCulture);
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        var dateTimeText = value.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "+0000";
        writer.WriteStringValue(dateTimeText);
    }
}
