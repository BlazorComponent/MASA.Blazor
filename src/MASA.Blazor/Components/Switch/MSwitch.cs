﻿using BlazorComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MASA.Blazor
{
    public partial class MSwitch : MInput<bool>, ISwitch
    {
        [Parameter]
        public bool Flat { get; set; }

        [Parameter]
        public bool Inset { get; set; }

        [Parameter]
        public EventCallback<bool> OnChange { get; set; }

        [Parameter]
        public string LeftText { get; set; }

        [Parameter]
        public string RightText { get; set; }

        [Parameter]
        public string TrackColor { get; set; }

        public override bool IsDirty => Value;

        public Dictionary<string, object> InputAttrs { get; set; } = new();

        public bool IsActive { get; set; }

        public bool? Ripple { get; set; }

        public override bool HasColor => Value;

        public bool HasText => LeftText != null || RightText != null;

        public string TextColor => HasText ? ComputedColor : (IsLoading ? null : ValidationState);

        public Task HandleOnBlur(FocusEventArgs args)
        {
            return Task.CompletedTask;
        }

        public Task HandleOnChange(ChangeEventArgs args)
        {
            return Task.CompletedTask;
        }

        public Task HandleOnFocus(FocusEventArgs args)
        {
            return Task.CompletedTask;
        }

        public Task HandleOnKeyDown(KeyboardEventArgs args)
        {
            return Task.CompletedTask;
        }

        protected override void SetComponentClass()
        {
            base.SetComponentClass();

            var prefix = "m-input";
            CssProvider
                .Merge(cssBuilder =>
                {
                    cssBuilder
                        .Add($"{prefix}--selection-controls")
                        .Add($"{prefix}--switch")
                        .AddIf($"{prefix}--switch--flat", () => Flat)
                        .AddIf($"{prefix}--switch--inset", () => Inset)
                        .AddIf($"{prefix}--switch--text", () => HasText);
                })
                .Apply("switch", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input--selection-controls__input");
                })
                .Apply("ripple", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input--selection-controls__ripple")
                        .AddTextColor(ValidationState);
                })
                .Apply("track", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input--switch__track")
                        .AddTextColor(TrackColor ?? TextColor)
                        .AddTheme(IsDark);
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddTextColor(TrackColor ?? TextColor);
                })
                .Apply("thumb", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input--switch__thumb")
                        .AddTextColor(TextColor)
                        .AddTheme(IsDark);
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddTextColor(TrackColor ?? TextColor);
                })
                .Apply("left", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input--switch__left");
                })
                .Apply("right", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input--switch__right");
                });

            AbstractProvider
                .Merge(typeof(BInputDefaultSlot<,>), typeof(BSwitchDefaultSlot))
                .Apply(typeof(BSwitchSwitch<>), typeof(BSwitchSwitch<MSwitch>))
                .Apply(typeof(BSelectableInput<>), typeof(BSelectableInput<MSwitch>))
                .Apply(typeof(BRippleableRipple<>), typeof(BRippleableRipple<MSwitch>))
                .Apply<BProgressCircular, MProgressCircular>(attrs =>
                {
                    if (!IsLoading) return;

                    string color = null;

                    Loading.Match(
                        s => color = s,
                        b => b ? color = Color ?? "primary" : null
                    );

                    if (color != null && string.IsNullOrWhiteSpace(color))
                    {
                        color = Color ?? "primary";
                    }

                    attrs[nameof(MProgressCircular.Color)] = color;
                    attrs[nameof(MProgressCircular.Indeterminate)] = true;
                    attrs[nameof(MProgressCircular.Size)] = (StringNumber)16;
                    attrs[nameof(MProgressCircular.Width)] = (StringNumber)2;
                })
                .Apply(typeof(BSwitchProgress<>), typeof(BSwitchProgress<MSwitch>));
        }

        public override async Task HandleOnClickAsync(MouseEventArgs args)
        {
            Value = !Value;
            if (OnChange.HasDelegate)
            {
                await OnChange.InvokeAsync(Value);
            }
            else
            {
                if (ValueChanged.HasDelegate)
                {
                    await ValueChanged.InvokeAsync(Value);
                }
            }
        }
    }
}