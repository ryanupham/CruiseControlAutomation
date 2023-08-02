using CruiseControl.DataAccess;
using PriorToTravelEmailSender.JsonConverters;
using PriorToTravelEmailSender.Models;
using System.Text.Json;

namespace PriorToTravelEmailSender.DataAccess;
internal class ExecutionHistoryDetailsDataStore
    : JsonFileDataStore<ExecutionHistoryDetails>
{
    public ExecutionHistoryDetailsDataStore(ISimpleDataStore<string> dataStore)
        : base(
            dataStore,
            new JsonSerializerOptions
            {   
                Converters = { new DateOnlyConverter() }
            }) { }
}
