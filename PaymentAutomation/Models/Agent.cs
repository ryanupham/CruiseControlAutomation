namespace PaymentAutomation.Models
{
    public abstract record Agent : IEquatable<Agent>
    {
        public abstract string Id { get; init; }
        public abstract string FirstName { get; init; }
        public abstract string LastName { get; init; }
        public abstract string FullName { get; }
        public abstract AgentSettings Settings { get; init; }
        public virtual bool Equals(Agent? other) => other?.Id == Id;
        public override int GetHashCode() => Id?.GetHashCode() ?? 0;
    }
}
