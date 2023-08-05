namespace PaymentAutomation.Utilities.ConsoleOptions;
internal class OptionList<T> : List<Option<T>>
{
    public int SelectedIndex { get; set; }
}
