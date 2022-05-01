using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentAutomation.JsonConverters;

internal class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (
            reader.TokenType != JsonTokenType.Number ||
            !reader.TryGetInt64(out var numericDate)
        ) throw new JsonException();

        var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(numericDate);
        return DateOnly.FromDateTime(dateTimeOffset.Date);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
