using CruiseControl.Models;

namespace PriorToTravelEmailSender.Models;

internal record AppSettings
{
    public EmailConfiguration EmailConfiguration { get; init; } = new();
    public EmailOptions EmailOptions { get; init; } = new();
    public int DaysUntilTravel { get; init; }
    public string Username { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
}
