namespace Masa.Blazor.Presets;

public partial class PCron
{
    [Inject]
    I18n I18n { get; set; }

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    private bool _hasError;

    private string _errorMessage = string.Empty;

    private StringNumber _selectedPeriod;

    private List<CronItemModel> CronItems { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        foreach (var item in Enum.GetValues<PeriodTypes>())
        {
            var cronDefaultValue = "*";

            if (item == PeriodTypes.Week)
            {
                cronDefaultValue = "?";
            }
            else if (item == PeriodTypes.Year)
            {
                cronDefaultValue = string.Empty;
            }

            CronItems.Add(new CronItemModel() { Period = item, CronValue = cronDefaultValue });
        }

        await CalculateCronValue();

        await base.OnInitializedAsync();
    }

    private Task CronValueHasChanged(CronItemModel cronItem)
    {
        if (cronItem != null)
        {
            if (cronItem.Period == PeriodTypes.Week)
            {
                var dayItem = CronItems.FirstOrDefault(p => p.Period == PeriodTypes.Day);

                dayItem.CronValue = cronItem.CronValue == "?" ? "*" : "?";
            }
            else if (cronItem.Period == PeriodTypes.Day)
            {
                var dayItem = CronItems.FirstOrDefault(p => p.Period == PeriodTypes.Week);

                dayItem.CronValue = cronItem.CronValue == "?" ? "*" : "?";
            }

            ChangeTimeItemDefaultValue(cronItem);
        }

        CalculateCronValue();

        return Task.CompletedTask;
    }

    private Task ChangeTimeItemDefaultValue(CronItemModel cronItem)
    {
        if (cronItem.Period != PeriodTypes.Second && cronItem.Period != PeriodTypes.Minute && cronItem.Period != PeriodTypes.Hour)
        {
            SetCronValueToZeroWhenDefault(PeriodTypes.Second);
            SetCronValueToZeroWhenDefault(PeriodTypes.Minute);
            SetCronValueToZeroWhenDefault(PeriodTypes.Hour);

            if (cronItem.Period == PeriodTypes.Month)
            {
                SetCronValueToZeroWhenDefault(PeriodTypes.Day, "1");
                SetCronValueToZeroWhenDefault(PeriodTypes.Week, "?");
            }
            else if (cronItem.Period == PeriodTypes.Year)
            {
                SetCronValueToZeroWhenDefault(PeriodTypes.Day, "1");
                SetCronValueToZeroWhenDefault(PeriodTypes.Week, "?");
                SetCronValueToZeroWhenDefault(PeriodTypes.Month, "1");
            }
        }

        return Task.CompletedTask;
    }

    private Task SetCronValueToZeroWhenDefault(PeriodTypes period, string defaultValue = "0")
    {
        var item = CronItems.FirstOrDefault(p => p.Period == period);
        if (item != null && (item.CronValue == "*" || item.CronValue == "?"))
        {
            item.CronValue = defaultValue;
        }

        return Task.CompletedTask;
    }

    private Task CalculateCronValue()
    {
        Value = string.Join(" ", CronItems.OrderBy(p => p.Period).Select(p => p.CronValue));

        try
        {
            CronExpression.ValidateExpression(Value);
            _hasError = false;
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = I18n.T(ex.Message);
        }

        if (ValueChanged.HasDelegate)
        {
            ValueChanged.InvokeAsync(Value);
        }

        return Task.CompletedTask;
    }
}
