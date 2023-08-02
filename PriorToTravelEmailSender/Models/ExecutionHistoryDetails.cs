namespace PriorToTravelEmailSender.Models;

internal record ExecutionHistoryDetails(
    DateOnly LastExecutionDate,
    IReadOnlyCollection<long> ProcessedBookings
);
