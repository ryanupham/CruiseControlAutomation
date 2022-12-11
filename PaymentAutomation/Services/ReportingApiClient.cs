using PaymentAutomation.JsonConverters;
using PaymentAutomation.Models;
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
    private readonly Uri baseAddress;
    private readonly long agencyId;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly CruiseControlCookies cruiseControlCookies;
    private readonly IAgentSettingsProvider agentSettingsProvider;

    public ReportingApiClient(
        Uri baseAddress,
        long agencyId,
        IHttpClientFactory httpClientFactory,
        CruiseControlCookies cruiseControlCookies,
        IAgentSettingsProvider agentSettingsProvider
    )
    {
        this.baseAddress = baseAddress;
        this.agencyId = agencyId;
        this.httpClientFactory = httpClientFactory;
        this.cruiseControlCookies = cruiseControlCookies;
        this.agentSettingsProvider = agentSettingsProvider;
    }

    private readonly Dictionary<DateOnly, Task<(IReadOnlyCollection<Booking> bookings, IReadOnlyCollection<Adjustment> adjustments)>> bookingsAndAdjustmentsForWeekEnding = new();
    public async Task<(IReadOnlyCollection<Booking> bookings, IReadOnlyCollection<Adjustment> adjustments)> GetBookingsAndAdjustmentsForWeekEnding(DateOnly weekEndingDate)
    {
        if (bookingsAndAdjustmentsForWeekEnding.TryGetValue(weekEndingDate, out var booking))
        {
            return await booking;
        }

        var commissionReportApiResponse = await GetCommissionReportApiResponse(weekEndingDate);

        var serializerOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new BookingCollectionConverter(),
                new AdjustmentCollectionConverter(),
                new AdjustmentTypeConverter(),
                new DateOnlyConverter(),
            }
        };

        var commissionReport = JsonSerializer.Deserialize<CommissionReport>(
            commissionReportApiResponse, serializerOptions)!;
        if (!commissionReport.IsValid) throw new InvalidDataException("Data marked invalid");

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

        var httpClient = GetHttpClientWithAuthorization();

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
            var httpClient = GetHttpClientWithAuthorization();

            var response = await httpClient.GetAsync("/rpe-api/report/getWeekEndingDates");
            var result = await response.Content.ReadAsStringAsync();

            var serializerOptions = new JsonSerializerOptions
            {
                Converters = { new WeekEndingDatesConverter() }
            };

            return JsonSerializer.Deserialize<IReadOnlyCollection<DateOnly>>(result, serializerOptions)!;
        });

        return await _getWeekEndingDates.Value;
    }

    private Lazy<Task<IReadOnlyCollection<Agent>>>? _getAgents;
    public async Task<IReadOnlyCollection<Agent>> GetAgents()
    {
        _getAgents ??= new(async () =>
        {
            var httpClient = GetHttpClientWithAuthorization();

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

    private HttpClient GetHttpClientWithAuthorization()
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Cookie", $"{nameof(cruiseControlCookies.NSC_TMAS)}={cruiseControlCookies.NSC_TMAS}");
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", cruiseControlCookies.accessToken);
        httpClient.BaseAddress = baseAddress;

        return httpClient;
    }
}
