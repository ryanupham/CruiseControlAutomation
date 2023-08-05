using CruiseControl.Models;
using CruiseControl.Services;
using Microsoft.Extensions.Logging;

namespace PriorToTravelEmailSender.Services;
internal class NullBookingNoteService : IBookingNoteService
{
    private readonly ILogger<NullBookingNoteService> logger;

    public NullBookingNoteService(ILogger<NullBookingNoteService> logger) =>
        this.logger = logger;

    public Task<long> AddNote(long bookingId, BookingNoteCreate note)
    {
        logger.LogInformation(
            "Would have added note to booking {BookingId}: {Note}",
            bookingId,
            note);

        return Task.FromResult(-1L);
    }

    public Task<BookingNote> GetNote(long bookingId, long noteId) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<BookingNote>> GetNotes(long bookingId) =>
        throw new NotImplementedException();
}
