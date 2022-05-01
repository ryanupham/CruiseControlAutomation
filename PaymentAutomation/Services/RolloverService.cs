using PaymentAutomation.DataAccess;
using PaymentAutomation.Enums;
using PaymentAutomation.Models;

namespace PaymentAutomation.Services;

internal interface IRolloverService
{
    Task<IReadOnlyCollection<Rollover>> ProcessRollovers(DateOnly weekEndingDate, IReadOnlyCollection<Booking> bookings, IReadOnlyCollection<Adjustment> adjustments);
}

internal class RolloverService : IRolloverService
{
    private readonly IRepository<(DateOnly weekEndingDate, string agentId), decimal> agentBalanceRepository;
    private readonly IReportingApiClient reportingApiClient;

    public RolloverService(
        IRepository<(DateOnly weekEndingDate, string agentId), decimal> agentBalanceRepository,
        IReportingApiClient reportingApiClient
    )
    {
        this.agentBalanceRepository = agentBalanceRepository;
        this.reportingApiClient = reportingApiClient;
    }

    public async Task<IReadOnlyCollection<Rollover>> ProcessRollovers(
        DateOnly weekEndingDate,
        IReadOnlyCollection<Booking> bookings,
        IReadOnlyCollection<Adjustment> adjustments
    )
    {
        var agents = (await reportingApiClient.GetAgents()!)
            .Where(a => !a.Settings.IsManager);

        var rollovers = agents.Select(
            agent => GetRolloverForAgent(
                weekEndingDate,
                agent,
                bookings.Where(b => b.Agent.Id == agent.Id).ToList(),
                adjustments.Where(a => a.Agent.Id == agent.Id).ToList()
            )
        ).ToList();

        foreach (var rollover in rollovers)
        {
            agentBalanceRepository.Add(
                (weekEndingDate, rollover.Agent.Id),
                Math.Min(rollover.CurrentBalance, 0)
            );
        }

        return rollovers;
    }

    private Rollover GetRolloverForAgent(
        DateOnly weekEndingDate,
        Agent agent,
        IReadOnlyCollection<Booking> bookings,
        IReadOnlyCollection<Adjustment> adjustments
    )
    {
        var priorBalance = GetRolloverAmountForAgent(weekEndingDate, agent);
        var bookingBalance = bookings.Sum(b => b.FranchisePayable);
        var adjustmenetBalance = adjustments
            .Where(a => a.Type != AdjustmentType.Unknown)
            .Sum(a => a.FranchisePayable);
        var currentBalance = priorBalance + bookingBalance + adjustmenetBalance;
        return new Rollover(agent, priorBalance, currentBalance);
    }

    private decimal GetRolloverAmountForAgent(DateOnly weekEndingDate, Agent agent) =>
        agentBalanceRepository.TryGet((weekEndingDate, agent.Id), out var balance)
            ? balance
            : 0;
}
