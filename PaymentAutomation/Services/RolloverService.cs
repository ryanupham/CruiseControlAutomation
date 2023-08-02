using PaymentAutomation.Enums;
using PaymentAutomation.Models;

namespace PaymentAutomation.Services;

internal interface IRolloverService
{
    Task<IReadOnlyCollection<Rollover>> ProcessRollovers(
        DateOnly weekEndingDate,
        IReadOnlyCollection<Booking> bookings,
        IReadOnlyCollection<Adjustment> adjustments);
}

internal class RolloverService : IRolloverService
{
    private readonly IAgentRolloverService agentRolloverService;
    private readonly IReportingApiClient reportingApiClient;

    public RolloverService(
        IAgentRolloverService agentRolloverService,
        IReportingApiClient reportingApiClient)
    {
        this.agentRolloverService = agentRolloverService;
        this.reportingApiClient = reportingApiClient;
    }

    public async Task<IReadOnlyCollection<Rollover>> ProcessRollovers(
        DateOnly weekEndingDate,
        IReadOnlyCollection<Booking> bookings,
        IReadOnlyCollection<Adjustment> adjustments)
    {
        var agents = (await reportingApiClient.GetAgents()!)
            .Where(a => !a.Settings.IsManager);

        var rollovers = agents.Select(
            agent => GetRolloverForAgent(
                weekEndingDate,
                agent,
                bookings.Where(b => b.Agent.Id == agent.Id).ToList(),
                adjustments.Where(a => a.Agent.Id == agent.Id).ToList()))
            .ToList();

        foreach (var rollover in rollovers)
        {
            agentRolloverService.Add(
                weekEndingDate,
                rollover.Agent.Id,
                Math.Min(rollover.CurrentBalance, 0));
        }

        return rollovers;
    }

    private Rollover GetRolloverForAgent(
        DateOnly weekEndingDate,
        Agent agent,
        IReadOnlyCollection<Booking> bookings,
        IReadOnlyCollection<Adjustment> adjustments)
    {
        var priorBalance = agentRolloverService.Get(weekEndingDate, agent.Id);
        var bookingBalance = bookings.Sum(b => b.FranchisePayable);
        var adjustmenetBalance = adjustments
            .Where(a => a.Type != AdjustmentType.Unknown)
            .Sum(a => a.FranchisePayable);
        var currentBalance = priorBalance + bookingBalance + adjustmenetBalance;
        return new(agent, priorBalance, currentBalance);
    }
}
