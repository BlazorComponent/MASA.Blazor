﻿using BlazorComponent;
using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MASA.Blazor
{
    public partial class MSystemBar : BSystemBar, IThemeable
    {
        [Parameter]
        public string Color { get; set; }

        [Parameter]
        public bool Dark { get; set; }

        [Parameter]
        public bool Light { get; set; }

        [CascadingParameter]
        public IThemeable Themeable { get; set; }

        public bool IsDark => Dark ?
            true :
            (Light ? false : Themeable != null && Themeable.IsDark);

        [Parameter]
        public StringNumber Height { get; set; }

        [Parameter]
        public bool LightsOut { get; set; }

        [Parameter]
        public bool Window { get; set; }

        [Parameter]
        public bool Absolute { get; set; }

        [Parameter]
        public bool App { get; set; }

        [Parameter]
        public bool Fixed { get; set; }

        [Inject]
        public MasaBlazor MasaBlazor { get; set; }

        private StringNumber ComputedHeight => Height != null ?
            (Regex.IsMatch(Height.ToString(), "^[0-9]*$") ? Height.ToInt32() : Height) :
            (Window ? 32 : 24);

        protected override void SetComponentClass()
        {
            CssProvider
                .Apply(cssBuilder =>
                {
                    cssBuilder
                        .Add("m-system-bar")
                        .AddTheme(IsDark)
                        .AddBackgroundColor(Color)
                        .AddIf("m-system-bar--lights-out", () => LightsOut)
                        .AddIf("m-system-bar--absolute", () => Absolute)
                        .AddIf("m-system-bar--fixed", () => !Absolute && (App || Fixed))
                        .AddIf("m-system-bar--window", () => Window);
                }, styleBuilder =>
                {
                    styleBuilder
                        .Add($"height:{ComputedHeight.ToUnit()}");
                });
        }

        protected override async Task OnParametersSetAsync()
        {
            await UpdateApplicationAsync();
        }

        protected async Task UpdateApplicationAsync()
        {
            if (!App)
            {
                return;
            }

            if (Ref.Id == null)
            {
                return;
            }

            var element = await JsInvokeAsync<BlazorComponent.Web.Element>(JsInteropConstants.GetDomInfo, Ref);
            MasaBlazor.Application.Bar = element != null ?
                element.ClientHeight : ComputedHeight.ToDouble();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            RemoveApplication();
        }

        private void RemoveApplication()
        {
            if (!App)
            {
                return;
            }

            MasaBlazor.Application.Bar = 0;
        }
    }
}
