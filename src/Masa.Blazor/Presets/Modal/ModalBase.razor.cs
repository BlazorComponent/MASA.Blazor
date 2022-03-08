﻿using BlazorComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Masa.Blazor.Presets
{
    public partial class ModalBase
    {
        [Inject]
        protected IJSRuntime JsRuntime { get; set; }

        [Parameter]
        public RenderFragment<ActivatorProps> ActivatorContent { get; set; }

        [Parameter]
        public string ActionsClass { get; set; }

        [Parameter]
        public string ActionsStyle { get; set; }

        /// <summary>
        /// Automatically scroll to the top when <see cref="Value"/> is true.
        /// </summary>
        [Parameter]
        public bool AutoScrollToTop { get; set; }

        [Parameter]
        public string BodyClass { get; set; }

        [Parameter]
        public string BodyStyle { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string Class { get; set; }

        [Parameter]
        public string ContentClass { get; set; }

        [Parameter]
        public string ContentStyle { get; set; }

        [Parameter]
        public int DebounceInterval { get; set; } = 100;

        [Parameter]
        public StringNumber Elevation { get; set; }

        [Parameter]
        public object FormModel { get; set; }

        [Parameter]
        public string HeaderClass { get; set; }

        [Parameter]
        public string HeaderStyle { get; set; }

        [Parameter]
        public StringNumber Height { get; set; }

        [Parameter]
        public bool HideCancelAction { get; set; }

        [Parameter]
        public StringNumber MaxWidth { get; set; }

        [Parameter]
        public bool Persistent { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public string Transition { get; set; }

        [Parameter]
        public bool Value { get; set; }

        [Parameter]
        public EventCallback<bool> ValueChanged { get; set; }

        [Parameter]
        public StringNumber Width { get; set; }

        #region ok,cancel,delete

        [Parameter]
        public Action<ModalButtonProps> OkProps { get; set; }

        [Parameter]
        public Action<ModalButtonProps> CancelProps { get; set; }

        [Parameter]
        public Action<ModalButtonProps> DeleteProps { get; set; }

        [Parameter]
        public string OkText { get; set; }

        [Parameter]
        public string CancelText { get; set; }

        [Parameter]
        public string DeleteText { get; set; }

        [Parameter]
        public EventCallback<ModalActionEventArgs> OnOk { get; set; }

        [Parameter]
        public EventCallback<ModalActionEventArgs> OnCancel { get; set; }

        [Parameter]
        public EventCallback<ModalActionEventArgs> OnDelete { get; set; }

        [Parameter]
        public RenderFragment<(Func<MouseEventArgs, Task> click, bool loading)> OkContent { get; set; }

        [Parameter]
        public RenderFragment<(Func<MouseEventArgs, Task> click, bool loading)> DeleteContent { get; set; }

        [Parameter]
        public RenderFragment<(Func<MouseEventArgs, Task> click, bool loading)> CancelContent { get; set; }

        #endregion

        private bool _okLoading;
        private bool _scrolledToTop;
        private Func<MouseEventArgs, Task> _debounceHandleOnOk;

        private MCardText BodyRef { get; set; }

        private bool Loading => _okLoading; // may add the _deleteLoading in the future 

        protected bool HasActions => OnDelete.HasDelegate || OnOk.HasDelegate;

        protected MForm Form { get; set; }

        protected ModalButtonProps ComputedOkButtonProps { get; set; }

        protected ModalButtonProps ComputedCancelButtonProps { get; set; }

        protected ModalButtonProps ComputedDeleteButtonProps { get; set; }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            AutoScrollToTop = true;

            await base.SetParametersAsync(parameters);

            OkText ??= "确定";
            CancelText ??= "取消";
            DeleteText ??= "删除";

            ComputedOkButtonProps = GetDefaultOkButtonProps();
            ComputedCancelButtonProps = GetDefaultCancelButtonProps();
            ComputedDeleteButtonProps = GetDefaultDeleteButtonProps();

            OkProps?.Invoke(ComputedOkButtonProps);
            CancelProps?.Invoke(ComputedCancelButtonProps);
            DeleteProps?.Invoke(ComputedDeleteButtonProps);
        }

        protected override void OnInitialized()
        {
            _debounceHandleOnOk = DebounceEvent<MouseEventArgs>(
                async (_) =>
                {
                    var args = new ModalActionEventArgs();

                    _okLoading = true;
                    await OnOk.InvokeAsync(args);
                    _okLoading = false;

                    if (args.Cancelled) return;

                    if (Form != null)
                    {
                        await Form.ResetAsync();
                    }
                },
                TimeSpan.FromMilliseconds(DebounceInterval));
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Value)
            {
                if (_scrolledToTop)
                {
                    _scrolledToTop = false;
                }
                else
                {
                    await ScrollToTop();
                    _scrolledToTop = true;
                    StateHasChanged();
                }
            }
        }

        protected virtual async Task HandleOnOk(MouseEventArgs args)
        {
            if (Form != null)
            {
                if (Form.EditContext.Validate())
                {
                    await _debounceHandleOnOk(args);
                }
            }
            else
            {
                await _debounceHandleOnOk(args);
            }
        }

        protected virtual async Task HandleOnCancel(MouseEventArgs _)
        {
            if (Form != null)
            {
                await Form.ResetAsync();
            }

            if (OnCancel.HasDelegate)
            {
                var args = new ModalActionEventArgs();
                
                await OnCancel.InvokeAsync(args);
            }
        }

        protected virtual async Task HandleOnDelete(MouseEventArgs _)
        {
            if (OnDelete.HasDelegate)
            {
                var args = new ModalActionEventArgs();

                await OnDelete.InvokeAsync(args);
            }
        }

        protected virtual ModalButtonProps GetDefaultOkButtonProps() => new()
        {
            Color = "primary",
            Text = true,
        };

        protected virtual ModalButtonProps GetDefaultCancelButtonProps() => new()
        {
            Text = true
        };

        protected virtual ModalButtonProps GetDefaultDeleteButtonProps() => new()
        {
            Color = "error",
            Text = true,
        };

        private async Task ScrollToTop()
        {
            if (AutoScrollToTop && BodyRef?.Ref != null)
            {
                await JsRuntime.InvokeVoidAsync(JsInteropConstants.ScrollToPosition, BodyRef.Ref, 0);
            }
        }

        #region form

        private Func<T, Task> DebounceEvent<T>(Func<T, Task> action, TimeSpan interval)
        {
            return Debounce<T>(async arg =>
            {
                await InvokeAsync(async () =>
                {
                    await action(arg);
                    StateHasChanged();
                });
            }, interval);
        }

        private Func<T, Task> Debounce<T>(Func<T, Task> action, TimeSpan interval)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var last = 0;
            return async arg =>
            {
                var current = Interlocked.Increment(ref last);

                await Task.Delay(interval);

                if (current == last) await action(arg);
            };
        }

        #endregion
    }
}