﻿using MASA.Blazor.Doc.Highlight;
using MASA.Blazor.Doc.Models;
using MASA.Blazor.Doc.Shared;
using MASA.Blazor.Doc.Utils;
using Microsoft.AspNetCore.Components;

namespace MASA.Blazor.Doc.Pages
{
    public partial class Docs
    {
        private string _previousPath;

        private string Path => $"{Category}/{FileName}.{GlobalConfig.Language ?? System.Globalization.CultureInfo.CurrentCulture.Name}";

        private DocFileModel File { get; set; }

        [CascadingParameter]
        public bool IsChinese { get; set; }

        [Inject]
        public GlobalConfigs GlobalConfig { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private IPrismHighlighter PrismHighlighter { get; set; }

        [CascadingParameter]
        public MainLayout MainLayout { get; set; }

        [Parameter]
        public string Category { get; set; }

        [Parameter]
        public string FileName { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                var menus = await Service.GetMenuAsync();
                var current = menus.FirstOrDefault(x => x.Url == Category);
                if (current == null) throw new Exception("No page matched.");

                NavigationManager.NavigateTo(current.Children[0].Url);
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (string.IsNullOrWhiteSpace(FileName)) return;

            if (_previousPath == Path) return;
            _previousPath = Path;

            File = await Service.GetDocFileAsync($"_content/MASA.Blazor.Doc/docs/{Path}.json");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await PrismHighlighter.HighlightAllAsync();
        }
    }
}