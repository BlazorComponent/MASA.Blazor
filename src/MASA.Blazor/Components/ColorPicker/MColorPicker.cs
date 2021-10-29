﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BlazorComponent;
using BlazorComponent.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MASA.Blazor
{
    public partial class MColorPicker : BColorPicker, IColorPicker
    {
        private ColorPickerColor _internalValue = ColorUtils.FromRGBA(new RGBA { R = 255, G = 0, B = 0, A = 1 });

        [Parameter]
        public bool Flat { get; set; }

        [Parameter]
        public StringNumber Elevation { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public StringNumber Width { get; set; } = 300;

        [Parameter]
        public StringNumber CanvasHeight { get; set; } = 150;

        [Parameter]
        public StringNumber DotSize { get; set; } = 10;

        [Parameter]
        public ColorTypes Type { get; set; }

        [Parameter]
        public RGBA Value { get; set; }

        protected override void SetComponentClass()
        {
            var prefix = "m-color-picker";

            CssProvider
                .Apply(cssBuilder =>
                {
                    cssBuilder
                        .Add(prefix)
                        .AddIf($"{prefix}--flat", () => Flat);
                });

            AbstractProvider
                .Apply<BSheet, MSheet>(props =>
                {
                    props[nameof(MSheet.Elevation)] = Elevation;
                    props[nameof(MSheet.MaxWidth)] = Width;
                })
                .Apply<BColorPickerCanvas, MColorPickerCanvas>(props =>
                {
                    props[nameof(MColorPickerCanvas.Color)] = _internalValue;
                    props[nameof(MColorPickerCanvas.Disabled)] = Disabled;
                    props[nameof(MColorPickerCanvas.DotSize)] = DotSize;
                    props[nameof(MColorPickerCanvas.Width)] = Width;
                    props[nameof(MColorPickerCanvas.Height)] = CanvasHeight;
                });
        }

        public void UpdateColor(ColorPickerColor color)
        {
            _internalValue = color;

            var hsva = color.Hsva;
            if (!Compare(hsva, Value))
            {
                //this.$emit('input', value)
                UpdateColor(_internalValue);
            }

            //switch (Type)
            //{
            //    case ColorTypes.HEX:
            //        var hex = (Value as string).Length == 7 ? color.Hex : color.Hexa;
            //        if (!Compare(hex, Value))
            //        {
            //            //this.$emit('input', value)
            //            UpdateColor(_internalValue);
            //        }
            //        break;
            //    case ColorTypes.RGB:
            //        var rgb = new RGB { R = color.Rgba.R, G = color.Rgba.G, B = color.Rgba.B };
            //        if (!Compare(rgb, Value))
            //        {
            //            //this.$emit('input', value)
            //            UpdateColor(_internalValue);
            //        }
            //        break;
            //    case ColorTypes.RGBA:
            //        var rgba = color.Rgba;
            //        if (!Compare(rgba, Value))
            //        {
            //            //this.$emit('input', value)
            //            UpdateColor(_internalValue);
            //        }
            //        break;
            //    case ColorTypes.HSL:
            //        var hsl = new HSL { H = color.Hsla.H, L = color.Hsla.L, S = color.Hsla.S };
            //        if (!Compare(hsl, Value))
            //        {
            //            //this.$emit('input', value)
            //            UpdateColor(_internalValue);
            //        }
            //        break;
            //    case ColorTypes.HSLA:
            //        var hsla = color.Hsla;
            //        if (!Compare(hsla, Value))
            //        {
            //            //this.$emit('input', value)
            //            UpdateColor(_internalValue);
            //        }
            //        break;
            //    case ColorTypes.HSV:
            //        var hsv = new HSV { H = color.Hsva.H, S = color.Hsva.S, V = color.Hsva.V };
            //        if (!Compare(hsv, Value))
            //        {
            //            //this.$emit('input', value)
            //            UpdateColor(_internalValue);
            //        }
            //        break;
            //    case ColorTypes.HSVA:
            //        var hsva= color.Hsva;
            //        if (!Compare(hsva, Value))
            //        {
            //            //this.$emit('input', value)
            //            UpdateColor(_internalValue);
            //        }
            //        break;
            //}
        }

        public static bool Compare(object obj1, object obj2)
        {
            if (obj1 == null || obj2 == null)
            {
                return false;
            }
            if (!obj1.GetType().Equals(obj2.GetType()))
            {
                return false;
            }

            Type type = obj1.GetType();
            if (type.IsPrimitive || typeof(string).Equals(type))
            {
                return obj1.Equals(obj2);
            }
            if (type.IsArray)
            {
                Array first = obj1 as Array;
                Array second = obj2 as Array;
                var en = first.GetEnumerator();
                int i = 0;
                while (en.MoveNext())
                {
                    if (!Compare(en.Current, second.GetValue(i)))
                        return false;
                    i++;
                }
            }
            else
            {
                foreach (PropertyInfo pi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                {
                    var val = pi.GetValue(obj1);
                    var tval = pi.GetValue(obj2);
                    if (!Compare(val, tval))
                        return false;
                }
                foreach (FieldInfo fi in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                {
                    var val = fi.GetValue(obj1);
                    var tval = fi.GetValue(obj2);
                    if (!Compare(val, tval))
                        return false;
                }
            }
            return true;
        }
    }
}
