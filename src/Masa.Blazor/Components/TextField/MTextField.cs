using BlazorComponent.Web;
using Microsoft.AspNetCore.Components.Web;

namespace Masa.Blazor
{
    public partial class MTextField<TValue> : MInput<TValue>, ITextField<TValue>
    {
        private string _badInput;

        private bool _shouldRender = true;

        [Parameter]
        public virtual bool Clearable { get; set; }

        [Parameter]
        public bool PersistentPlaceholder { get; set; }

        [Parameter]
        public string ClearIcon { get; set; } = "mdi-close";

        [Parameter]
        public bool FullWidth { get; set; }

        [Parameter]
        public string Prefix { get; set; }

        [Parameter]
        public bool SingleLine { get; set; }

        [Parameter]
        public bool Solo { get; set; }

        [Parameter]
        public bool SoloInverted { get; set; }

        [Parameter]
        public bool Flat { get; set; }

        [Parameter]
        public bool Filled { get; set; }

        [Parameter]
        public virtual bool Outlined { get; set; }

        [Parameter]
        public bool Reverse { get; set; }

        [Parameter]
        public string Placeholder { get; set; }

        [Parameter]
        public bool Rounded { get; set; }

        [Parameter]
        public bool Shaped { get; set; }

        [Parameter]
        public string Type { get; set; } = "text";

        [Parameter]
        public RenderFragment PrependInnerContent { get; set; }

        [Parameter]
        public string AppendOuterIcon { get; set; }

        [Parameter]
        public RenderFragment AppendOuterContent { get; set; }

        [Parameter]
        public string PrependInnerIcon { get; set; }

        [Parameter]
        public string Suffix { get; set; }

        [Parameter]
        public bool Autofocus { get; set; }

        [Parameter]
        public EventCallback<FocusEventArgs> OnBlur { get; set; }

        [Parameter]
        public EventCallback<FocusEventArgs> OnFocus { get; set; }

        [Parameter]
        public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

        [Parameter]
        public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }

        [Parameter]
        public EventCallback<TValue> OnInput { get; set; }

        [Parameter]
        public RenderFragment ProgressContent { get; set; }

        [Parameter]
        public StringNumber LoaderHeight { get; set; } = 2;

        [Parameter]
        public StringNumberBoolean Counter { get; set; }

        [Parameter]
        public Func<TValue, int> CounterValue { get; set; }

        [Parameter]
        public RenderFragment CounterContent { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> OnAppendOuterClick { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> OnPrependInnerClick { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> OnClearClick { get; set; }

        [Parameter]
        public virtual Action<TextFieldNumberProperty> NumberProps { get; set; }

        [Inject]
        public MasaBlazor MasaBlazor { get; set; }

        [Inject]
        public Document Document { get; set; }

        [Inject]
        public DomEventJsInterop DomEventJsInterop { get; set; } = null!;

        public TextFieldNumberProperty Props { get; set; } = new();

        public bool IsSolo => Solo || SoloInverted;

        public bool IsBooted { get; set; } = true;

        public bool IsEnclosed => Filled || IsSolo || Outlined;

        public bool ShowLabel => HasLabel && !(IsSingle && LabelValue);

        public bool IsSingle => IsSolo || SingleLine || FullWidth || (Filled && !HasLabel);

        public virtual bool LabelValue => IsFocused || IsLabelActive || PersistentPlaceholder;

        public virtual ElementReference InputElement { get; set; }

        protected double LabelWidth { get; set; }

        public string LegendInnerHTML => "&#8203;";

        public bool HasCounter => Counter != false && Counter != null;

        public override string ComputedColor
        {
            get
            {
                if (!SoloInverted || !IsFocused)
                {
                    return base.ComputedColor;
                }

                return Color ?? "primary";
            }
        }

        public override bool HasColor => IsFocused;

        public virtual string Tag => "input";

        protected virtual Dictionary<string, object> InputAttrs
        {
            get
            {
                Dictionary<string, object> attibutes = new(Attributes)
                {
                    { "type", Type },
                    { "value", _badInput == null ? InternalValue : _badInput }
                };

                if (Type == "number")
                {
                    if (Props.Min.HasValue)
                        attibutes.Add("min", Props.Min);
                    if (Props.Max.HasValue)
                        attibutes.Add("max", Props.Max);
                    attibutes.Add("step", Props.Step);
                }

                return attibutes;
            }
        }

        public virtual StringNumber ComputedCounterValue
        {
            get
            {
                if (CounterValue != null)
                {
                    return CounterValue(InternalValue);
                }

                return InternalValue?.ToString()?.Length ?? 0;
            }
        }

        public override bool HasDetails => base.HasDetails || HasCounter;

        public StringNumber Max
        {
            get
            {
                int? max = null;

                if (Counter == true)
                {
                    if (Attributes.TryGetValue("maxlength", out var maxValue))
                    {
                        max = Convert.ToInt32(maxValue);
                    }
                }
                else
                {
                    max = Counter?.ToInt32();
                }

                return max == null ? null : (StringNumber)max;
            }
        }

        protected double PrefixWidth { get; set; }

        protected double PrependWidth { get; set; }

        private string NumberValue
        {
            get
            {
                return InternalValue == null || string.IsNullOrWhiteSpace(InternalValue.ToString()) ? "0" : InternalValue.ToString();
            }
        }

        protected (StringNumber left, StringNumber right) LabelPosition
        {
            get
            {
                var offset = (Prefix != null && !LabelValue) ? PrefixWidth : 0;

                if (LabelValue && PrependWidth > 0)
                {
                    offset -= PrependWidth;
                }

                return MasaBlazor.RTL == Reverse ? (offset, "auto") : ("auto", offset);
            }
        }

        public bool UpButtonEnabled
        {
            get
            {
                if (Props.Max == null)
                {
                    return true;
                }

                if (BindConverter.TryConvertToDecimal(NumberValue, System.Globalization.CultureInfo.InvariantCulture, out var value))
                {
                    return value < Props.Max;
                }

                return false;
            }
        }

        public bool DownButtonEnabled
        {
            get
            {
                if (Props.Min == null)
                {
                    return true;
                }

                if (BindConverter.TryConvertToDecimal(NumberValue, System.Globalization.CultureInfo.InvariantCulture, out var value))
                {
                    return value > Props.Min;
                }

                return false;
            }
        }

        public BLabel LabelReference { get; set; }

        public ElementReference PrefixElement { get; set; }

        public ElementReference PrependInnerElement { get; set; }
        
        public ElementReference AppendInnerElement { get; set; }

        protected virtual Dictionary<string, object> InputSlotAttrs { get; set; } = new();

        bool ITextField<TValue>.IsDirty => IsDirty;

        Dictionary<string, object> ITextField<TValue>.InputAttrs => InputAttrs;

        Dictionary<string, object> ITextField<TValue>.InputSlotAttrs => InputSlotAttrs;

        TValue ITextField<TValue>.InputValue => InputValue;

        protected override void SetComponentClass()
        {
            base.SetComponentClass();

            var prefix = "m-text-field";
            CssProvider
                .Merge(cssBuilder =>
                {
                    cssBuilder
                        .Add(prefix)
                        .AddIf($"{prefix}--full-width", () => FullWidth)
                        .AddIf($"{prefix}--prefix", () => Prefix != null)
                        .AddIf($"{prefix}--single-line", () => IsSingle)
                        .AddIf($"{prefix}--solo", () => IsSolo)
                        .AddIf($"{prefix}--solo-inverted", () => SoloInverted)
                        .AddIf($"{prefix}--solo-flat", () => Flat)
                        .AddIf($"{prefix}--filled", () => Filled)
                        .AddIf($"{prefix}--is-booted", () => IsBooted)
                        .AddIf($"{prefix}--enclosed", () => IsEnclosed)
                        .AddIf($"{prefix}--reverse", () => Reverse)
                        .AddIf($"{prefix}--outlined", () => Outlined)
                        .AddIf($"{prefix}--placeholder", () => Placeholder != null)
                        .AddIf($"{prefix}--rounded", () => Rounded)
                        .AddIf($"{prefix}--shaped", () => Shaped);
                })
                .Merge("text-field-details", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-text-field__details");
                })
                .Merge("input-slot", cssBuilder =>
                {
                    cssBuilder
                        .AddBackgroundColor(BackgroundColor);
                })
                .Apply("prepend-inner", cssBuilder =>
                {
                    cssBuilder
                        .Add($"m-input__prepend-inner");
                })
                .Apply("append-inner", cssBuilder =>
                {
                    cssBuilder
                        .Add($"m-input__append-inner");
                })
                .Apply("append-outer-icon", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input__icon")
                        .Add("m-input__icon--append-outter");
                })
                .Apply("prepend-inner-icon", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input__icon")
                        .Add("m-input__icon--prepend-inner");
                })
                .Apply("clear-icon", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input__icon")
                        .Add("m-input__icon--clear");
                })
                .Apply("legend", styleAction: styleBuilder =>
                {
                    var width = (!SingleLine && (LabelValue || IsDirty)) ? LabelWidth : 0;
                    styleBuilder
                        .AddIf(() => $"width:{width}px", () => !IsSingle);
                })
                .Apply("legend-span", cssBuilder =>
                {
                    cssBuilder
                        .Add("notranslate");
                })
                .Apply("text-field-slot", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-text-field__slot");
                })
                .Apply("text-field-input", cssBuilder =>
                {
                    cssBuilder
                        .AddTextColor(TextColor);
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddTextColor(TextColor);
                })
                .Apply("text-field-prefix", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-text-field__prefix");
                })
                .Apply("text-field-suffix", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-text-field__suffix");
                })
                .Apply("number-wrap", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input-number-handler-wrap")
                        .AddIf("m-input-number-handler-focused", () => IsFocused);
                })
                .Apply("append-icon-number-up", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input__icon")
                        .Add("m-input__icon--append")
                        .Add("m-number-input-icon")
                        .AddIf("m-number-input-up-disabled", () => !UpButtonEnabled);

                })
                .Apply("append-icon-number-down", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input__icon")
                        .Add("m-input__icon--append")
                        .Add("m-number-input-icon")
                        .AddIf("m-number-input-down-disabled", () => !DownButtonEnabled);

                });

            AbstractProvider
                .ApplyTextFieldDefault<TValue>()
                .ApplyTextFieldCounter(typeof(MCounter), attrs =>
                {
                    attrs[nameof(MCounter.Dark)] = Dark;
                    attrs[nameof(MCounter.Light)] = Light;
                    attrs[nameof(MCounter.Max)] = Max;
                    attrs[nameof(MCounter.Value)] = ComputedCounterValue;
                })
                .ApplyTextFieldLabel(typeof(MLabel), attrs =>
                {
                    var (left, right) = LabelPosition;

                    attrs[nameof(MLabel.Absolute)] = true;
                    attrs[nameof(MLabel.Focused)] = !IsSingle && (IsFocused || ValidationState != null);
                    attrs[nameof(MLabel.Left)] = left;
                    attrs[nameof(MLabel.Right)] = right;
                    attrs[nameof(MLabel.Value)] = LabelValue;
                })
                .ApplyTextFieldProcessLinear(typeof(MProgressLinear), attrs =>
                {
                    attrs[nameof(MProgressLinear.Absolute)] = true;
                    attrs[nameof(MProgressLinear.Color)] = (Loading == true || Loading == "") ? (Color ?? "primary") : Loading.ToString();
                    attrs[nameof(MProgressLinear.Height)] = LoaderHeight;
                    attrs[nameof(MProgressLinear.Indeterminate)] = true;
                })
                .ApplyTextFieldClearIcon(typeof(MIcon), attrs =>
                {
                    attrs[nameof(MIcon.Color)] = ValidationState;
                    attrs[nameof(MIcon.Dark)] = Dark;
                    attrs[nameof(MIcon.Disabled)] = Disabled;
                    attrs[nameof(MIcon.Light)] = Light;
                    attrs[nameof(MIcon.OnClickStopPropagation)] = true;
                    attrs[nameof(MIcon.OnClickPreventDefault)] = true;
                })
                .ApplyTextFieldAppendOuterIcon(typeof(MIcon), attrs =>
                {
                    attrs[nameof(MIcon.Color)] = ValidationState;
                    attrs[nameof(MIcon.Dark)] = Dark;
                    attrs[nameof(MIcon.Disabled)] = Disabled;
                    attrs[nameof(MIcon.Light)] = Light;
                })
                .ApplyTextFieldPrependInnerIcon(typeof(MIcon), attrs =>
                {
                    attrs[nameof(MIcon.Color)] = ValidationState;
                    attrs[nameof(MIcon.Dark)] = Dark;
                    attrs[nameof(MIcon.Disabled)] = Disabled;
                    attrs[nameof(MIcon.Light)] = Light;
                });
        }

        public override Func<Task> DebounceTimerRun => TextFieldDebounceTimerRun;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            NumberProps?.Invoke(Props);

            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await JsInvokeAsync(JsInteropConstants.RegisterTextFieldOnMouseDown, InputSlotElement, InputElement, DotNetObjectReference.Create(new Invoker<MouseEventArgs>(HandleOnMouseDownAsync)));
                //OnAfterRender doesn't indicate DOM ready
                //So we should wait a little time
                //We may remove this when dialog been refactored
                await Task.Delay(16 * 3);

                var inputElement = Document.GetElementByReference(InputElement);
                await inputElement.AddEventListenerAsync("compositionstart", CreateEventCallback(OnCompositionStart));
                await inputElement.AddEventListenerAsync("compositionend", CreateEventCallback(OnCompositionEnd));

                await DomEventJsInterop.IntersectionObserver(InputElement.GetSelector(), TryAutoFocus);

                var tasks = new Task[3];

                tasks[0] = SetLabelWidthAsync();
                tasks[1] = SetPrefixWidthAsync();
                tasks[2] = SetPrependWidthAsync();

                if (tasks.All(task => task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Canceled))
                {
                    return;
                }

                await Task.WhenAll(tasks);
                StateHasChanged();
            }
        }

        private async Task SetLabelWidthAsync()
        {
            if (!Outlined)
            {
                return;
            }

            //No label
            if (LabelReference == null || LabelReference.Ref.Id == null)
            {
                return;
            }

            var label = Document.GetElementByReference(LabelReference.Ref);
            var scrollWidth = await label.GetScrollWidthAsync();

            if (scrollWidth == null)
            {
                return;
            }

            var element = Document.GetElementByReference(Ref);
            var offsetWidth = await element.GetOffsetWidthAsync();

            if (offsetWidth == null)
            {
                return;
            }

            LabelWidth = Math.Min(scrollWidth.Value * 0.75 + 6, offsetWidth.Value - 24);
        }

        private async Task SetPrefixWidthAsync()
        {
            if (PrefixElement.Id == null)
            {
                return;
            }

            var prefix = Document.GetElementByReference(PrefixElement);
            var offsetWidth = await prefix.GetOffsetWidthAsync();

            if (offsetWidth == null)
            {
                return;
            }

            PrefixWidth = offsetWidth.Value;
        }

        private async Task SetPrependWidthAsync()
        {
            if (!Outlined)
            {
                return;
            }

            if (PrependInnerElement.Id == null)
            {
                return;
            }

            var prependInner = Document.GetElementByReference(PrependInnerElement);
            var offsetWidth = await prependInner.GetOffsetWidthAsync();

            if (offsetWidth == null)
            {
                return;
            }

            PrependWidth = offsetWidth.Value;
        }

        private async Task<bool> TryAutoFocus()
        {
            if (!Autofocus || InputElement.Context is null)
            {
                return false;
            }

            await InputElement.FocusAsync();

            return true;
        }

        public virtual async Task HandleOnAppendOuterClickAsync(MouseEventArgs args)
        {
            if (OnAppendOuterClick.HasDelegate)
            {
                await OnAppendOuterClick.InvokeAsync(args);
            }
        }

        public virtual async Task HandleOnPrependInnerClickAsync(MouseEventArgs args)
        {
            if (OnPrependInnerClick.HasDelegate)
            {
                await OnPrependInnerClick.InvokeAsync(args);
            }
        }

        public override async Task HandleOnClickAsync(ExMouseEventArgs args)
        {
            if (IsFocused || IsDisabled)
            {
                return;
            }

            await InputElement.FocusAsync();
        }

        public virtual async Task HandleOnChangeAsync(ChangeEventArgs args)
        {
            if (OnChange.HasDelegate)
            {
                await OnChange.InvokeAsync(InternalValue);
            }
        }

        public virtual async Task HandleOnBlurAsync(FocusEventArgs args)
        {
            _badInput = null;
            IsFocused = false;

            var checkValue = await CheckNumberValidate();

            if (!EqualityComparer<TValue>.Default.Equals(checkValue, InternalValue))
            {
                InputValue = checkValue;
            }

            await ChangeValue(true);

            if (OnBlur.HasDelegate)
            {
                await OnBlur.InvokeAsync(args);
            }
        }

        public virtual async Task HandleOnInputAsync(ChangeEventArgs args)
        {
            var success = BindConverter.TryConvertTo<TValue>(args.Value.ToString(), System.Globalization.CultureInfo.InvariantCulture, out var val);

            if (success)
            {
                _badInput = null;
                // InputValue = val;
                // await SetInternalValueAsync(val);
                InternalValue = val;

                Console.WriteLine($"HandleOnInputAsync InputValue: {val}");

                if (OnInput.HasDelegate)
                {
                    await OnInput.InvokeAsync(val);
                }
            }
            else
            {
                _badInput = val.ToString();
            }
        }

        private bool _compositionInputting;

        public Task OnCompositionStart()
        {
            _compositionInputting = true;
            return Task.CompletedTask;
        }

        public Task OnCompositionEnd()
        {
            _compositionInputting = false;
            return Task.CompletedTask;
        }

        public async Task TextFieldDebounceTimerRun()
        {
            if (!_compositionInputting)
            {
                Console.WriteLine($"TextFieldDebounceTimerRun SetInternalValueAsync InputValue:{InputValue}");
                
                // await SetInternalValueAsync(InputValue);
                // InternalValue = InputValue;

                StateHasChanged();
            }
        }

        private Task<TValue> CheckNumberValidate()
        {
            if (Type == "number" && BindConverter.TryConvertToDecimal(NumberValue, System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                TValue returnValue;

                if (Props.Min != null && value < Props.Min && BindConverter.TryConvertTo<TValue>(Props.Min.ToString(), System.Globalization.CultureInfo.InvariantCulture, out returnValue))
                    return Task.FromResult(returnValue);
                else if (Props.Max != null && value > Props.Max && BindConverter.TryConvertTo<TValue>(Props.Max.ToString(), System.Globalization.CultureInfo.InvariantCulture, out returnValue))
                    return Task.FromResult(returnValue);
            }

            return Task.FromResult(InternalValue);
        }

        public async Task HandleOnKeyUpAsync(KeyboardEventArgs args)
        {
            if (OnKeyUp.HasDelegate)
                await OnKeyUp.InvokeAsync(args);
        }

        public async Task HandleOnNumberUpClickAsync(MouseEventArgs args)
        {
            if (UpButtonEnabled && BindConverter.TryConvertToDecimal(NumberValue, System.Globalization.CultureInfo.InvariantCulture, out decimal value))
            {
                if (Props.Min != null && value < Props.Min)
                {
                    value = Props.Min.Value;
                }

                value += Props.Step;

                if (Props.Max != null && value > Props.Max)
                {
                    value = Props.Max.Value;
                }

                if (BindConverter.TryConvertTo<TValue>(value.ToString(), System.Globalization.CultureInfo.InvariantCulture, out var internalValue))
                {
                    // await SetInternalValueAsync(internalValue);
                    InternalValue = internalValue;
                }
            }

            await InputElement.FocusAsync();
        }
        public async Task HandleOnNumberDownClickAsync(MouseEventArgs args)
        {
            if (DownButtonEnabled && BindConverter.TryConvertToDecimal(NumberValue, System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                if (Props.Max != null && value > Props.Max)
                {
                    value = Props.Max.Value;
                }

                value -= Props.Step;

                if (Props.Min != null && value < Props.Min)
                {
                    value = Props.Min.Value;
                }

                if (BindConverter.TryConvertTo<TValue>(value.ToString(), System.Globalization.CultureInfo.InvariantCulture, out var internalValue))
                {
                    // await SetInternalValueAsync(internalValue);
                    InternalValue = internalValue;
                }
            }

            await InputElement.FocusAsync();
        }

        protected override bool ShouldRender()
        {
            return _shouldRender;
        }

        public virtual async Task HandleOnFocusAsync(FocusEventArgs args)
        {
            if (!IsFocused)
            {
                IsFocused = true;

                if (OnFocus.HasDelegate)
                {
                    await OnFocus.InvokeAsync(args);
                }
            }
        }

        public virtual async Task HandleOnKeyDownAsync(KeyboardEventArgs args)
        {
            await Task.Yield();

            if (args.Key == "Enter")
            {
                await ChangeValue(true);
            }
            else
            {
                await ChangeValue();
            }

            if (OnKeyDown.HasDelegate)
            {
                await OnKeyDown.InvokeAsync(args);
            }
        }

        public virtual async Task HandleOnClearClickAsync(MouseEventArgs args)
        {
            // await SetInternalValueAsync(default);
            InternalValue = default;
            
            if (OnClearClick.HasDelegate)
            {
                await OnClearClick.InvokeAsync(args);
            }

            await InputElement.FocusAsync();
        }

        public override async Task HandleOnMouseUpAsync(ExMouseEventArgs args)
        {
            if (HasMouseDown)
            {
                await InputElement.FocusAsync();
            }
        
            await base.HandleOnMouseUpAsync(args);
        }
    }
}
