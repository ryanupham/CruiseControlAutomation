namespace PriorToTravelEmailSender.Models;
internal record Booking(
    long Id,
    DateOnly DepartureDate,
    string FullName,
    string ContactEmail
);
