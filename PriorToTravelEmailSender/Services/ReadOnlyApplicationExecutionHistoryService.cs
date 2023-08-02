namespace PriorToTravelEmailSender.Services;

internal class ReadOnlyApplicationExecutionHistoryService : IApplicationExecutionHistoryService
{
    private readonly IApplicationExecutionHistoryService delegateService;

    public ReadOnlyApplicationExecutionHistoryService(
        IApplicationExecutionHistoryService delegateService) =>
        this.delegateService = delegateService;

    public DateOnly GetLastExecutionDate() =>
        delegateService.GetLastExecutionDate();

    public IReadOnlyCollection<long> GetProcessedBookingIds() =>
        delegateService.GetProcessedBookingIds();

    public void AddProcessedBookingId(long bookingId) { }

    public void SetLastExecutionDate(DateOnly lastExecutionDate) { }
}
