using PaymentAutomation.Models;

namespace PaymentAutomation.Services;

public interface IAgentSettingsProvider
{
    bool TryGet(string agentId, out AgentSettings settings);
    AgentSettings GetOrDefault(string agentId);
    IReadOnlyCollection<AgentSettings> GetAll();
}

internal class AgentSettingsProvider : IAgentSettingsProvider
{
    private readonly IReadOnlyDictionary<string, AgentSettings> agentSettings;

    public AgentSettingsProvider(IReadOnlyDictionary<string, AgentSettings> agentSettings) =>
        this.agentSettings = agentSettings;

    public bool TryGet(string agentId, out AgentSettings settings) =>
        agentSettings.TryGetValue(agentId, out settings!);

    public AgentSettings GetOrDefault(string agentId) =>
        agentSettings.TryGetValue(agentId, out var settings)
            ? settings
            : new AgentSettings();

    public IReadOnlyCollection<AgentSettings> GetAll() =>
        agentSettings.Values.ToList();
}
