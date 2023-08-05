using PaymentAutomation.Models;
using System.Diagnostics.CodeAnalysis;

namespace PaymentAutomation.Utilities;
public class AgentEqualityComparer : IEqualityComparer<Agent>
{
    public bool Equals(Agent? x, Agent? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] Agent obj) => obj.Id.GetHashCode();
}
