using CruiseControl.DataAccess;
using PriorToTravelEmailSender.Extensions;
using PriorToTravelEmailSender.Models;

namespace PriorToTravelEmailSender.Services;

internal class ApplicationExecutionHistoryService : IApplicationExecutionHistoryService
{
    private readonly ISimpleDataStore<ExecutionHistoryDetails> dataStore;

    public ApplicationExecutionHistoryService(
        ISimpleDataStore<ExecutionHistoryDetails> dataStore) =>
            this.dataStore = dataStore;

    public DateOnly GetLastExecutionDate()
    {
        var lastExecutionDate = GetExecutionHistoryDetails().LastExecutionDate;
        return lastExecutionDate == DateOnly.MinValue
            ? DateTime.Now.ToDateOnly().AddDays(-1)
            : lastExecutionDate;
    }

    public void SetLastExecutionDate(DateOnly lastExecutionDate)
    {
        var executionDetails = GetExecutionHistoryDetails()
            with { LastExecutionDate = lastExecutionDate };
        dataStore.Write(executionDetails);
    }

    public IReadOnlyCollection<long> GetProcessedBookingIds() =>
        GetExecutionHistoryDetails().ProcessedBookings;

    public void AddProcessedBookingId(long bookingId)
    {
        var currentExecutionDetails = GetExecutionHistoryDetails();
        var processedBookings =
            currentExecutionDetails.ProcessedBookings.ToList();
        processedBookings.Add(bookingId);
        var newExecutionDetails = currentExecutionDetails
            with { ProcessedBookings = processedBookings.Distinct().ToList() };
        dataStore.Write(newExecutionDetails);
    }

    private ExecutionHistoryDetails GetExecutionHistoryDetails() =>
        dataStore.Read()
            ?? throw new InvalidOperationException(
                "Unable to deserialize execution history details");
}
