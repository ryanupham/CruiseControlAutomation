namespace PaymentAutomation.Utilities.ConsoleOptions;
internal class ConsoleOptionSelector<T>
{
    private readonly OptionList<T> options;
    private readonly string selectTitle;

    public ConsoleOptionSelector(OptionList<T> options, string selectTitle = "")
    {
        this.options = options;
        this.selectTitle = selectTitle;
    }

    public T GetSelectedOption()
    {
        ConsoleKey key;

        do
        {
            DrawOptions(options);

            key = Console.ReadKey().Key;
            options.SelectedIndex = key switch
            {
                ConsoleKey.UpArrow =>
                    Math.Max(options.SelectedIndex - 1, 0),
                ConsoleKey.DownArrow =>
                    Math.Min(options.SelectedIndex + 1, options.Count - 1),
                _ => options.SelectedIndex
            };
        } while (key != ConsoleKey.Enter);

        return options[options.SelectedIndex].Value;
    }

    private void DrawOptions(OptionList<T> options)
    {
        var optionTexts = Enumerable.Range(-2, 5)
            .Select(indexModifier =>
            {
                var index = options.SelectedIndex + indexModifier;
                if (index < 0 || index >= options.Count) return "";
                if (indexModifier == -2 && index > 0) return "...";
                if (indexModifier == 2 && index < options.Count - 1) return "...";

                var isSelected = index == options.SelectedIndex;
                return $"{(isSelected ? ">" : " ")}{options[index].Description}";
            });

        Console.Clear();
        Console.CursorVisible = false;
        if (!string.IsNullOrEmpty(selectTitle))
        {
            Console.WriteLine(selectTitle);
        }
        Console.Write(string.Join('\n', optionTexts));
    }
}
