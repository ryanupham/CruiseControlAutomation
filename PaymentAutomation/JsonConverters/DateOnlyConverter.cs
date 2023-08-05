using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentAutomation.JsonConverters;
internal class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (!reader.TryGetInt64(out var numericDate))
            {
                throw new JsonException();
            }

            return GetDateOnlyFromNumber(numericDate);
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            var stringDate = reader.GetString()
                ?? throw new JsonException();
            return GetDateOnlyFromString(stringDate);
        }

        throw new JsonException();
    }

    private static DateOnly GetDateOnlyFromNumber(long date)
    {
        var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(date);
        return DateOnly.FromDateTime(dateTimeOffset.Date);
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
