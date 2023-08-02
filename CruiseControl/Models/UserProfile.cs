namespace CruiseControl.Models;

public record UserProfile(
    string FirstName,
    string LastName,
    string FullName,
    string Username,
    string EmailAddress
);
