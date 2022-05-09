﻿using Masa.Blazor.Popup.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Masa.Blazor.Presets
{
    public partial class PToastItem: AlertingPopupComponentBase  
    {
        [Parameter]
        public EventCallback<ToastConfig> OnClose { get; set; }

        [Parameter]
        public ToastConfig Config { get; set; }

        [CascadingParameter(Name = "IsDark")]
        public bool CascadingIsDark { get; set; }

        public bool IsDark
        {
            get
            {
                if (Config.Dark)
                {
                    return true;
                }

                if (Config.Light)
                {
                    return false;
                }

                return CascadingIsDark;
            }
        }

        public async Task HandleOnClose()
        {
            if (OnClose.HasDelegate)
                await OnClose.InvokeAsync(Config);
        }

        protected override Task OnInitializedAsync()
        {
            this.Type = Config?.Type;
            return base.OnInitializedAsync();
        }
    }
}
