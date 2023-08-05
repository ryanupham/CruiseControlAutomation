using CruiseControl.JsonConverters;
using CruiseControl.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace CruiseControl.Services;
public interface IBookingNoteService
{
    Task<long> AddNote(long bookingId, BookingNoteCreate note);
    Task<BookingNote> GetNote(long bookingId, long noteId);
    Task<IReadOnlyList<BookingNote>> GetNotes(long bookingId);
}

public class BookingNoteService : IBookingNoteService
{
    private readonly ILogger<BookingNoteService> logger;
    private readonly CruiseControlClient client;

    private readonly JsonSerializerOptions serializerOptions;

    public BookingNoteService(
        ILogger<BookingNoteService> logger,
        CruiseControlClient client)
    {
        this.logger = logger;
        this.client = client;

        serializerOptions = new JsonSerializerOptions
        {
            Converters = { new ColonlessOffsetToDateTimeConverter() },
            PropertyNameCaseInsensitive = true,
        };
    }

    public async Task<long> AddNote(long bookingId, BookingNoteCreate note)
    {
        logger.LogInformation("Adding note to booking {bookingId}", bookingId);

        var noteJson = JsonSerializer.Serialize(note, serializerOptions);
        var payload = new StringContent(
            noteJson, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(
            $"booking-api/booking/{bookingId}/notes/", payload);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Error adding note: {Error}", error);
            throw new Exception(error);
        }

        var rawResponse = await response.Content.ReadAsStringAsync();
        return long.Parse(rawResponse);
    }

    public async Task<IReadOnlyList<BookingNote>> GetNotes(long bookingId)
    {
        logger.LogInformation("Getting notes for booking {BookingId}", bookingId);

        var response = await client.GetAsync(
            $"booking-api/booking/{bookingId}/notes/");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Error getting notes: {Error}", error);
            throw new Exception(error);
        }

        var rawContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer
            .Deserialize<BookingNotesResponse>(rawContent, serializerOptions)
            ?.Notes ?? throw new Exception("Error deserializing notes");
    }

    public async Task<BookingNote> GetNote(long bookingId, long noteId)
    {
        logger.LogInformation(
            "Getting note {NoteId} for booking {BookingId}", noteId, bookingId);

        var response = await client.GetAsync(
            $"booking-api/booking/{bookingId}/notes/{noteId}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Error getting note: {Error}", error);
            throw new Exception(error);
        }

        var rawContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer
            .Deserialize<BookingNote>(rawContent, serializerOptions)
                ?? throw new Exception("Error deserializing note");
    }
}
