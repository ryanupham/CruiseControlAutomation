namespace CruiseControl.DataAccess;
public interface ISimpleDataStore<T>
{
    T? Read();
    void Write(T? contents);
}
