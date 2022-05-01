using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentAutomation.JsonConverters;

internal class WeekEndingDatesConverter : JsonConverter<IReadOnlyCollection<DateOnly>>
{
    public override IReadOnlyCollection<DateOnly>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (
            reader.TokenType != JsonTokenType.StartArray ||
            !reader.Read() ||
            reader.TokenType != JsonTokenType.StartObject
        ) throw new JsonException();

        var dates = new List<DateOnly>();

        do
        {
            var date = GetNextDate(ref reader);
            dates.Add(date);
        } while (reader.Read() && reader.TokenType == JsonTokenType.StartObject);

        if (reader.TokenType != JsonTokenType.EndArray) throw new JsonException();

        return dates.AsReadOnly();
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<DateOnly> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    private static DateOnly GetNextDate(ref Utf8JsonReader reader)
    {
        if (
            reader.TokenType != JsonTokenType.StartObject ||
            !reader.Read() ||
            reader.TokenType != JsonTokenType.PropertyName
        ) throw new JsonException();

        var result = DateOnly.MinValue;

        foreach (var _ in Enumerable.Range(0, 2))
        {
            var isValid = reader.GetString() switch
            {
                "weekEndingDate" => TryParseDate(ref reader, out result),
                "valid" => TryParseValid(ref reader),
                _ => throw new JsonException()
            };

            if (!isValid || !reader.Read()) throw new JsonException();
        }

        return result;
    }

    private static bool TryParseValid(ref Utf8JsonReader reader) =>
        reader.TokenType == JsonTokenType.PropertyName &&
            reader.Read() &&
            reader.GetBoolean();

    private static bool TryParseDate(ref Utf8JsonReader reader, out DateOnly result)
    {
        result = DateOnly.MinValue;
        if (!reader.Read() || reader.TokenType != JsonTokenType.String) return false;

        var dateString = reader.GetString();
        return DateOnly.TryParse(dateString, out result);
    }
}
