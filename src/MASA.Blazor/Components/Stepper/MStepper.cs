﻿using BlazorComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MASA.Blazor
{
    public partial class MStepper : MSheet
    {
        protected bool IsBooted { get; set; } = true;

        [Parameter]
        public bool Flat { get; set; }

        [Parameter]
        public bool Vertical { get; set; }

        [Parameter]
        public bool AltLabels { get; set; }

        [Parameter]
        public bool NonLinear { get; set; }

        [Parameter]
        public StringNumber Value { get; set; } = 1;

        protected override void SetComponentClass()
        {
            base.SetComponentClass();

            CssProvider
                .Merge(cssBuilder =>
                {
                    cssBuilder
                        .Add("m-stepper")
                        .AddIf("m-stepper--flat", () => Flat)
                        .AddIf("m-stepper--is-booted", () => IsBooted)
                        .AddIf("m-stepper--vertical", () => Vertical)
                        .AddIf("m-stepper--alt-labels", () => AltLabels)
                        .AddIf("m-stepper--non-linear", () => NonLinear);
                });
        }
    }
}
