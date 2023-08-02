namespace PriorToTravelEmailSender.Services;

internal interface IApplicationExecutionHistoryService
{
    void AddProcessedBookingId(long bookingId);
    DateOnly GetLastExecutionDate();
    IReadOnlyCollection<long> GetProcessedBookingIds();
    void SetLastExecutionDate(DateOnly lastExecutionDate);
}
