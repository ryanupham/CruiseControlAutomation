namespace PriorToTravelEmailSender.Models;

internal record EmailOptions
{
    public List<string> Cc { get; init; } = new List<string>();
    public List<string> Bcc { get; init; } = new(new List<string>());
    public string Template { get; init; } = string.Empty;
}
