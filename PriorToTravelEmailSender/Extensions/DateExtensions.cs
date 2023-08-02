namespace PriorToTravelEmailSender.Extensions;
internal static class DateExtensions
{
    public static DateOnly ToDateOnly(this DateTime dateTime) =>
        DateOnly.FromDateTime(dateTime);

    public static bool IsBetweenInclusive(
            this DateOnly date,
            DateOnly startDate,
            DateOnly endDate
        ) => date >= startDate && date <= endDate;
}
