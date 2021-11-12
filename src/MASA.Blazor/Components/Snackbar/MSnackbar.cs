﻿using BlazorComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MASA.Blazor
{
    public partial class MSnackbar : BSnackbar, IThemeable, ISnackbar
    {
        private bool _value;

        [Parameter]
        public bool Absolute { get; set; }

        [Parameter]
        public bool Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                if (_value && Timeout > 0)
                {
                    if (Timer == null)
                    {
                        Timer = new Timer(Timeout);
                        Timer.Elapsed += Timer_Elapsed;
                    }

                    Timer.Enabled = true;
                }
            }
        }

        [Parameter]
        public EventCallback<bool> IsActiveChanged { get; set; }

        [Parameter]
        public EventCallback OnClosed { get; set; }

        [Parameter]
        public bool Bottom { get; set; }

        [Parameter]
        public bool Centered { get; set; }

        [Parameter]
        public bool Outlined { get; set; }

        [Parameter]
        public bool Left { get; set; }

        [Parameter]
        public bool MultiLine { get; set; }

        [Parameter]
        public bool Right { get; set; }

        [Parameter]
        public bool Text { get; set; }

        [Parameter]
        public bool Top { get; set; }

        [Parameter]
        public bool Vertical { get; set; }

        [Parameter]
        public bool Shaped { get; set; }

        [Parameter]
        public bool Dark { get; set; }

        [Parameter]
        public bool Light { get; set; }

        [CascadingParameter]
        public IThemeable Themeable { get; set; }

        public bool IsDark
        {
            get
            {
                if (Dark)
                {
                    return true;
                }

                if (Light)
                {
                    return false;
                }

                return Themeable != null && Themeable.IsDark;
            }
        }

        [Parameter]
        public int Timeout { get; set; } = 5000;

        [Parameter]
        public string Color { get; set; }

        [Parameter]
        public StringNumber Elevation { get; set; }

        [Parameter]
        public bool Tile { get; set; }

        [Parameter]
        public StringBoolean Rounded { get; set; }

        protected Timer Timer { get; set; }

        [Parameter]
        public string Action { get; set; }

        [Parameter]
        public RenderFragment ActionContent { get; set; }

        protected override void SetComponentClass()
        {
            var prefix = "m-snack";
            CssProvider
                .Apply(cssBuilder =>
                {
                    cssBuilder
                        .Add(prefix)
                        .AddIf($"{prefix}--absolute", () => Absolute)
                        .AddIf($"{prefix}--active", () => Value)
                        .AddIf($"{prefix}--bottom", () => Bottom || !Top)
                        .AddIf($"{prefix}--centered", () => Centered)
                        .AddIf($"{prefix}--has-background", () => !Text && !Outlined)
                        .AddIf($"{prefix}--left", () => Left)
                        .AddIf($"{prefix}--multi-line", () => MultiLine && !Vertical)
                        .AddIf($"{prefix}--right", () => Right)
                        .AddIf($"{prefix}--text", () => Text)
                        .AddIf($"{prefix}--top", () => Top)
                        .AddIf($"{prefix}--vertical", () => Vertical);
                }, styleBuilder =>
                {
                    styleBuilder
                        .Add("padding-bottom: 0px")
                        .Add("padding-top: 64px");
                })
                .Apply("wrap", cssBuilder =>
                {
                    cssBuilder
                        .Add($"{prefix}__wrapper")
                        .Add("m-sheet")
                        .AddIf("m-sheet--outlined", () => Outlined)
                        .AddIf("m-sheet--shaped", () => Shaped)
                        .AddBackgroundColor(Color, () => !Text && !Outlined)
                        .AddTextColor(Color, () => Text || Outlined)
                        .AddRounded(Rounded, Tile)
                        .AddElevation(Elevation)
                        .AddTheme(IsDark);
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddIf("display:none", () => !Value);
                })
                .Apply("content", cssBuilder =>
                {
                    cssBuilder
                        .Add($"{prefix}__content");
                })
                .Apply("action", cssBuilder =>
                {
                    cssBuilder
                        .Add($"{prefix}__action");
                });

            AbstractProvider
                .ApplySnackbarDefault()
                .Apply<BButton, MButton>(props =>
                {
                    props[nameof(Class)] = "m-snack__btn";
                    props[nameof(MButton.Text)] = true;
                    props[nameof(MButton.OnClick)] = EventCallback.Factory.Create<MouseEventArgs>(this, async () =>
                    {
                        Value = false;
                        Timer.Stop();
                        if (IsActiveChanged.HasDelegate)
                        {
                            await IsActiveChanged.InvokeAsync(_value);
                        }
                        if (OnClosed.HasDelegate)
                        {
                            await OnClosed.InvokeAsync();
                        }
                    });
                });
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Value = false;
            if (IsActiveChanged.HasDelegate)
            {
                InvokeAsync(() => IsActiveChanged.InvokeAsync(_value));
            }
            if (OnClosed.HasDelegate)
            {
                InvokeAsync(() => OnClosed.InvokeAsync());
            }

            Timer.Enabled = false;
            InvokeStateHasChanged();
        }
    }
}
