using CruiseControl.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CruiseControl.Services;
public interface IUserService
{
    Task<UserProfile> GetUserProfile();
}

public class UserService : IUserService
{
    private readonly ILogger<UserService> logger;
    private readonly CruiseControlClient client;

    private readonly JsonSerializerOptions serializerOptions;

    public UserService(
        ILogger<UserService> logger,
        CruiseControlClient client)
    {
        this.logger = logger;
        this.client = client;

        serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
    }

    public async Task<UserProfile> GetUserProfile()
    {
        logger.LogInformation("Getting user profile");

        var response = await client.GetAsync(
            $"commons-api/common/user/profile");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Error getting user profile: {Error}", error);
            throw new Exception(error);
        }

        var rawContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer
            .Deserialize<UserProfile>(rawContent, serializerOptions)
                ?? throw new Exception("Could not deserialize user profile");
    }
}
