namespace PaymentAutomation.DataAccess;

internal interface IRepository<TKey, TValue>
{
    public bool TryGet(TKey key, out TValue value);
    public TValue Get(TKey key);
    public bool Add(TKey key, TValue value);
    public bool Delete(TKey key);
}
