﻿using BlazorComponent;
using BlazorComponent.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MASA.Blazor.Doc.Services;
using MASA.Blazor.Doc.Utils;

namespace MASA.Blazor.Doc.Shared
{
    public partial class BaseLayout : IDisposable
    {
        private string _searchBorderColor = "#00000000";
        private string _languageIcon;
        private bool _isShowMiniLogo = true;
        private StringNumber _selectTab = 0;

        public StringNumber SelectTab
        {
            get
            {
                var url = Navigation.Uri;
                if (url == Navigation.BaseUri)
                {
                    return 0;
                }
                else if (url.Contains("about/meet-the-team"))
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
            set
            {
                if (value != null && value.AsT1 != 3 && value.AsT1 != 4)
                {
                    _selectTab = value;
                }
            }
        }

        [Inject]
        public I18n I18n { get; set; }

        [Inject]
        public NavigationManager Navigation { get; set; }

        [Inject]
        public GlobalConfigs GlobalConfig { get; set; }

        public bool IsChinese { get; set; }

        public bool Drawer { get; set; } = true;

        public bool Temporary { get; set; } = true;

        public void UpdateNav(bool drawer, bool temporary = true)
        {
            Drawer = drawer;
            Temporary = temporary;
        }

        private void TurnLanguage()
        {
            IsChinese = !IsChinese;
            var lang = IsChinese ? "zh-CN" : "en-US";

            ChangeLanguage(lang);

            GlobalConfig.Language = lang;
            GlobalConfig.SaveChanges();
        }

        private void ChangeLanguage(string lang)
        {
            _languageIcon = $"{lang}.png";
            I18n.SetLang(lang);
        }

        protected override void OnInitialized()
        {
            string lang = GlobalConfig.Language ?? CultureInfo.CurrentCulture.Name;
            if (GlobalConfig.Language != null)
                lang = GlobalConfig.Language;
            else if (GlobalConfigs.StaticLanguage is not null)
                lang = GlobalConfigs.StaticLanguage;
            else
                lang = CultureInfo.CurrentCulture.Name;


            IsChinese = lang == "zh-CN";

            ChangeLanguage(lang);

            Navigation.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            var isShowMiniLogo = _isShowMiniLogo;

            if (e.Location == Navigation.BaseUri)
                _isShowMiniLogo = true;
            else
                _isShowMiniLogo = false;

            var selectTab = SelectTab;
            if (e.Location.Contains("meet-the-team"))
                SelectTab = 2;
            else if (e.Location != Navigation.BaseUri)
                SelectTab = 1;

            if (isShowMiniLogo != _isShowMiniLogo || selectTab != _selectTab)
            {
                _ = InvokeAsync(StateHasChanged);
            }
        }

        private void ShowDraw()
        {
            UpdateNav(true);
        }

        public string T(string key)
        {
            return I18n.LanguageMap.GetValueOrDefault(key);
        }

        public void Dispose()
        {
            Navigation.LocationChanged -= OnLocationChanged;
        }
    }
}
