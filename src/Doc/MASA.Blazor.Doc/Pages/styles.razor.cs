﻿using Microsoft.AspNetCore.Components;
using MASA.Blazor.Doc.Models;
using MASA.Blazor.Doc.Utils;
using System.Globalization;

namespace MASA.Blazor.Doc.Pages
{
    public partial class Styles
    {
        private DemoComponentModel _demoComponent;

        private DemoItemModel Usage { get; set; }

        private List<DemoItemModel> PropsList { get; set; }

        private List<DemoItemModel> ContentsList { get; set; }

        private List<DemoItemModel> EventsList { get; set; }

        private List<DemoItemModel> MiscList { get; set; }

        [Parameter]
        public string Name { get; set; }

        [CascadingParameter]
        public bool IsChinese { get; set; }

        [Inject]
        public GlobalConfigs GlobalConfig { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (Name.Contains('?'))
            {
                Name = Name.Split("?")[0];
            }

            if (Name.Contains('#'))
            {
                Name = Name.Split("#")[0];
            }

            Service.ChangeLanguage(GlobalConfig.Language ?? CultureInfo.CurrentCulture.Name);
            _demoComponent = await Service.GetStyleAsync(Name);

            var demos = _demoComponent.DemoList?
                .Where(x => !x.Debug && !x.Docs.HasValue)
                .OrderBy(x => x.Order)
                .ThenBy(r => r.Name) ?? Enumerable.Empty<DemoItemModel>();

            Usage = demos.FirstOrDefault(demo => demo.Group == DemoGroup.Usage);
            PropsList = demos.Where(demo => demo.Group == DemoGroup.Props).ToList();
            EventsList = demos.Where(demo => demo.Group == DemoGroup.Events).ToList();
            ContentsList = demos.Where(demo => demo.Group == DemoGroup.Contents).ToList();
            MiscList = demos.Where(demo => demo.Group == DemoGroup.Misc).ToList();
        }
    }
}