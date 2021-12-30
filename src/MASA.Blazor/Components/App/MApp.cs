﻿using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorComponent;
using Microsoft.AspNetCore.Components;

namespace MASA.Blazor
{
    /// <summary>
    /// Root for application
    /// </summary>
    public partial class MApp : BApp, IThemeable
    {
        /// <summary>
        /// Whether to display from left to right
        /// </summary>
        [Parameter]
        public bool LeftToRight { get; set; } = true;

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

        [Inject]
        public HeadJsInterop HeadJsInterop { get; set; }

        [Inject]
        public MasaBlazor MasaBlazor { get; set; }

        protected ThemeCssBuilder ThemeCssBuilder { get; } = new ThemeCssBuilder();

        protected override Task OnInitializedAsync()
        {
            if (Variables.Theme != null)
                HeadJsInterop.InsertAdjacentHTML("beforeend", ThemeCssBuilder.Build());

            return base.OnInitializedAsync();
        }

        protected override void SetComponentClass()
        {
            var prefix = "m-application";

            CssProvider
                .Apply(cssBuilder =>
                {
                    cssBuilder
                        .Add("m-application")
                        .Add(() =>
                        {
                            var suffix = LeftToRight ? "ltr" : "rtl";
                            return $"{prefix}--is-{suffix}";
                        })
                        .AddTheme(IsDark);
                })
                .Apply("wrap", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-application--wrap");
                });

            Attributes.Add("data-app", true);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await MasaBlazor.Breakpoint.InitAsync();
            }
        }
    }
}
