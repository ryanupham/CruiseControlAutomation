using System.Text.Json;

namespace PaymentAutomation.DataAccess;

internal class AgentRolloverRepository : IRepository<(DateOnly weekEndingDate, string agentId), decimal>
{
    private readonly string filename;
    private readonly Lazy<Dictionary<string, Dictionary<string, decimal>>> agentBalancesByWeek;

    public AgentRolloverRepository(string filename)
    {
        this.filename = filename;
        agentBalancesByWeek = new(() =>
        {
            var agentBalancesRaw = File.Exists(filename)
                ? File.ReadAllText(filename)
                : "{}";
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, decimal>>>(agentBalancesRaw)!;
        });
    }

    public bool TryGet((DateOnly weekEndingDate, string agentId) key, out decimal value)
    {
        value = agentBalancesByWeek!.Value.Keys
            .Select(d => DateOnly.Parse(d))
            .OrderByDescending(d => d)
            .SkipWhile(d => d >= key.weekEndingDate)
            .Select(d => agentBalancesByWeek.Value[d.ToString()])
            .FirstOrDefault(b => b.ContainsKey(key.agentId))
            ?.GetValueOrDefault(key.agentId) ?? 0;

        return true;
    }

    public decimal Get((DateOnly weekEndingDate, string agentId) key) =>
        TryGet(key, out var value) ? value : throw new KeyNotFoundException();

    public bool Add((DateOnly weekEndingDate, string agentId) key, decimal value)
    {
        agentBalancesByWeek.Value[key.weekEndingDate.ToString()] =
            agentBalancesByWeek.Value.GetValueOrDefault(key.weekEndingDate.ToString()) ?? new();

        agentBalancesByWeek.Value[key.weekEndingDate.ToString()][key.agentId] = value;

        return Save();
    }

    public bool Delete((DateOnly weekEndingDate, string agentId) key)
    {
        if (
            !agentBalancesByWeek.Value.ContainsKey(key.weekEndingDate.ToString()) ||
            !agentBalancesByWeek.Value[key.weekEndingDate.ToString()].ContainsKey(key.agentId)
        ) return true;

        agentBalancesByWeek.Value[key.weekEndingDate.ToString()].Remove(key.agentId);

        return Save();
    }

    protected bool Save()
    {
        var serializedAgentBalancesByWeek = JsonSerializer.Serialize(agentBalancesByWeek.Value);
        File.WriteAllText(filename, serializedAgentBalancesByWeek);
        return true;
    }
}
