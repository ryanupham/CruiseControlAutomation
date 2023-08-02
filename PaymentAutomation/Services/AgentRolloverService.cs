using CruiseControl.DataAccess;
using PaymentAutomation.Models;

namespace PaymentAutomation.Services;

internal interface IAgentRolloverService
{
    void Add(DateOnly weekEndingDate, string agentId, decimal amount);
    decimal Get(DateOnly weekEndingDate, string agentId);
}

internal class AgentRolloverService : IAgentRolloverService
{
    private readonly ISimpleDataStore<AgentRolloverCollection> dataStore;

    public AgentRolloverService(
        ISimpleDataStore<AgentRolloverCollection> dataStore) =>
            this.dataStore = dataStore;

    public decimal Get(DateOnly weekEndingDate, string agentId)
    {
        var rolloverCollection = GetRolloverCollection();

        return rolloverCollection.Keys
            .OrderByDescending(d => d)
            .SkipWhile(d => d >= weekEndingDate)
            .Select(d => rolloverCollection[d])
            .FirstOrDefault(b => b.ContainsKey(agentId))
            ?.GetValueOrDefault(agentId) ?? 0;
    }

    public void Add(DateOnly weekEndingDate, string agentId, decimal amount)
    {
        var rolloverCollection = GetRolloverCollection();

        rolloverCollection[weekEndingDate] =
            rolloverCollection.GetValueOrDefault(weekEndingDate) ?? new();

        rolloverCollection[weekEndingDate][agentId] = amount;
    }

    private AgentRolloverCollection GetRolloverCollection() =>
        dataStore.Read() ?? new AgentRolloverCollection();
}
