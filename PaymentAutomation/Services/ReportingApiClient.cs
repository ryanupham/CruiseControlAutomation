using PaymentAutomation.JsonConverters;
using PaymentAutomation.Models;
using PaymentAutomation.Models.ApiResponses;
using System.Net.Http.Json;
using System.Text.Json;

namespace PaymentAutomation.Services;

public interface IReportingApiClient
{
    Task<(IReadOnlyCollection<Booking> bookings, IReadOnlyCollection<Adjustment> adjustments)> GetBookingsAndAdjustmentsForWeekEnding(DateOnly weekEndingDate);
    Task<IReadOnlyCollection<DateOnly>> GetWeekEndingDates();
    Task<IReadOnlyCollection<Agent>> GetAgents();
}

internal class ReportingApiClient : IReportingApiClient
{
    private readonly long agencyId;
    private readonly HttpClient httpClient;
    private readonly IAgentSettingsProvider agentSettingsProvider;

    public ReportingApiClient(
        long agencyId,
        HttpClient httpClient,
        IAgentSettingsProvider agentSettingsProvider
    )
    {
        this.agencyId = agencyId;
        this.httpClient = httpClient;
        this.agentSettingsProvider = agentSettingsProvider;
    }

    private readonly Dictionary<DateOnly, Task<(IReadOnlyCollection<Booking> bookings, IReadOnlyCollection<Adjustment> adjustments)>> bookingsAndAdjustmentsForWeekEnding = new();
    public async Task<(IReadOnlyCollection<Booking> bookings, IReadOnlyCollection<Adjustment> adjustments)> GetBookingsAndAdjustmentsForWeekEnding(DateOnly weekEndingDate)
    {
        if (bookingsAndAdjustmentsForWeekEnding.TryGetValue(weekEndingDate, out var booking))
        {
            return await booking;
        }

        var commissionReportApiResponseRaw = await GetCommissionReportApiResponse(weekEndingDate);

        var serializerOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new DateOnlyConverter(),
                new BookingConverter(),
                new AdjustmentTypeConverter(),
                new AdjustmentConverter(),
            }
        };

        var commissionReportApiResponse = JsonSerializer.Deserialize<CommissionDetailReportResponse>(commissionReportApiResponseRaw, serializerOptions)
            ?? throw new InvalidDataException("Commission report could not be loaded");
        if (!commissionReportApiResponse.Valid) throw new InvalidDataException("Commission report is marked invalid");

        var allBookings =
            from bookingGroup in commissionReportApiResponse.ConeCommisonTrackingHistoryDetailDtos.Values
            from companyBookingGroup in bookingGroup.Values
            from numberedBookingGroup in companyBookingGroup.Values
            from bookingRecord in numberedBookingGroup
            where bookingRecord.Valid
            select bookingRecord;
        var allAdjustments =
            from adjustmentGroup in commissionReportApiResponse.AdjustmentAmountsDtos.Values
            from adjustmentRecord in adjustmentGroup
            where adjustmentRecord.Valid
            select adjustmentRecord;

        var commissionReport = new CommissionReport
        {
            Bookings = allBookings.ToList(),
            Adjustments = allAdjustments.ToList(),
            Valid = commissionReportApiResponse.Valid
        };

        commissionReport = commissionReport with
        {
            Bookings = commissionReport.Bookings.Select(b =>
                b with
                {
                    Agent = b.Agent with
                    {
                        Settings = agentSettingsProvider.GetOrDefault(b.Agent.Id)
                    }
                }
            ).ToList(),
            Adjustments = commissionReport.Adjustments.Select(a =>
                a with
                {
                    Agent = a.Agent with
                    {
                        Settings = agentSettingsProvider.GetOrDefault(a.Agent.Id)
                    }
                }
            ).ToList()
        };

        return (commissionReport.Bookings, commissionReport.Adjustments);
    }

    private readonly Dictionary<string, string> apiResponses = new();
    private async Task<string> GetCommissionReportApiResponse(DateOnly weekEndingDate)
    {
        var payload = new
        {
            agentIds = new List<int>(),
            weekEndingDates = new List<string> { weekEndingDate.ToString() },
            fileType = "On-Screen View",
            reportType = "detail",
            reportPeriod = "commissionHistory",
            bcAgencyId = agencyId,
            caseId = "",
            payrollYear = null as object,
        };

        if (!apiResponses.ContainsKey(weekEndingDate.ToString()))
        {
            var response = await httpClient.PostAsJsonAsync(
                "/rpe-api/report/getCommTrackingHistoryDetailReport",
                payload
            );
            apiResponses[weekEndingDate.ToString()] = await response.Content.ReadAsStringAsync();
        }

        return apiResponses[weekEndingDate.ToString()];
    }

    private Lazy<Task<IReadOnlyCollection<DateOnly>>>? _getWeekEndingDates;
    public async Task<IReadOnlyCollection<DateOnly>> GetWeekEndingDates()
    {
        _getWeekEndingDates ??= new(async () =>
        {
            var response = await httpClient.GetAsync("/rpe-api/report/getWeekEndingDates");
            var result = await response.Content.ReadAsStringAsync();

            var serializerOptions = new JsonSerializerOptions
            {
                Converters = { new DateOnlyConverter() }
            };

            var weekEndingDates = JsonSerializer.Deserialize<IEnumerable<WeekEndingDate>>(result, serializerOptions)
                ?? throw new InvalidDataException("Week ending dates could not be loaded");

            return weekEndingDates
                .Where(d => d.Valid)
                .Select(d => d.Date)
                .ToList();
        });

        return await _getWeekEndingDates.Value;
    }

    private Lazy<Task<IReadOnlyCollection<Agent>>>? _getAgents;
    public async Task<IReadOnlyCollection<Agent>> GetAgents()
    {
        _getAgents ??= new(async () =>
        {
            var response = await httpClient.GetAsync($"rpe-api/common/agencyusers/{agencyId}");
            return (await response.Content.ReadFromJsonAsync<List<AgentListRecord>>())!
                .Select(agent => (agent, valid: agentSettingsProvider.TryGet(agent.Id, out var settings), settings))
                .Where(t => t.valid)
                .Select(t => t.agent with
                {
                    Settings = t.settings
                })
                .ToList();
        });

        return await _getAgents.Value;
    }
}
