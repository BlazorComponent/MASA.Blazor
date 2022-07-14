namespace Masa.Blazor.Presets;

public partial class CronItem
{
    [Inject]
    protected I18n I18n { get; set; } = null!;

    [Parameter]
    public PeriodTypes Period { get; set; }

    [Parameter]
    public string Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (value != _value)
            {
                _value = value;

                InitValue();
            }
        }
    }

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<CronItemModel> CronValueHasChanged { get; set; }

    private CronTypes _selectedCronType;

    private int? _minUnit = null;

    private int? _maxUnit = null;

    private int? _periodStart;

    private int? _periodEnd;

    private int? _startFromPeriod;

    private int _startEveryPeriod;

    private List<int> _allPeriodList = new();

    private List<int> _specifyPeriods = new();

    private string _value;

    private string _i18nUnitText = string.Empty;

    private bool _showNotSpecify;

    private WeekNumbers _selectWeekNumber;

    private DayOfWeek _selectDayOfWeek;

    private List<WeekNumbers> _weekNumbers = new();

    private List<DayOfWeek> _dayOfWeeks = new();

    private int? _lastPeriodOfWeek;

    private int? _nearestOfDay;

    protected override async Task OnInitializedAsync()
    {
        if (Period != PeriodTypes.Week && Period != PeriodTypes.Year)
        {
            _i18nUnitText = I18n.T($"$masaBlazor.{nameof(Period)}.{Period}");
        }

        switch (Period)
        {
            case PeriodTypes.Second:
            case PeriodTypes.Minute:
                _minUnit = 0;
                _maxUnit = 59;
                _allPeriodList = Enumerable.Range(_minUnit.Value, _maxUnit.Value).ToList();
                _showNotSpecify = false;
                break;
            case PeriodTypes.Hour:
                _minUnit = 0;
                _maxUnit = 23;
                _allPeriodList = Enumerable.Range(_minUnit.Value, _maxUnit.Value).ToList();
                _showNotSpecify = false;
                break;
            case PeriodTypes.Day:
                _minUnit = 1;
                _maxUnit = 31;
                _allPeriodList = Enumerable.Range(_minUnit.Value, _maxUnit.Value).ToList();
                _showNotSpecify = true;
                break;
            case PeriodTypes.Month:
                _minUnit = 1;
                _maxUnit = 12;
                _allPeriodList = Enumerable.Range(_minUnit.Value, _maxUnit.Value).ToList();
                _showNotSpecify = false;
                break;
            case PeriodTypes.Week:
                _minUnit = 1;
                _maxUnit = 7;
                _allPeriodList = Enumerable.Range(_minUnit.Value, _maxUnit.Value).ToList();
                _showNotSpecify = true;
                break;
            case PeriodTypes.Year:
                _minUnit = 1;
                break;
            default:
                break;
        }

        if (Period == PeriodTypes.Week)
        {
            _weekNumbers = Enum.GetValues<WeekNumbers>().ToList();
            _dayOfWeeks = Enum.GetValues<DayOfWeek>().ToList();
        }

        _periodStart = Period == PeriodTypes.Year ? DateTime.Now.Year : _minUnit;
        _periodEnd = Period == PeriodTypes.Year ? DateTime.Now.AddYears(10).Year : 2;
        _startFromPeriod = Period == PeriodTypes.Year ? DateTime.Now.Year : _minUnit;
        _startEveryPeriod = 1;
        _nearestOfDay = _minUnit;
        _lastPeriodOfWeek = _minUnit;
        _selectWeekNumber = WeekNumbers.First;
        await base.OnInitializedAsync();
    }

    private Task OnSelectedCronTypeChanged(CronTypes type)
    {
        if (_selectedCronType != type)
        {
            _selectedCronType = type;

            CalculateCronValue();
        }
        return Task.CompletedTask;
    }

    private Task OnPeriodStartChanged(int? periodStart)
    {
        if (_periodStart != periodStart)
        {
            _periodStart = periodStart;

            CalculateCronValue();
        }
        return Task.CompletedTask;
    }

    private Task OnPeriodEndChanged(int? periodEnd)
    {
        if (_periodEnd != periodEnd)
        {
            _periodEnd = periodEnd;
            CalculateCronValue();
        }
        return Task.CompletedTask;
    }

    private Task OnNearestOfDayChanged(int? nearestOfDay)
    {
        if (_nearestOfDay != nearestOfDay)
        {
            _nearestOfDay = nearestOfDay;
            CalculateCronValue();
        }
        return Task.CompletedTask;
    }

    private Task OnLastPeriodOfWeekChanged(int? lastPeriodOfWeek)
    {
        if (_lastPeriodOfWeek != lastPeriodOfWeek)
        {
            _lastPeriodOfWeek = lastPeriodOfWeek;
            CalculateCronValue();
        }
        return Task.CompletedTask;
    }

    private Task OnStartFromPeriodChange(int? startFromPeriod)
    {
        if (_startFromPeriod != startFromPeriod)
        {
            _startFromPeriod = startFromPeriod;
            CalculateCronValue();
        }
        return Task.CompletedTask;
    }

    private Task OnStartEveryPeriod(int startEveryPeriod)
    {
        if (_startEveryPeriod != startEveryPeriod)
        {
            _startEveryPeriod = startEveryPeriod;
            CalculateCronValue();
        }
        return Task.CompletedTask;
    }

    private Task OnSpecifyPeriodChanged(List<int> specifyPeriods)
    {
        if (_specifyPeriods != specifyPeriods)
        {
            _specifyPeriods = specifyPeriods;
            CalculateCronValue();
        }
        return Task.CompletedTask;
    }

    private Task OnWeekNumberChanged(WeekNumbers weekNumber)
    {
        if (_selectWeekNumber != weekNumber)
        {
            _selectWeekNumber = weekNumber;
            CalculateCronValue();
        }

        return Task.CompletedTask;
    }

    private Task OnDayOfWeekChanged(DayOfWeek dayOfWeek)
    {
        if (_selectDayOfWeek != dayOfWeek)
        {
            _selectDayOfWeek = dayOfWeek;
            CalculateCronValue();
        }
        return Task.CompletedTask;
    }

    private Task CalculateCronValue()
    {
        switch (_selectedCronType)
        {
            case CronTypes.Range:
                _value = _periodStart + "-" + _periodEnd;
                break;
            case CronTypes.StartFrom:
                _value = _startFromPeriod + "/" + _startEveryPeriod;
                break;
            case CronTypes.Specify:
                if (_specifyPeriods.Any())
                {
                    _value = string.Join(",", _specifyPeriods);
                }
                else
                {
                    _value = "*";
                }
                break;
            case CronTypes.NotSpecify:
                _value = "?";
                break;
            case CronTypes.WeekStartFrom:
                _value = _selectDayOfWeek.ToString("d") + "#" + _selectWeekNumber.ToString("d");
                break;
            case CronTypes.LastOfPeriod:
                if (Period == PeriodTypes.Day)
                {
                    _value = "L";
                }
                else
                {
                    _value = _lastPeriodOfWeek + "L";
                }
                break;
            case CronTypes.NearestDay:
                _value = _nearestOfDay + "W";
                break;
            default:
                _value = "*";
                break;
        }

        if (ValueChanged.HasDelegate)
        {
            ValueChanged.InvokeAsync(_value);
        }

        if (CronValueHasChanged.HasDelegate)
        {
            CronValueHasChanged.InvokeAsync(new CronItemModel() { Period = Period, CronValue = _value });
        }

        return Task.CompletedTask;
    }

    private string GetWeekNumberI18nString(WeekNumbers weekNumber)
    {
        return I18n.T($"$masaBlazor.weekNumber.{weekNumber}");
    }

    private string GetDayOfWeekI18nString(DayOfWeek dayOfWeek)
    {
        return I18n.T($"$masaBlazor.dayOfWeek.{dayOfWeek}");
    }

    private Task InitValue()
    {
        if (_value == "*")
        {
            _selectedCronType = CronTypes.Period;
        }
        else if (_value.Contains('-'))
        {
            _selectedCronType = CronTypes.Range;
            var splitArr = _value.Split('-');
            if (splitArr.Length == 2)
            {
                _periodStart = int.Parse(splitArr[0]);
                _periodEnd = int.Parse(splitArr[1]);
            }
        }
        else if (_value.Contains('/'))
        {
            _selectedCronType = CronTypes.StartFrom;
            var splitArr = _value.Split('/');
            if (splitArr.Length == 2)
            {
                _startFromPeriod = int.Parse(splitArr[0]);
                _startEveryPeriod = int.Parse(splitArr[1]);
            }
        }
        else if (_value == "?")
        {
            _selectedCronType = CronTypes.NotSpecify;
        }
        else if (_value.Contains('#'))
        {
            _selectedCronType = CronTypes.WeekStartFrom;
            var splitArr = _value.Split("#");
            if (splitArr.Length == 2)
            {
                _selectDayOfWeek = Enum.Parse<DayOfWeek>(splitArr[0]);
                _selectWeekNumber = Enum.Parse<WeekNumbers>(splitArr[1]);
            }
        }
        else if (_value.Contains('L'))
        {
            _selectedCronType = CronTypes.LastOfPeriod;
            if (Period == PeriodTypes.Week)
            {
                _lastPeriodOfWeek = int.Parse(_value[0].ToString());
            }
        }
        else if (_value.Contains('W'))
        {
            _selectedCronType = CronTypes.NearestDay;
            _nearestOfDay = int.Parse(_value[0].ToString());
        }
        else if (string.IsNullOrWhiteSpace(_value))
        {
            _selectedCronType = 0;
        }
        else
        {
            _selectedCronType = CronTypes.Specify;
            _specifyPeriods = _value.Split(',').Select(p => int.Parse(p)).ToList();
        }

        return Task.CompletedTask;
    }
}
