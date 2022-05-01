using PaymentAutomation.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentAutomation.JsonConverters;

internal class AdjustmentTypeConverter : JsonConverter<AdjustmentType>
{
    public override AdjustmentType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var description = reader.GetString() ?? throw new JsonException();
        return AdjustmentType.FromValue(description);
    }

    public override void Write(Utf8JsonWriter writer, AdjustmentType value, JsonSerializerOptions options) =>
        throw new NotImplementedException();
}
