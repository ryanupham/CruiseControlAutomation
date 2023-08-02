using PaymentAutomation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentAutomation.JsonConverters;

internal class BookingConverter : JsonConverter<Booking>
{
    public override Booking Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var nonBookingConverters = options.Converters
            .Where(c => !c.CanConvert(typeToConvert));

        var bookingOptions = new JsonSerializerOptions();
        foreach (var converter in nonBookingConverters)
        {
            bookingOptions.Converters.Add(converter);
        }

        var tmpReader = reader;
        var agent = JsonSerializer.Deserialize<AgentBookingRecord>(
            ref tmpReader, options)
                ?? throw new JsonException();
        var booking = JsonSerializer.Deserialize<Booking>(
            ref reader, bookingOptions)
                ?? throw new JsonException();

        return booking with { Agent = agent };
    }

    public override void Write(
        Utf8JsonWriter writer,
        Booking value,
        JsonSerializerOptions options) => throw new NotImplementedException();
}
