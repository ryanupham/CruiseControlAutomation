using PaymentAutomation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentAutomation.JsonConverters;

internal class AdjustmentCollectionConverter : JsonConverter<IReadOnlyCollection<Adjustment>>
{
    public override IReadOnlyCollection<Adjustment>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!TryParseNextAdjustmentGroup(ref reader, options, out var adjustments)) throw new JsonException();
        return adjustments;
    }

    private static bool TryParseNextAdjustmentGroup(ref Utf8JsonReader reader, JsonSerializerOptions options, out IReadOnlyCollection<Adjustment> adjustments)
    {
        adjustments = new List<Adjustment>();

        if (
            reader.TokenType != JsonTokenType.StartObject ||
            !reader.Read()
        ) throw new JsonException();

        do
        {
            if (
                reader.TokenType != JsonTokenType.PropertyName ||
                !reader.Read() ||
                reader.TokenType != JsonTokenType.StartArray ||
                !reader.Read() ||
                reader.TokenType != JsonTokenType.StartObject
            ) return false;
            do
            {
                if (!TryParseNextAdjustment(ref reader, options, out var adjustment)) return false;

                (adjustments as List<Adjustment>)!.Add(adjustment);

                if (!reader.Read()) return false;
            } while (reader.TokenType == JsonTokenType.StartObject);

            if (
                reader.TokenType != JsonTokenType.EndArray ||
                !reader.Read()
            ) return false;
        } while (reader.TokenType == JsonTokenType.PropertyName);

        return true;
    }

    private static bool TryParseNextAdjustment(ref Utf8JsonReader reader, JsonSerializerOptions options, out Adjustment adjustment)
    {
        var readerCopy = reader;
        var agent = JsonSerializer.Deserialize<AgentBookingRecord>(ref readerCopy, options)!;
        adjustment = JsonSerializer.Deserialize<Adjustment>(ref reader, options)! with { Agent = agent };
        return true;
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<Adjustment> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
