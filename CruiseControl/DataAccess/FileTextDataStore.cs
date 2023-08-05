namespace CruiseControl.DataAccess;
public class FileTextDataStore : ISimpleDataStore<string>
{
    private readonly string filePath;

    public FileTextDataStore(string filePath) =>
        this.filePath = filePath;

    public string Read() =>
        File.ReadAllText(filePath);

    public void Write(string? contents) =>
        File.WriteAllText(filePath, contents);
}
