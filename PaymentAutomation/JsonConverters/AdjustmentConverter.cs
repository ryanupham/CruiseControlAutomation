using PaymentAutomation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentAutomation.JsonConverters;
internal class AdjustmentConverter : JsonConverter<Adjustment>
{
    public override Adjustment Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var nonAdjustmentConverters = options.Converters
            .Where(c => !c.CanConvert(typeToConvert));

        var adjustmentOptions = new JsonSerializerOptions();
        foreach (var converter in nonAdjustmentConverters)
        {
            adjustmentOptions.Converters.Add(converter);
        }

        var tmpReader = reader;
        var agent = JsonSerializer.Deserialize<AgentBookingRecord>(
            ref tmpReader, options)
                ?? throw new JsonException();
        var adjustment = JsonSerializer.Deserialize<Adjustment>(
            ref reader, adjustmentOptions)
                ?? throw new JsonException();

        return adjustment with { Agent = agent };
    }

    public override void Write(
        Utf8JsonWriter writer,
        Adjustment value,
        JsonSerializerOptions options) => throw new NotImplementedException();
}
