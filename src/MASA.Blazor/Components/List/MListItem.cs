using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlazorComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace MASA.Blazor
{
    public partial class MListItem : BListItem, IThemeable
    {
        /// <summary>
        /// Lowers max height of list tiles
        /// </summary>
        [Parameter]
        public bool Dense { get; set; }

        /// <summary>
        /// If set, the list tile will not be rendered as a link even if it has to/href prop or @click handler
        /// </summary>
        [Parameter]
        public bool Inactive { get; set; }

        /// <summary>
        /// Allow text selection inside v-list-item. This prop uses user-select
        /// </summary>
        [Parameter]
        public bool Selectable { get; set; }

        /// <summary>
        /// Increases list-item height for two lines. This prop uses line-clamp and is not supported in all browsers.
        /// </summary>
        [Parameter]
        public bool TwoLine { get; set; }

        /// <summary>
        /// Increases list-item height for three lines. This prop uses line-clamp and is not supported in all browsers.
        /// </summary>
        [Parameter]
        public bool ThreeLine { get; set; }

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
        public bool Highlighted { get; set; }

        [Parameter]
        public bool Ripple { get; set; }
        
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            Attributes["ripple"] = IsClickable || Ripple;
        }

        protected override void SetComponentClass()
        {
            var prefix = "m-list-item";
            CssProvider
                .Apply(cssBuilder =>
                {
                    cssBuilder
                        .Add("m-list-item")
                        .AddIf($"{prefix}--dense", () => Dense)
                        .AddIf($"{prefix}--disabled", () => Disabled)
                        .AddIf($"{prefix}--selectable", () => Selectable)
                        .AddIf($"{prefix}--two-line", () => TwoLine)
                        .AddIf($"{prefix}--three-line", () => ThreeLine)
                        .AddIf($"{prefix}--link", () => IsClickable && !Inactive)
                        .AddIf($"{prefix}--active {ComputedActiveClass}", () =>
                        {
                            if (IsActive) return true;

                            if (!Link) return false;

                            if (Value == null) return false;

                            if (ItemGroup == null) return false;

                            if (ItemGroup.Multiple) return ItemGroup.Values.Contains(Value);

                            return ItemGroup.Value == Value;
                        })
                        .AddIf("m-list-item--highlighted", () => Highlighted)
                        .AddTextColor(Color)
                        .AddTheme(IsDark);
                });
        }
    }
}