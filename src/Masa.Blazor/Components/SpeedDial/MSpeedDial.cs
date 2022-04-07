﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.Blazor
{
    public class MSpeedDial<TButton> : BSpeedDial<TButton> where TButton : BButton
    {
        protected override void SetComponentClass()
        {
            var prefix = "m-speed-dial";

            CssProvider
              .Apply(cssBuilder =>
              {
                  cssBuilder.Add(prefix);
                  cssBuilder.AddIf($"{prefix}--top", () => Top);
                  cssBuilder.AddIf($"{prefix}--right", () => Right);
                  cssBuilder.AddIf($"{prefix}--bottom", () => Bottom);
                  cssBuilder.AddIf($"{prefix}--left", () => Left);
                  cssBuilder.AddIf($"{prefix}--absolute", () => Absolute);
                  cssBuilder.AddIf($"{prefix}--fixed", () => Fixed);
                  cssBuilder.Add($"{prefix}--direction-{Direction}");
                  cssBuilder.AddIf($"{prefix}--is-active", () => Value);
              })
              .Apply("dial-list", cssBuilder =>
              {
                  cssBuilder.Add($"{prefix}__list");
              });

            for (int i = 0; i < Buttons.Count; i++)
            {
                CssProvider.Apply($"dial-{i}", styleAction: styleBuilder =>
                {
                    styleBuilder.Add($"transitionDelay: {(i * 0.05)}s");
                });
            }
        }

    }
}
