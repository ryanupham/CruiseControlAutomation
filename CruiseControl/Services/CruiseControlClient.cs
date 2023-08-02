namespace CruiseControl.Services;
public class CruiseControlClient
{
    private readonly HttpClient httpClient;

    public CruiseControlClient(HttpClient httpClient) =>
        this.httpClient = httpClient;

    public async Task<HttpResponseMessage> GetAsync(string url) =>
        await httpClient.GetAsync(url);

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content) =>
        await httpClient.PostAsync(url, content);

    public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content) =>
        await httpClient.PutAsync(url, content);

    public async Task<HttpResponseMessage> DeleteAsync(string url) =>
        await httpClient.DeleteAsync(url);
}
