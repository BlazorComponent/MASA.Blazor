﻿using BlazorComponent;
using BlazorComponent.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Element = BlazorComponent.Web.Element;

namespace Masa.Blazor
{
    public partial class MDialog : BDialog, IDialog, IThemeable
    {
        private bool _animated = false;

        protected override string AttachSelector => Attach ?? ".m-application";

        [Parameter]
        public string ContentClass { get; set; }

        [Parameter]
        public string Origin { get; set; }

        [Parameter]
        public bool Scrollable { get; set; }

        [Parameter]
        public string Transition { get; set; }

        public Dictionary<string, object> ContentAttrs
        {
            get
            {
                var attrs = new Dictionary<string, object>
                {
                    { "role", "document" }
                };
                if (Value)
                {
                    attrs.Add("tabindex", 0);
                }

                return attrs;
            }
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            Origin ??= "center center";
            Transition ??= "dialog-transition";
        }

        protected override void SetComponentClass()
        {
            var prefix = "m-dialog";

            CssProvider
                .Apply(cssBuilder =>
                {
                    cssBuilder
                        .Add($"{prefix}__container")
                        .AddIf($"{prefix}__container--attached", () => Attach != null);
                })
                .Apply("content", cssBuilder =>
                {
                    cssBuilder
                        .Add($"{prefix}__content")
                        .AddIf($"{prefix}__content--active", () => Value)
                        .AddTheme(IsDark);
                }, styleBuilder =>
                {
                    styleBuilder
                        .Add($"z-index: {ZIndex}");
                })
                .Apply("innerContent", cssBuilder =>
                {
                    cssBuilder
                        .Add(prefix)
                        .Add(ContentClass)
                        .AddIf($"{prefix}--active", () => Value)
                        .AddIf($"{prefix}--persistent", () => Persistent)
                        .AddIf($"{prefix}--fullscreen", () => Fullscreen)
                        .AddIf($"{prefix}--scrollable", () => Scrollable)
                        .AddIf($"{prefix}--animated", () => _animated);
                }, styleBuilder =>
                {
                    styleBuilder
                        .Add($"transform-origin: {Origin}")
                        .AddWidth(Width)
                        .AddMaxWidth(MaxWidth);
                });

            AbstractProvider
                .Apply<BOverlay, MOverlay>(attrs =>
                {
                    attrs[nameof(MOverlay.Value)] = ShowOverlay && Value;
                    attrs[nameof(MOverlay.ZIndex)] = ZIndex - 1;
                })
                .ApplyDialogDefault();
        }

        private async Task AfterShowContent()
        {
            await JsInvokeAsync(JsInteropConstants.AddOutsideClickEventListener,
                DotNetObjectReference.Create(new Invoker<object>(OutsideClick)),
                new[] { Document.GetElementByReference(DialogRef).Selector },
                new[] { Document.GetElementByReference(OverlayRef!.Value).Selector });
        }

        private async Task AnimateClick()
        {
            _animated = true;
            await InvokeStateHasChangedAsync();

            await Task.Delay(150);

            _animated = false;
            await InvokeStateHasChangedAsync();
        }

        public async Task Keydown(KeyboardEventArgs args)
        {
            if (args.Key == "Escape")
            {
                await Close();
            }
        }

        protected override async Task Close()
        {
            if (Persistent)
            {
                await AnimateClick();
                return;
            }

            await base.Close();
        }

        private bool CloseConditional()
        {
            return IsActive;
        }

        protected async Task OutsideClick(object _)
        {
            if (!CloseConditional()) return;

            if (OnOutsideClick.HasDelegate)
                await OnOutsideClick.InvokeAsync();

            if (Persistent)
            {
                await AnimateClick();
                return;
            }

            await UpdateValue(false);

            await InvokeStateHasChangedAsync();
        }

        protected override async Task ShowLazyContent()
        {
            if (!ShowContent && Value)
            {
                ShowContent = true;
                Value = false;

                await InvokeStateHasChangedAsync();
                await Task.Delay(BROWSER_RENDER_INTERVAL);

                await AfterShowContent();
                Value = true;

                await MoveContentTo();
                await InvokeStateHasChangedAsync();
            }
        }

        private async Task MoveContentTo()
        {
            await JsInvokeAsync(JsInteropConstants.AddElementTo, OverlayRef, AttachSelector);
            await JsInvokeAsync(JsInteropConstants.AddElementTo, ContentRef, AttachSelector);
        }
    }
}