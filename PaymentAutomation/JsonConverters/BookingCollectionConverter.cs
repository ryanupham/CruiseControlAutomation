using PaymentAutomation.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentAutomation.JsonConverters;

class BookingCollectionConverter : JsonConverter<IReadOnlyCollection<Booking>>
{
    public override IReadOnlyCollection<Booking>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bookings = new List<Booking>();

        if (
            reader.TokenType != JsonTokenType.StartObject ||
            !reader.Read()
        ) throw new JsonException();

        do
        {
            if (!TryParseNextBookingGroup(ref reader, options, out var newBookings)) throw new JsonException();
            bookings.AddRange(newBookings);
        } while (
            reader.TokenType == JsonTokenType.EndObject &&
            reader.Read() &&
            reader.TokenType == JsonTokenType.PropertyName
        );

        return bookings;
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<Booking> value, JsonSerializerOptions options) =>
        throw new NotImplementedException();

    private static bool TryParseNextBookingGroup(ref Utf8JsonReader reader, JsonSerializerOptions options, out IReadOnlyCollection<Booking> bookings)
    {
        bookings = new List<Booking>();

        if (
            reader.TokenType != JsonTokenType.PropertyName ||
            !reader.Read() ||
            reader.TokenType != JsonTokenType.StartObject ||
            !reader.Read()
        ) return false;

        do
        {
            if (
                reader.TokenType != JsonTokenType.PropertyName ||
                !reader.Read() ||
                reader.TokenType != JsonTokenType.StartObject ||
                !reader.Read()
            ) return false;

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
                    if (!TryParseNextBooking(ref reader, options, out var booking)) return false;

                    (bookings as List<Booking>)!.Add(booking);

                    if (!reader.Read()) return false;
                } while (reader.TokenType == JsonTokenType.StartObject);

                if (
                    reader.TokenType != JsonTokenType.EndArray ||
                    !reader.Read()
                ) return false;
            } while (reader.TokenType == JsonTokenType.PropertyName);

            if (
                reader.TokenType != JsonTokenType.EndObject ||
                !reader.Read()
            ) return false;
        } while (reader.TokenType != JsonTokenType.EndObject);

        return true;
    }

    private static bool TryParseNextBooking(ref Utf8JsonReader reader, JsonSerializerOptions options, out Booking booking)
    {
        var readerCopy = reader;
        var agent = JsonSerializer.Deserialize<AgentBookingRecord>(ref readerCopy, options)!;
        booking = JsonSerializer.Deserialize<Booking>(ref reader, options)! with { Agent = agent };
        return true;
    }
}
