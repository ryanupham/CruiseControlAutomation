using System.Text.Json;

namespace CruiseControl.DataAccess;
public class JsonFileDataStore<T> : ISimpleDataStore<T>
{
    protected readonly ISimpleDataStore<string> dataStore;
    protected JsonSerializerOptions? options;

    public JsonFileDataStore(
        ISimpleDataStore<string> dataStore,
        JsonSerializerOptions? options = null)
    {
        this.dataStore = dataStore;
        this.options = options;
    }

    public T? Read() =>
        JsonSerializer.Deserialize<T?>(
            dataStore.Read() ?? string.Empty, options);

    public void Write(T? contents) =>
        dataStore.Write(JsonSerializer.Serialize(contents, options));
}
