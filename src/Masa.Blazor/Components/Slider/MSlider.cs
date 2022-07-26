using BlazorComponent.Web;
using Microsoft.AspNetCore.Components.Web;

namespace Masa.Blazor
{
    public class MSlider<TValue> : MInput<TValue>, ISlider<TValue>
    {
        [Parameter]
        public bool Vertical { get; set; }

        [Inject]
        public Document Document { get; set; }

        [Parameter]
        public double Max { get; set; } = 100;

        [Parameter]
        public double Min { get; set; } = 0;

        [Parameter]
        public double Step { get; set; } = 1;

        [Parameter]
        public List<string> TickLabels { get; set; } = new();

        [Parameter]
        public StringBoolean Ticks { get; set; } = false;

        [Inject]
        public MasaBlazor MasaBlazor { get; set; }

        [Parameter]
        public string TrackColor { get; set; }

        [Parameter]
        public string TrackFillColor { get; set; }

        [Parameter]
        public double TickSize { get; set; } = 2;

        [Parameter]
        public StringBoolean ThumbLabel { get; set; }

        [Parameter]
        public RenderFragment<double> ThumbLabelContent { get; set; }

        public override int DebounceMilliseconds { get; set; }

        protected virtual double GetRoundedValue(int index)
        {
            return RoundValue(DoubleInternalValue);
        }

        RenderFragment<int> ISlider<TValue>.ThumbLabelContent
        {
            get
            {
                if (ThumbLabelContent != null)
                {
                    return context => ThumbLabelContent(GetRoundedValue(context));
                }

                return context => new RenderFragment(builder =>
                 {
                     builder.OpenElement(0, "span");
                     builder.AddContent(1, GetRoundedValue(context));
                     builder.CloseElement();
                 });
            }
        }

        [Parameter]
        public string ThumbColor { get; set; }

        [Parameter]
        public StringNumber ThumbSize { get; set; } = 32;

        [Parameter]
        public EventCallback<FocusEventArgs> OnFocus { get; set; }

        [Parameter]
        public EventCallback<FocusEventArgs> OnBlur { get; set; }

        [Parameter]
        public bool InverseLabel { get; set; }

        [Parameter]
        public StringNumber LoaderHeight { get; set; } = 2;

        [Parameter]
        public RenderFragment ProgressContent { get; set; }

        protected virtual double DoubleInternalValue
        {
            get
            {
                return InternalValue is double val ? val : default;
            }
            set
            {
                var val = RoundValue(Math.Min(Math.Max(value, Min), Max));
                InternalValue = val is TValue v ? v : default;
            }
        }

        public bool IsActive { get; set; }

        public bool NoClick { get; set; }

        public ElementReference ThumbElement { get; set; }

        public ElementReference TrackElement { get; set; }

        public bool ThumbPressed { get; set; }

        public double StartOffset { get; set; }

        public double OldValue { get; set; }

        public CancellationTokenSource MouseCancellationTokenSource { get; set; }

        public HtmlElement App { get; set; }

        public Dictionary<string, object> InputAttrs { get; } = new();

        public string TrackTransition
        {
            get
            {
                if (ThumbPressed)
                {
                    if (ShowTicks || Step > 0)
                    {
                        return "0.1s cubic-bezier(0.25, 0.8, 0.5, 1)";
                    }
                    else
                    {
                        return "none";
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public bool ShowTicks
        {
            get
            {
                return TickLabels.Count > 0 || (!IsDisabled && Step > 0 && Ticks != false);
            }
        }

        public double InputWidth
        {
            get
            {
                return (RoundValue(DoubleInternalValue) - Min) / (Max - Min) * 100;
            }
        }

        public double StepNumeric
        {
            get
            {
                return Step > 0 ? Step : 0;
            }
        }

        public string ComputedTrackColor
        {
            get
            {
                if (IsDisabled)
                {
                    return null;
                }

                if (TrackColor != null)
                {
                    return TrackColor;
                }

                if (IsDark)
                {
                    return ValidationState;
                }

                return string.IsNullOrEmpty(ValidationState) ? "primary lighten-3" : ValidationState;
            }
        }

        public string ComputedTrackFillColor
        {
            get
            {
                if (IsDisabled)
                {
                    return null;
                }

                if (TrackFillColor != null)
                {
                    return TrackFillColor;
                }

                return string.IsNullOrEmpty(ValidationState) ? ComputedColor : ValidationState;
            }
        }

        public string ComputedThumbColor
        {
            get
            {
                if (ThumbColor != null)
                {
                    return ThumbColor;
                }

                return string.IsNullOrEmpty(ValidationState) ? ComputedColor : ValidationState;
            }
        }

        public double NumTicks
        {
            get
            {
                return Math.Ceiling((Max - Min) / StepNumeric);
            }
        }

        public bool ShowThumbLabel
        {
            get
            {
                return !IsDisabled && ((ThumbLabel != null && ThumbLabel != false) || ThumbLabelContent != null);
            }
        }

        public Dictionary<string, object> ThumbAttrs => new()
        {
            { "role", "slider" },
            { "tabindex", IsDisabled ? -1 : 0 }
        };

        public bool ShowThumbLabelContainer => IsFocused || IsActive || ThumbLabel == "always";

        protected virtual async Task SetInternalValueAsync(double internalValue)
        {
            var val = RoundValue(Math.Min(Math.Max(internalValue, Min), Max));
            // await SetInternalValueAsync(val is TValue v ? v : default);
            InternalValue = val is TValue v ? v : default;
        }

        protected override void OnWatcherInitialized()
        {
            Watcher
                .Watch<TValue>(nameof(Value), val =>
                {
                    //Value may not between min and max
                    //If that so,we should invoke ValueChanged 
                    var roundedVal = ConvertDoubleToTValue(RoundValue(Math.Min(Math.Max(Convert.ToDouble(val), Min), Max)));
                    if (!EqualityComparer<TValue>.Default.Equals(val, roundedVal) && ValueChanged.HasDelegate)
                    {
                        NextTick(async () =>
                        {
                            await ValueChanged.InvokeAsync(roundedVal);
                        });
                    }

                    LazyValue = roundedVal;
                });
        }

        private static TValue ConvertDoubleToTValue(double val)
        {
            return val is TValue value ? value : default;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (App == null)
            {
                App = Document.QuerySelector("[data-app]");
            }

            CheckTValue();
        }

        protected virtual void CheckTValue()
        {
            if (typeof(TValue) != typeof(double))
            {
                throw new ArgumentNullException(nameof(TValue), "Only double supported");
            }
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (OnChange.HasDelegate)
            {
                ValueChanged = OnChange;
            }
        }

        public virtual async Task HandleOnSliderClickAsync(MouseEventArgs args)
        {
            if (NoClick)
            {
                NoClick = false;
                return;
            }

            await ThumbElement.FocusAsync();
            await HandleOnMouseMoveAsync(args);
        }

        public virtual async Task HandleOnTouchStartAsync(ExTouchEventArgs args)
        {
            Console.WriteLine("touch start");
            await HandleOnSliderStartSwiping(args.Target, args.Touches[0].ClientX, args.Touches[0].ClientY);
            await App.AddEventListenerAsync("touchmove", CreateEventCallback<TouchEventArgs>(HandleTouchMove), false, new EventListenerExtras() { PreventDefault = true, StopPropagation = true });
            await App.AddEventListenerAsync("touchend", CreateEventCallback<TouchEventArgs>(HandleOnTouchEndAsync), new EventListenerOptions
            {
                Capture = true,
                Once = true
            });
        }

        public virtual async Task HandleOnSliderMouseDownAsync(ExMouseEventArgs args)
        {
            await HandleOnSliderStartSwiping(args.Target, args.ClientX, args.ClientY);

            await App.AddEventListenerAsync("mousemove", CreateEventCallback<MouseEventArgs>(HandleOnMouseMoveAsync), false, new EventListenerExtras() { PreventDefault = true, StopPropagation = true });
            await App.AddEventListenerAsync("mouseup", CreateEventCallback<MouseEventArgs>(HandleOnSliderMouseUpAsync), new EventListenerOptions
            {
                Capture = true,
                Once = true
            }, new EventListenerExtras() { PreventDefault = true, StopPropagation = true });
        }

        public async Task HandleTouchMove(TouchEventArgs args)
        {
            var mouseEventArgs = new MouseEventArgs()
            {
                ClientX = args.Touches[0].ClientX,
                ClientY = args.Touches[0].ClientY,
            };

            await HandleOnMouseMoveAsync(mouseEventArgs);
        }

        public virtual async Task HandleOnSliderStartSwiping(EventTarget target, double clientX, double clientY)
        {
            OldValue = DoubleInternalValue;
            IsActive = true;

            if (target.Class.Contains("m-slider__thumb-container"))
            {
                ThumbPressed = true;
                var container = Document.QuerySelector($"#{Id} .m-slider__thumb-container");
                var domRect = await container.GetBoundingClientRectAsync();
                StartOffset = Vertical ? (clientX - (domRect.Top + domRect.Height / 2)) : (clientY - (domRect.Left + domRect.Width / 2));
            }
            else
            {
                StartOffset = 0;

                MouseCancellationTokenSource?.Cancel();
                MouseCancellationTokenSource = new CancellationTokenSource();

                _ = Task.Run(async () =>
                {
                    await Task.Delay(300, MouseCancellationTokenSource.Token);
                    ThumbPressed = true;

                    InvokeStateHasChanged();
                });
            }
            var args = new MouseEventArgs()
            {
                ClientX = clientX,
                ClientY = clientY,
            };

            await HandleOnMouseMoveAsync(args);
        }

        public async Task HandleOnTouchEndAsync(TouchEventArgs args)
        {
            Console.WriteLine("touch end");
            await HandleOnSliderEndSwiping();
        }

        public async Task HandleOnSliderMouseUpAsync(MouseEventArgs args)
        {
            await HandleOnSliderEndSwiping();
        }

        public async Task HandleOnSliderEndSwiping()
        {
            MouseCancellationTokenSource?.Cancel();
            ThumbPressed = false;
            await App.RemoveEventListenerAsync("mousemove");
            await App.RemoveEventListenerAsync("touchmove");

            IsActive = false;
        }

        public virtual async Task HandleOnMouseMoveAsync(MouseEventArgs args)
        {
            if (args.Type == "mousemove")
            {
                ThumbPressed = true;
            }
           
            var val = await ParseMouseMoveAsync(args);

            await SetInternalValueAsync(val);
        }

        protected async Task<double> ParseMouseMoveAsync(MouseEventArgs args)
        {
            var track = Document.GetElementByReference(TrackElement);
            var rect = await track.GetBoundingClientRectAsync();

            var tractStart = Vertical ? rect.Top : rect.Left;
            var trackLength = Vertical ? rect.Height : rect.Width;
            var clickOffset = Vertical ? args.ClientY : args.ClientX;

            var clickPos = Math.Min(Math.Max((clickOffset - tractStart - StartOffset) / trackLength, 0), 1);
            if (Vertical)
            {
                clickPos = 1 - clickPos;
            }
            //TODO:rtl

            return Min + clickPos * (Max - Min);
        }

        protected double RoundValue(double value)
        {
            if (StepNumeric == 0)
            {
                return value;
            }

            var trimmedStep = Step.ToString().Trim();
            var decimals = trimmedStep.IndexOf('.') > -1
                ? (trimmedStep.Length - trimmedStep.IndexOf('.') - 1)
                : 0;
            var offset = Min % StepNumeric;

            var newValue = Math.Round((value - offset) / StepNumeric) * StepNumeric + offset;
            var rounded = Math.Round(Math.Min(newValue, Max), decimals);
            if (rounded == 0)
            {
                //Avoid -0
                rounded = Math.Abs(rounded);
            }

            return rounded;
        }

        protected virtual bool IsThumbActive(int index)
        {
            return IsActive;
        }

        protected virtual bool IsThumbFocus(int index)
        {
            return IsFocused;
        }

        protected override void SetComponentClass()
        {
            base.SetComponentClass();

            CssProvider
                .Merge(cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input__slider")
                        .AddIf("m-input__slider--vertical", () => Vertical)
                        .AddIf("m-input__slider--inverse-label", () => InverseLabel);
                })
                .Apply("slider", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-slider")
                        .AddIf("m-slider--horizontal", () => !Vertical)
                        .AddIf("m-slider--vertical", () => Vertical)
                        .AddIf("m-slider--focused", () => IsFocused)
                        .AddIf("m-slider--active", () => IsActive)
                        .AddIf("m-slider--disabled", () => IsDisabled)
                        .AddIf("m-slider--readonly", () => IsReadonly)
                        .AddTheme(IsDark);
                })
                .Apply("track-container", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-slider__track-container");
                })
                .Apply("track-background", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-slider__track-background")
                        .AddBackgroundColor(ComputedTrackColor);
                }, styleBuilder =>
                {
                    //TODO: change here
                    var startDir = Vertical ? MasaBlazor.RTL ? "bottom" : "top" : MasaBlazor.RTL ? "left" : "right";
                    var endDir = Vertical ? "height" : "width";

                    var start = "0px";
                    var end = IsDisabled ? $"calc({100 - InputWidth}% - 10px)" : $"calc({100 - InputWidth}%)";

                    styleBuilder
                        .AddBackgroundColor(ComputedTrackColor)
                        .AddTransition(TrackTransition)
                        .Add($"{startDir}:{start}")
                        .Add($"{endDir}:{end}");
                })
                .Apply("track-fill", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-slider__track-fill")
                        .AddBackgroundColor(ComputedTrackFillColor);
                }, styleBuilder =>
                {
                    var startDir = Vertical ? "bottom" : "left";
                    var endDir = Vertical ? "top" : "right";
                    var valueDir = Vertical ? "height" : "width";

                    var start = MasaBlazor.RTL ? "auto" : "0";
                    var end = MasaBlazor.RTL ? "0" : "auto";
                    var value = IsDisabled ? $"calc({InputWidth}% - 10px)" : $"{InputWidth}%";

                    styleBuilder
                        .AddBackgroundColor(ComputedTrackFillColor)
                        .AddTransition(TrackTransition)
                        .Add($"{startDir}:{start}")
                        .Add($"{endDir}:{end}")
                        .Add($"{valueDir}:{value}");
                })
                .Apply("ticks-container", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-slider__ticks-container")
                        .AddIf("m-slider__ticks-container--always-show", () => Ticks == "always" || TickLabels.Count > 0);
                })
                .Apply("tick", cssBuilder =>
                {
                    var width = cssBuilder.Index * (100 / NumTicks);
                    var filled = MasaBlazor.RTL ? (100 - InputWidth) < width : width < InputWidth;

                    cssBuilder
                        .Add("m-slider__tick")
                        .AddIf("m-slider__tick--filled", () => filled);
                }, styleBuilder =>
                {
                    var direction = Vertical ? "bottom" : (MasaBlazor.RTL ? "right" : "left");
                    var offsetDirection = Vertical ? (MasaBlazor.RTL ? "left" : "right") : "top";
                    var width = styleBuilder.Index * (100 / NumTicks);

                    styleBuilder
                        .AddWidth(TickSize)
                        .AddHeight(TickSize)
                        .Add($"{direction}:calc({width}% - {TickSize / 2}px)")
                        .Add($"{offsetDirection}:calc(50% - {TickSize / 2}px)");
                })
                .Apply("tick-label", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-slider__tick-label");
                })
                .Apply("thumb-container", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-slider__thumb-container")
                        .AddIf("m-slider__thumb-container--active", () => IsThumbActive(cssBuilder.Index))
                        .AddIf("m-slider__thumb-container--focused", () => IsThumbFocus(cssBuilder.Index))
                        .AddIf("m-slider__thumb-container--show-label", () => ShowThumbLabel)
                        .AddTextColor(ComputedThumbColor);
                }, styleBuilder =>
                {
                    GetThumbContainerStyles(styleBuilder);
                })
                .Apply("thumb", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-slider__thumb")
                        .AddBackgroundColor(ComputedThumbColor);
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddBackgroundColor(ComputedThumbColor);
                })
                .Apply("thumb-label-container", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-slider__thumb-label-container");
                })
                .Apply("thumb-label", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-slider__thumb-label")
                        .AddBackgroundColor(ComputedThumbColor);
                }, styleBuilder =>
                {
                    var transform = Vertical
                        ? $"translateY(20%) translateY({(ThumbSize.ToInt32() / 3) - 1}px) translateX(55%) rotate(135deg)"
                        : "translateY(-20%) translateY(-12px) translateX(-50%) rotate(45deg)";
                    styleBuilder
                        .AddHeight(ThumbSize)
                        .AddWidth(ThumbSize)
                        .Add($"transform:{transform}")
                        .AddBackgroundColor(ComputedThumbColor);
                });

            AbstractProvider
                .ApplySliderDefault<TValue>();
        }

        protected virtual void GetThumbContainerStyles(StyleBuilder styleBuilder)
        {
            var direction = Vertical ? "top" : "left";
            var value = MasaBlazor.RTL ? 100 - InputWidth : InputWidth;
            value = Vertical ? 100 - value : value;

            styleBuilder
                .AddTransition(TrackTransition)
                .Add($"{direction}:{value}%")
                .AddTextColor(ComputedThumbColor);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                MasaBlazor.OnRTLChange += OnRTLChange;
            }
        }

        private void OnRTLChange(bool rtl)
        {
            InvokeStateHasChanged();
        }

        public virtual async Task HandleOnFocusAsync(FocusEventArgs args)
        {
            IsFocused = true;

            if (OnFocus.HasDelegate)
            {
                await OnFocus.InvokeAsync(args);
            }
        }

        public virtual async Task HandleOnBlurAsync(FocusEventArgs args)
        {
            IsFocused = false;

            if (OnBlur.HasDelegate)
            {
                await OnBlur.InvokeAsync(args);
            }
        }

        public virtual async Task HandleOnKeyDownAsync(KeyboardEventArgs args)
        {
            if (!IsInteractive)
            {
                return;
            }

            var value = ParseKeyDown(args, DoubleInternalValue);

            if (value == null || value.AsT2 < Min || value.AsT2 > Max)
            {
                return;
            }

            await SetInternalValueAsync(value.AsT2);
            if (OnChange.HasDelegate)
            {
                await OnChange.InvokeAsync(InternalValue);
            }
        }

        protected StringNumber ParseKeyDown(KeyboardEventArgs args, double value)
        {
            if (!IsInteractive)
            {
                return null;
            }

            var keyCodes = new string[] { "pageup", "pagedown", "end", "home", "left", "right", "down", "up" };
            var directionCodes = new string[] { "left", "right", "down", "up" };
            if (!keyCodes.Contains(args.Code))
            {
                return null;
            }

            var step = StepNumeric == 0 ? 1 : StepNumeric;
            var steps = Max - Min / step;
            if (directionCodes.Contains(args.Code))
            {
                var increase = MasaBlazor.RTL ? new string[] { "left", "up" } : new string[] { "right", "up" };
                var direction = increase.Contains(args.Code) ? 1 : -1;
                var multiplier = args.ShiftKey ? 3 : (args.CtrlKey ? 2 : 1);

                value += (direction * step * multiplier);
            }
            else if (args.Code == "home")
            {
                value = Min;
            }
            else if (args.Code == "end")
            {
                value = Max;
            }
            else
            {
                var direction = args.Code == "pagedown" ? 1 : -1;
                value = value - (direction * step * (steps > 100 ? steps / 10 : 10));
            }

            return value;
        }
    }
}
