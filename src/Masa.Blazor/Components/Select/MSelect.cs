﻿using System.Reflection.Metadata;
using BlazorComponent.Web;
using Microsoft.AspNetCore.Components.Web;

namespace Masa.Blazor
{
    public class MSelect<TItem, TItemValue, TValue> : MTextField<TValue>, ISelect<TItem, TItemValue, TValue>
    {
        [Parameter]
        public override string AppendIcon { get; set; } = "mdi-menu-down";

        //TODO:Attach,CacheItems

        [Parameter]
        public bool Chips { get; set; }

        [Parameter]
        public bool DeletableChips { get; set; }

        //TODO: DisableLookup,Eager

        [Parameter]
        public bool HideSelected { get; set; }

        [EditorRequired]
        [Parameter]
        public IList<TItem> Items { get; set; } = new List<TItem>();

        [Parameter]
        public string ItemColor { get; set; } = "primary";

        [Parameter]
        public Func<TItem, bool> ItemDisabled { get; set; } = item => false;

        [EditorRequired]
        [Parameter]
        public Func<TItem, string> ItemText { get; set; }

        [EditorRequired]
        [Parameter]
        public Func<TItem, TItemValue> ItemValue { get; set; }

        [Parameter]
        public Action<BMenuProps> MenuProps { get; set; }

        [Parameter]
        public bool Multiple
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        //TODO:OpenOnClear

        [Parameter]
        public bool SmallChips { get; set; }

        //TODO:remove this
        [Parameter]
        public StringNumber MinWidth { get; set; }

        //Filterable
        [Parameter]
        public string NoDataText { get; set; } = "No data available";

        [Parameter]
        public EventCallback<TItem> OnSelectedItemUpdate { get; set; }

        [Parameter]
        public RenderFragment AppendItemContent { get; set; }

        [Parameter]
        public RenderFragment<SelectListItemProps<TItem>> ItemContent { get; set; }

        [Parameter]
        public RenderFragment NoDataContent { get; set; }

        [Parameter]
        public RenderFragment PrependItemContent { get; set; }

        [Parameter]
        public RenderFragment<SelectSelectionProps<TItem>> SelectionContent { get; set; }

        bool ISelect<TItem, TItemValue, TValue>.HasChips => HasChips;

        IList<TItem> ISelect<TItem, TItemValue, TValue>.ComputedItems => ComputedItems;

        IList<TItemValue> ISelect<TItem, TItemValue, TValue>.InternalValues => InternalValues;

        object ISelect<TItem, TItemValue, TValue>.Menu
        {
            set => Menu = value;
        }

        IList<TItem> ISelect<TItem, TItemValue, TValue>.SelectedItems => SelectedItems;

        string ISelect<TItem, TItemValue, TValue>.GetText(TItem item) => GetText(item);

        TItemValue ISelect<TItem, TItemValue, TValue>.GetValue(TItem item) => GetValue(item);

        bool ISelect<TItem, TItemValue, TValue>.GetDisabled(TItem item) => GetDisabled(item);

        protected bool IsMenuActive
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        protected int MenuListIndex { get; private set; } = -1;

        protected int SelectedIndex { get; set; } = -1;

        public override int DebounceMilliseconds { get; set; }

        protected object Menu { get; set; }

        protected MMenu MMenu => Menu as MMenu;

        protected BMenuProps ComputedMenuProps { get; set; }

        protected bool HasChips => Chips || SmallChips;

        protected override bool IsDirty => SelectedItems.Count > 0;
        public override Action<TextFieldNumberProperty> NumberProps { get; set; }

        protected override Dictionary<string, object> InputAttrs => new()
        {
            { "type", Type },
            { "value", null },
            { "readonly", true }
        };

        protected virtual IList<TItem> ComputedItems => Items;

        protected IList<TItemValue> InternalValues
        {
            get
            {
                if (InternalValue is IList<TItemValue> values)
                {
                    return values;
                }

                if (InternalValue is TItemValue value)
                {
                    return new List<TItemValue>
                    {
                        value
                    };
                }

                return new List<TItemValue>();
            }
        }

        protected virtual IList<TItem> SelectedItems
        {
            get { return Items.Where(item => InternalValues.Contains(ItemValue(item))).ToList(); }
        }

        protected virtual bool MenuCanShow => true;

        protected virtual BMenuProps GetDefaultMenuProps() => new()
        {
            CloseOnClick = true,
            CloseOnContentClick = false,
            DisableKeys = true,
            OpenOnClick = true,
            MaxHeight = 304,
        };

        protected virtual string GetText(TItem item)
        {
            return item == null ? null : ItemText(item);
        }

        protected TItemValue GetValue(TItem item)
        {
            return ItemValue(item);
        }

        protected bool GetDisabled(TItem item)
        {
            return ItemDisabled(item);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            ComputedMenuProps = GetDefaultMenuProps();
            MenuProps?.Invoke(ComputedMenuProps);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await JsInvokeAsync(JsInteropConstants.PreventDefaultOnArrowUpDown, InputElement);
                //await (Menu as MMenu)?.UpdateActivatorAsync(InputSlotElement);
            }

            if (MMenu is not null && InputSlotAttrs.Keys.Count == 0)
            {
                InputSlotAttrs = MMenu.ActivatorAttributes;
                MMenu.CloseConditional = CloseConditional;
                MMenu.Handler = Blur;

                if (!MMenu.Attached && InputSlotAttrs.Keys.Count > 0)
                {
                    await MMenu.RemoveOutsideClickEventListener();

                    // Before the select menu element is generated,
                    // some scenarios still need to trigger outside-click events,
                    // such as when input is focused through the tab key.
                    await MMenu.AddOutsideClickEventListener();
                }

                StateHasChanged();
            }
        }

        protected override void OnWatcherInitialized()
        {
            base.OnWatcherInitialized();

            Watcher.Watch<bool>(nameof(IsMenuActive), WatchIsMenuActive);
        }

        protected virtual async void WatchIsMenuActive(bool val)
        {
            if ((Multiple && !val) || GetMenuIndex() > -1)
            {
                return;
            }

            var index = await JsInvokeAsync<int>(JsInteropConstants.GetListIndexWhereAttributeExists,
                $"{MMenu.ContentElement.GetSelector()} .m-list-item",
                "aria-selected", "True");

            SetMenuIndex(index);
            StateHasChanged();
        }

        private async Task<bool> CloseConditional(ClickOutsideArgs args)
        {
            if (IsFocused) return true;

            if (!IsMenuActive) return true;

            var contains = await JsInvokeAsync<bool>(JsInteropConstants.Contains, MMenu.ContentElement, args.PointerSelector);

            return !contains;
        }

        private Task Blur()
        {
            var prevIsMenuActive = IsMenuActive;
            var prevIsFocused = IsFocused;
            var prevSelectedIndex = SelectedIndex;
            var prevMenuListIndex = MenuListIndex;

            IsMenuActive = false;
            IsFocused = false;
            SelectedIndex = -1;
            SetMenuIndex(-1);

            if (prevIsMenuActive || prevIsFocused || prevSelectedIndex != -1 || prevMenuListIndex != -1)
            {
                StateHasChanged();
            }

            return Task.CompletedTask;
        }

        protected override void SetComponentClass()
        {
            base.SetComponentClass();

            CssProvider
                .Merge(cssBuilder =>
                {
                    cssBuilder
                        .Add("m-select")
                        .AddIf("m-select--is-menu-active", () => IsMenuActive)
                        .AddIf("m-select--is-multi", () => Multiple)
                        .AddIf("m-select--chips", () => Chips)
                        .AddIf("m-select--chips--small", () => SmallChips)
                        .AddTheme(IsDark);
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddMinWidth(MinWidth);
                })
                .Apply("select-slot", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-select__slot");
                })
                .Apply("selections", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-select__selections");
                })
                .Apply("selection-comma", cssBuilder =>
                {
                    //TODO: color,disabled
                    cssBuilder
                        .Add("m-select__selection")
                        .Add("m-select__selection--comma");
                })
                .Apply("input-wrapper", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-select__selections__input-wrapper");
                });

            AbstractProvider
                .ApplySelectDefault<TItem, TItemValue, TValue>()
                .Merge<BIcon, MIcon>("append-icon", attrs =>
                {
                    // Don't allow the dropdown icon to be focused
                    var onClick = (EventCallback<MouseEventArgs>)attrs.Data;
                    if (onClick.HasDelegate)
                    {
                        attrs["tabindex"] = -1;
                    }

                    attrs["aria-hidden"] = "true";
                    attrs["aria-label"] = null;
                })
                .Apply<BMenu, MMenu>(attrs =>
                {
                    attrs[nameof(MMenu.ExternalActivator)] = true;
                    attrs[nameof(MMenu.Value)] = MenuCanShow && IsMenuActive;
                    attrs[nameof(MMenu.ValueChanged)] = EventCallback.Factory.Create<bool>(this, async val =>
                    {
                        IsMenuActive = val;
                        if (val && !IsFocused && !IsDisabled)
                        {
                            IsFocused = true;
                            await InputElement.FocusAsync();
                        }
                    });
                    attrs[nameof(MMenu.Disabled)] = Disabled || Readonly;
                    attrs[nameof(MMenu.Bottom)] = ComputedMenuProps.Bottom;
                    attrs[nameof(MMenu.CloseOnClick)] = ComputedMenuProps.CloseOnClick;
                    attrs[nameof(MMenu.CloseOnContentClick)] = ComputedMenuProps.CloseOnContentClick;
                    attrs[nameof(MMenu.DisableKeys)] = ComputedMenuProps.DisableKeys;
                    attrs[nameof(MMenu.Left)] = ComputedMenuProps.Left;
                    attrs[nameof(MMenu.MaxHeight)] = ComputedMenuProps.MaxHeight;
                    attrs[nameof(MMenu.MinWidth)] = ComputedMenuProps.MinWidth;
                    attrs[nameof(MMenu.NudgeTop)] = ComputedMenuProps.NudgeTop;
                    attrs[nameof(MMenu.NudgeRight)] = ComputedMenuProps.NudgeRight;
                    attrs[nameof(MMenu.NudgeBottom)] = ComputedMenuProps.NudgeBottom;
                    attrs[nameof(MMenu.NudgeLeft)] = ComputedMenuProps.NudgeLeft;
                    attrs[nameof(MMenu.NudgeWidth)] = ComputedMenuProps.NudgeWidth;
                    attrs[nameof(MMenu.OffsetX)] = ComputedMenuProps.OffsetX;
                    attrs[nameof(MMenu.OffsetY)] = ComputedMenuProps.OffsetY;
                    attrs[nameof(MMenu.OpenOnClick)] = ComputedMenuProps.OpenOnClick;
                    attrs[nameof(MMenu.Right)] = ComputedMenuProps.Right;
                    attrs[nameof(MMenu.Top)] = ComputedMenuProps.Top;
                })
                .Apply(typeof(BSelectList<,,>), typeof(MSelectList<TItem, TItemValue, TValue>), attrs =>
                {
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.Action)] = Multiple;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.Color)] = ItemColor;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.Dense)] = Dense;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.HideSelected)] = HideSelected;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.Items)] = ComputedItems;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.ItemDisabled)] = ItemDisabled;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.ItemText)] = ItemText;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.ItemValue)] = ItemValue;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.NoDataText)] = NoDataText;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.SelectedItems)] = SelectedItems;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.OnSelect)] = CreateEventCallback<TItem>(SelectItem);
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.ItemContent)] = ItemContent;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.PrependItemContent)] = PrependItemContent;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.AppendItemContent)] = AppendItemContent;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.SelectedIndex)] = MenuListIndex;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.NoDataContent)] = NoDataContent;
                })
                .Apply<BChip, MChip>(attrs =>
                {
                    attrs[nameof(MChip.Close)] = DeletableChips && (!IsDisabled && !IsReadonly);
                    attrs[nameof(MChip.Disabled)] = IsDisabled;
                    attrs[nameof(MChip.Class)] = "m-chip--select";
                    attrs[nameof(MChip.Small)] = SmallChips;
                });
        }

        protected async Task SelectItemByIndex(int index)
        {
            if (index > -1 && index < ComputedItems.Count)
            {
                var item = ComputedItems[index];
                await SelectItem(item);
            }
        }

        protected virtual async Task SelectItem(TItem item)
        {
            var value = ItemValue(item);
            if (!Multiple)
            {
                if (value is TValue val)
                {
                    await SetInternalValueAsync(val);
                }

                IsMenuActive = false;

                SetMenuIndex(ComputedItems.IndexOf(item));
            }
            else
            {
                var internalValues = InternalValues.ToList();
                if (internalValues.Contains(value))
                {
                    internalValues.Remove(value);
                }
                else
                {
                    internalValues.Add(value);
                }

                if (internalValues is TValue val)
                {
                    await SetInternalValueAsync(val);
                }

                if (HideSelected)
                {
                    SetMenuIndex(-1);
                }
                else
                {
                    var index = ComputedItems.IndexOf(item);
                    SetMenuIndex(index);
                }
            }
        }

        public override async Task HandleOnKeyDownAsync(KeyboardEventArgs args)
        {
            switch (args.Code)
            {
                case "ArrowUp":
                    ChangeSelectedIndex(-1);
                    break;
                case "ArrowDown":
                    ChangeSelectedIndex(+1);
                    break;
                case "Enter":
                    await OnEnter(args);
                    break;
                case "Tab":
                    await OnTabDown(args);
                    break;
                default:
                    break;
            }
        }

        private void ChangeSelectedIndex(int change)
        {
            var index = MenuListIndex + change;
            if (index > ComputedItems.Count - 1)
            {
                //Back to first
                index = 0;
            }
            else if (index < 0)
            {
                //Go to last
                index = ComputedItems.Count - 1;
            }

            SetMenuIndex(index);
        }

        protected virtual async Task OnEnter(KeyboardEventArgs args)
        {
            if (IsMenuActive)
            {
                await SelectItemByIndex(MenuListIndex);
            }
            else
            {
                IsMenuActive = true;
            }
        }

        protected virtual async Task OnTabDown(KeyboardEventArgs args)
        {
            // An item that is selected by
            // menu-index should toggled
            if (!Multiple && MenuListIndex != -1 && IsMenuActive)
            {
                // TODO: need e.preventDefault() and e.stopPropagation()?

                await SelectItemByIndex(MenuListIndex);
            }
            else
            {
                // If we make it here,
                // the user has no selected indexes
                // and is probably tabbing out
                await Blur();
            }
        }

        public override async Task HandleOnBlurAsync(FocusEventArgs args)
        {
            if (OnBlur.HasDelegate)
            {
                await OnBlur.InvokeAsync(args);
            }
        }

        public override async Task HandleOnClickAsync(MouseEventArgs args)
        {
            await base.HandleOnClickAsync(args);

            // TODO: isInteractive?
            // TODO: isAppendInner?

            if (!IsFocused)
            {
                IsFocused = true;
                if (OnFocus.HasDelegate)
                {
                    await OnFocus.InvokeAsync();
                }
            }
        }

        public override async Task HandleOnClearClickAsync(MouseEventArgs args)
        {
            if (Multiple)
            {
                IList<TItemValue> values = new List<TItemValue>();
                await SetInternalValueAsync((TValue)values);
            }
            else
            {
                await SetInternalValueAsync(default);
            }

            if (OnClearClick.HasDelegate)
            {
                await OnClearClick.InvokeAsync(args);
            }

            SetMenuIndex(-1);

            // whether to need NextTick?
            await InputElement.FocusAsync();

            // TODO: OpenOnClear
        }

        protected void SetMenuIndex(int number)
        {
            MenuListIndex = number;

            if (number > -1)
            {
                _ = JsInvokeAsync(JsInteropConstants.ScrollToTile,
                    MMenu.ContentElement.GetSelector(),
                    $"{MMenu.ContentElement.GetSelector()} .m-list-item",
                    number);
            }
        }

        protected int GetMenuIndex()
        {
            return MenuListIndex;
        }
    }
}
