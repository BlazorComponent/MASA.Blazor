using System.Timers;
using Timer = System.Timers.Timer;

namespace Masa.Blazor;

public partial class MCarousel : MWindow, ICarousel, IDisposable
{
    [Parameter]
    public bool Cycle
    {
        get => GetValue(false);
        set => SetValue(value);
    }

    [Parameter]
    public string DelimiterIcon { get; set; }

    [Parameter]
    public StringNumber Height
    {
        get => GetValue((StringNumber)500);
        set => SetValue(value);
    }

    [Parameter]
    public bool HideDelimiters { get; set; }

    [Parameter]
    public bool HideDelimiterBackground { get; set; }

    [Parameter]
    public int Interval
    {
        get => GetValue(6000);
        set => SetValue(value);
    }

    [Parameter]
    public bool Progress { get; set; }

    [Parameter]
    public string ProgressColor { get; set; }

    /// <summary>
    /// TODO: enum
    /// </summary>
    [Parameter]
    public string VerticalDelimiters { get; set; }

    public override bool IsDark
    {
        get
        {
            if (Dark)
            {
                return true;
            }

            return !Light;
        }
    }

    public override bool ArrowsVisible => !IsVertical && base.ArrowsVisible;

    private bool IsVertical => VerticalDelimiters is not null;

    public StringNumber InternalValue => Value;

    public double ProgressValue => Items.Count > 0 ? (InternalIndex + 1d) / Items.Count * 100 : 0;

    private int SlideTimeout { get; set; }

    private Timer Timer { get; set; }

    public StringNumber InternalHeight { get; private set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        Continuous = true;
        Mandatory = true;
        ShowArrows = true;

        return base.SetParametersAsync(parameters);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        InternalHeight = Height;

        if (Timer == null)
        {
            Timer = new Timer
            {
                Interval = Interval > 0 ? Interval : 6000
            };

            Timer.Elapsed += TimerOnElapsed;
        }

        StartTimeout();
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        Next();
    }

    protected override void SetComponentClass()
    {
        base.SetComponentClass();

        CssProvider
            .Merge(cssBuilder =>
            {
                cssBuilder.Add("m-carousel")
                          .AddIf("m-carousel--hide-delimiter-background", () => HideDelimiterBackground)
                          .AddIf("m-carousel--vertical-delimiters", () => IsVertical);
            }, styleBuilder =>
            {
                styleBuilder.AddHeight(InternalHeight);
            })
            .Apply("controls", cssBuilder =>
            {
                cssBuilder.Add("m-carousel__controls");
            }, styleBuilder =>
            {
                styleBuilder
                    .Add(() => $"left: {(VerticalDelimiters == "left" && IsVertical ? "0" : "auto")}")
                    .Add(() => $"right: {(VerticalDelimiters == "right" ? "0" : "auto")}");
            });

        AbstractProvider
            .Apply(typeof(BCarouselDelimiters<>), typeof(BCarouselDelimiters<MCarousel>))
            .Apply(typeof(BCarouselProgress<>), typeof(BCarouselProgress<MCarousel>))
            .Apply(typeof(BItemGroup), typeof(MItemGroup))
            .Apply<BButton, MButton>("controls-item", attrs =>
            {
                attrs[nameof(MButton.Class)] = "m-carousel__controls__item";
                attrs[nameof(MButton.Icon)] = true;
                attrs[nameof(MButton.Small)] = true;
            })
            .Apply<BIcon, MIcon>("controls-item", attrs => { attrs[nameof(MIcon.Size)] = (StringNumber)18; })
            .Apply<BProgressLinear, MProgressLinear>();
    }

    protected override void OnWatcherInitialized()
    {
        base.OnWatcherInitialized();

        Watcher.Watch<StringNumber>(nameof(Value), RestartTimeout)
               .Watch<int>(nameof(Interval), RestartTimeout)
               .Watch<StringNumber>(nameof(Height), (val, oldVal) =>
               {
                   Console.WriteLine($"val:{val} oldVal:{oldVal}");
                   if (string.IsNullOrWhiteSpace(val.ToString()))
                   {
                       return;
                   }

                   InternalHeight = val;
               })
               .Watch<bool>(nameof(Cycle), val =>
               {
                   if (val)
                   {
                       RestartTimeout();
                   }
                   else
                   {
                       Timer.Stop();
                   }

                   StateHasChanged();
               });
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Interval < 0)
        {
            Interval = 6000;
        }
    }

    public async Task InternalValueChanged(StringNumber val)
    {
        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(val);
        }
        else
        {
            Value = val;
        }
    }

    private void RestartTimeout()
    {
        if (Timer is null) return;

        Timer.Stop();
        StateHasChanged();
        StartTimeout();
    }

    private void StartTimeout()
    {
        if (!Cycle) return;

        Timer.Start();
    }

    public override void Dispose()
    {
        base.Dispose();

        if (Timer is not null)
        {
            Timer.Elapsed -= TimerOnElapsed;
        }
    }
}
