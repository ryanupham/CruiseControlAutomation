using System.Text.Json;
using System.Text.Json.Serialization;

namespace PriorToTravelEmailSender.JsonConverters;
internal class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        var stringDate = reader.GetString() ?? throw new JsonException();
        return stringDate == string.Empty
            ? DateOnly.MinValue
            : GetDateOnlyFromString(stringDate);
    }

    private static DateOnly GetDateOnlyFromString(string dateString) =>
        DateTime.TryParse(dateString, out DateTime dateTime)
            ? DateOnly.FromDateTime(dateTime)
            : throw new JsonException();

    public override void Write(
        Utf8JsonWriter writer,
        DateOnly value,
        JsonSerializerOptions options) =>
            throw new NotImplementedException();
}
