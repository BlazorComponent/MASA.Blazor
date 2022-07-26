using System.Linq.Expressions;
using System.Reflection.Metadata;
using BlazorComponent.Web;
using Microsoft.AspNetCore.Components.Web;

namespace Masa.Blazor
{
    public class MSelect<TItem, TItemValue, TValue> : MTextField<TValue>, ISelect<TItem, TItemValue, TValue>
    {
        [Inject]
        protected I18n I18n { get; set; }

        [Parameter]
        public override string AppendIcon { get; set; } = "mdi-menu-down";

        //TODO:Attach

        [Parameter]
        public bool CacheItems { get; set; }

        [Parameter]
        public bool Chips { get; set; }

        [Parameter]
        public bool DeletableChips { get; set; }

        //TODO: DisableLookup,Eager

        [Parameter]
        public bool HideSelected { get; set; }

        [EditorRequired]
        [Parameter]
        public IList<TItem> Items
        {
            get => GetValue((IList<TItem>)new List<TItem>());
            set => SetValue(value);
        }

        [Parameter]
        public string ItemColor { get; set; } = "primary";

        [Parameter]
        public Func<TItem, bool> ItemDisabled { get; set; } = _ => false;

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
        public string NoDataText { get; set; }

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

        private static Func<TItem, string> ItemHeader { get; } = GetFuncOrDefault<string>("Header");

        private static Func<TItem, bool> ItemDivider { get; } = GetFuncOrDefault<bool>("Divider");

        private IList<TItem> CachedItems { get; set; } = new List<TItem>();

        protected bool IsMenuActive
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        protected int MenuListIndex { get; private set; } = -1;

        public int SelectedIndex { get; set; } = -1;

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

        protected List<TItem> AllItems => FilterDuplicates(CachedItems.Concat(Items)).ToList();

        protected virtual List<TItem> ComputedItems => AllItems;

        protected List<TItem> ComputedItemsIfHideSelected =>
            HideSelected ? ComputedItems.Where(item => !SelectedItems.Contains(item)).ToList() : ComputedItems;

        protected IList<TItemValue> InternalValues
        {
            get
            {
                return GetComputedValue(() =>
                {
                    return InternalValue switch
                    {
                        IList<TItemValue> values => values,
                        TItemValue value => new List<TItemValue> { value },
                        _ => new List<TItemValue>()
                    };
                }, new[] { nameof(InternalValue), nameof(Value) });
            }
        }

        protected virtual IList<TItem> SelectedItems { get; set; } = new List<TItem>();

        protected string TilesSelector =>
            $"{MMenu.ContentElement.GetSelector()} .m-list-item, {MMenu.ContentElement.GetSelector()} .m-divider, {MMenu.ContentElement.GetSelector()} .m-subheader";

        protected virtual bool MenuCanShow => true;

        protected virtual BMenuProps GetDefaultMenuProps() => new()
        {
            CloseOnClick = false,
            CloseOnContentClick = false,
            DisableKeys = true,
            OpenOnClick = false,
            MaxHeight = 304
        };

        protected virtual string GetText(TItem item) => item is null || ItemText is null ? null : ItemText(item);

        protected TItemValue GetValue(TItem item) => ItemValue is null ? default : ItemValue(item);

        protected bool GetDisabled(TItem item) => ItemDisabled(item);

        protected virtual bool EnableSpaceKeDownPreventDefault => true;

        public override Task SetParametersAsync(ParameterView parameters)
        {
            NoDataText = I18n.T("$masaBlazor.noDataText");

            return base.SetParametersAsync(parameters);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            CachedItems = CacheItems ? Items : new List<TItem>();
            InternalValue = Value;

            ComputedMenuProps = GetDefaultMenuProps();
            MenuProps?.Invoke(ComputedMenuProps);
            if (ComputedMenuProps.OffsetY && ComputedMenuProps.NudgeBottom is null)
            {
                ComputedMenuProps.NudgeBottom = 1;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                var keys = new List<string> { KeyCodes.ArrowUp, KeyCodes.ArrowDown, KeyCodes.Home, KeyCodes.End, KeyCodes.Enter, KeyCodes.Escape };

                if (EnableSpaceKeDownPreventDefault)
                {
                    keys.Add(KeyCodes.Space);
                }

                await JsInvokeAsync(JsInteropConstants.EnablePreventDefaultForEvent, InputElement, "keydown", keys);
            }

            await GenMenu();
        }

        protected override void OnWatcherInitialized()
        {
            base.OnWatcherInitialized();

            Watcher.Watch<bool>(nameof(IsMenuActive), val => OnMenuActiveChange(val))
                   .Watch<IList<TItem>>(nameof(Items), async _ => OnItemsChange())
                   .Watch<IList<TItemValue>>(nameof(InternalValues), _ => OnInternalValueChange());
        }

        private void OnItemsChange()
        {
            if (CacheItems)
            {
                NextTick(() =>
                {
                    CachedItems = FilterDuplicates(CachedItems.Concat(Items));
                    StateHasChanged();
                });
            }

            NextTickIf(SetSelectedItems, () => ItemValue is null);

            StateHasChanged();
        }

        private IList<TItem> FilterDuplicates(IEnumerable<TItem> list)
        {
            var uniqueKeys = new List<TItemValue>();
            var uniqueItems = new List<TItem>();

            foreach (var item in list)
            {
                if (item is null)
                {
                    continue;
                }

                if (ItemDivider(item) || ItemHeader(item) is not null)
                {
                    uniqueItems.Add(item);
                    continue;
                }

                var val = GetValue(item);
                if (!uniqueKeys.Contains(val))
                {
                    uniqueKeys.Add(val);
                    uniqueItems.Add(item);
                }
            }

            return uniqueItems;
        }

        private async Task GenMenu()
        {
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

                MMenu.AfterShowContent = async (isLazyContent) =>
                {
                    if (isLazyContent)
                    {
                        await OnMenuActiveChange(true);
                    }
                };

                StateHasChanged();
            }
        }

        protected virtual async Task OnMenuActiveChange(bool val)
        {
            if ((Multiple && !val) || GetMenuIndex() > -1)
            {
                return;
            }

            var index = await JsInvokeAsync<int>(JsInteropConstants.GetListIndexWhereAttributeExists,
                TilesSelector,
                "aria-selected", "True");

            await SetMenuIndex(index);

            StateHasChanged();
        }

        private async Task<bool> CloseConditional(ClickOutsideArgs args)
        {
            if (!IsMenuActive) return true;

            var contains = await JsInvokeAsync<bool>(JsInteropConstants.Contains, MMenu.ContentElement, args.PointerSelector);

            return !contains;
        }

        public async Task Blur()
        {
            var prevIsMenuActive = IsMenuActive;
            var prevIsFocused = IsFocused;
            var prevSelectedIndex = SelectedIndex;
            var prevMenuListIndex = MenuListIndex;

            IsMenuActive = false;
            IsFocused = false;
            SelectedIndex = -1;
            await SetMenuIndex(-1);

            if (prevIsMenuActive || prevIsFocused || prevSelectedIndex != -1 || prevMenuListIndex != -1)
            {
                StateHasChanged();
            }
        }

        public void ActivateMenu()
        {
            if (!IsInteractive || IsMenuActive)
            {
                return;
            }

            IsMenuActive = true;
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
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.ItemDivider)] = ItemDivider;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.ItemHeader)] = ItemHeader;
                })
                .Apply<BChip, MChip>(attrs =>
                {
                    var index = -1;
                    TItem item = default;
                    if (attrs.Data is (TItem t, int i))
                    {
                        item = t;
                        index = i;
                    }

                    var isDisabled = GetDisabled(item);
                    var isInteractive = !isDisabled && IsInteractive;

                    attrs[nameof(MChip.Close)] = DeletableChips && isInteractive;
                    attrs[nameof(MChip.Disabled)] = isDisabled;
                    attrs[nameof(MChip.Class)] = "m-chip--select";
                    attrs[nameof(MChip.Attributes)] = new Dictionary<string, object>() { { "tabindex", -1 } };
                    attrs[nameof(MChip.Small)] = SmallChips;
                    attrs[nameof(MChip.IsActive)] = index == SelectedIndex;
                    attrs[nameof(MChip.OnClick)] = CreateEventCallback<MouseEventArgs>(e =>
                    {
                        if (!isInteractive) return;

                        // e.stopPropagation()

                        SelectedIndex = index;
                    });
                    attrs[nameof(MChip.OnCloseClick)] = CreateEventCallback<MouseEventArgs>(_ => OnChipInput(item));
                });
        }

        protected void OnInternalValueChange()
        {
            SetSelectedItems();

            if (Multiple)
            {
                NextTick(() =>
                {
                    // TODO: need this?
                    // await MMenu.UpdateDimensionsAsync();

                    return Task.CompletedTask;
                });
            }

            StateHasChanged();
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
                    // await SetInternalValueAsync(val);
                    InternalValue = val;
                }

                IsMenuActive = false;
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
                    // await SetInternalValueAsync(val);
                    InternalValue = val;
                }
            }

            if (HideSelected)
            {
                await SetMenuIndex(-1);
            }
            else
            {
                var index = ComputedItems.IndexOf(item);
                if (index > -1)
                {
                    await SetMenuIndex(index);
                }
            }

            if (OnSelectedItemUpdate.HasDelegate)
            {
                await OnSelectedItemUpdate.InvokeAsync(item);
            }
        }

        public override async Task HandleOnKeyDownAsync(KeyboardEventArgs args)
        {
            if (IsReadonly && args.Code != KeyCodes.Tab) return;

            if (OnKeyDown.HasDelegate)
            {
                await OnKeyDown.InvokeAsync(args);
            }

            var keyCode = args.Code;

            // If menu is active, allow default
            // listIndex change from menu
            if (IsMenuActive && new[] { KeyCodes.ArrowUp, KeyCodes.ArrowDown, KeyCodes.Home, KeyCodes.End, KeyCodes.Enter }.Contains(keyCode))
            {
                NextTick(async () =>
                {
                    await ChangeMenuListIndex(keyCode);
                    StateHasChanged();
                });
            }

            // If enter, space, open menu
            if (new[] { KeyCodes.Enter, KeyCodes.Space }.Contains(keyCode))
            {
                ActivateMenu();
            }

            // If menu is not active, up/down/home/end can do
            // one of 2 things. If multiple, opens the
            // menu, if not, will cycle through all
            // available options
            if (!IsMenuActive && new[] { KeyCodes.ArrowUp, KeyCodes.ArrowDown, KeyCodes.Home, KeyCodes.End }.Contains(keyCode))
            {
                await OnUpDown(keyCode);
                return;
            }

            // If escape deactivate the menu
            if (keyCode == KeyCodes.Escape)
            {
                await OnEscDown();
                return;
            }

            // If tab - select item or close menu
            if (keyCode == KeyCodes.Tab)
            {
                await OnTabDown(args);
                return;
            }

            // If space preventDefault
            if (keyCode == KeyCodes.Space)
            {
                // invoke preventDefault at js interop
            }
        }

        private async Task ChangeMenuListIndex(string code)
        {
            if (code == KeyCodes.ArrowUp)
            {
                PrevTile();
            }
            else if (code == KeyCodes.ArrowDown)
            {
                NextTile();
            }
            else if (code == KeyCodes.Home)
            {
                FirstTile();
            }
            else if (code == KeyCodes.End)
            {
                LastTile();
            }
            else if (code == KeyCodes.Enter && MenuListIndex != -1)
            {
                var item = ComputedItemsIfHideSelected.ElementAtOrDefault(MenuListIndex);
                await SelectItem(item);
            }

            await SetMenuIndex(MenuListIndex);
        }

        private void NextTile()
        {
            var nextIndex = MenuListIndex + 1;

            if (nextIndex >= ComputedItemsIfHideSelected.Count)
            {
                if (ComputedItemsIfHideSelected.Count == 0) return;

                MenuListIndex = -1;
                NextTile();

                return;
            }

            var nextItem = ComputedItemsIfHideSelected.ElementAtOrDefault(nextIndex);

            MenuListIndex++;
            if (ItemDivider(nextItem) || ItemHeader(nextItem) is not null || ItemDisabled(nextItem))
            {
                NextTile();
            }
        }

        private void PrevTile()
        {
            var prevIndex = MenuListIndex - 1;

            if (prevIndex < 0)
            {
                if (ComputedItemsIfHideSelected.Count == 0) return;

                MenuListIndex = ComputedItemsIfHideSelected.Count;
                PrevTile();

                return;
            }

            var prevItem = ComputedItemsIfHideSelected.ElementAt(prevIndex);

            MenuListIndex--;
            if (ItemDivider(prevItem) || ItemHeader(prevItem) is not null || ItemDisabled(prevItem))
            {
                PrevTile();
            }
        }

        private void LastTile()
        {
            var lastIndex = ComputedItemsIfHideSelected.Count - 1;

            if (lastIndex == -1) return;

            var lastItem = ComputedItemsIfHideSelected.ElementAt(lastIndex);

            MenuListIndex = lastIndex;

            if (ItemDivider(lastItem) || ItemHeader(lastItem) is not null || ItemDisabled(lastItem))
            {
                PrevTile();
            }
        }

        private void FirstTile()
        {
            if (ComputedItemsIfHideSelected.Count == 0) return;

            var firstItem = ComputedItemsIfHideSelected.First();

            MenuListIndex = 0;

            if (ItemDivider(firstItem) || ItemHeader(firstItem) is not null || ItemDisabled(firstItem))
            {
                NextTile();
            }
        }

        protected virtual async Task OnUpDown(string code)
        {
            // Multiple selects do not cycle their value
            // when pressing up or down, instead activate
            // the menu
            if (Multiple)
            {
                ActivateMenu();
                return;
            }

            // TODO: menu.hasClickableTiles

            await ChangeMenuListIndex(code);

            await SelectItemByIndex(MenuListIndex);
        }

        protected virtual Task OnEscDown()
        {
            if (IsMenuActive)
            {
                IsMenuActive = false;
            }

            return Task.CompletedTask;
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

        public override async Task HandleOnClickAsync(ExMouseEventArgs args)
        {
            if (!IsInteractive) return;

            if (await IsAppendInner(args.Target) is false)
            {
                IsMenuActive = true;
            }

            if (!IsFocused)
            {
                IsFocused = true;
                if (OnFocus.HasDelegate)
                {
                    await OnFocus.InvokeAsync();
                }
            }

            if (OnClick.HasDelegate)
            {
                await OnClick.InvokeAsync(args);
            }
        }

        public override async Task HandleOnMouseUpAsync(ExMouseEventArgs args)
        {
            if (HasMouseDown && args.Button != 2 && IsInteractive)
            {
                // If append inner is present
                // and the target is itself
                // or inside, toggle menu
                if (await IsAppendInner(args.Target))
                {
                    IsMenuActive = !IsMenuActive;
                }
            }

            await base.HandleOnMouseUpAsync(args);
        }

        /// <summary>
        /// target is itself or inside
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected Task<bool> IsAppendInner(EventTarget target)
        {
            return JsInvokeAsync<bool>(JsInteropConstants.EqualsOrContains, AppendInnerElement, target.Selector);
        }

        public override async Task HandleOnClearClickAsync(MouseEventArgs args)
        {
            if (Multiple)
            {
                IList<TItemValue> values = new List<TItemValue>();
                // await SetInternalValueAsync((TValue)values);
                InternalValue = (TValue)values;
            }
            else
            {
                // await SetInternalValueAsync(default);
                InternalValue = default;
            }

            if (OnClearClick.HasDelegate)
            {
                await OnClearClick.InvokeAsync(args);
            }

            await SetMenuIndex(-1);

            // whether to need NextTick?
            await InputElement.FocusAsync();

            // TODO: OpenOnClear
        }

        protected async Task SetMenuIndex(int index)
        {
            // TODO: scroll 
            var i = index;
            var menuItem = ComputedItems.ElementAtOrDefault(index);
            if (menuItem is not null)
            {
                i = ComputedItemsIfHideSelected.IndexOf(menuItem);
            }

            if (i > -1)
            {
                await JsInvokeAsync(JsInteropConstants.ScrollToTile,
                    MMenu.ContentElement.GetSelector(),
                    TilesSelector,
                    i);
            }

            MenuListIndex = index;
        }

        protected virtual void SetSelectedItems()
        {
            var selectedItems = new List<TItem>();

            var values = InternalValues;

            foreach (var value in values)
            {
                var index = AllItems.FindIndex(v => EqualityComparer<TItemValue>.Default.Equals(value, GetValue(v)));

                if (index > -1)
                {
                    selectedItems.Add(AllItems[index]);
                }
            }

            SelectedItems = selectedItems;

            StateHasChanged();
        }

        protected int GetMenuIndex()
        {
            return MenuListIndex;
        }

        private async Task OnChipInput(TItem item)
        {
            if (Multiple)
            {
                await SelectItem(item);
            }
            else
            {
                // await SetInternalValueAsync(default(TValue));
                InternalValue = default;
            }

            // if all items have been delete,
            // open menu
            IsMenuActive = SelectedItems.Count == 0;

            SelectedIndex = -1;
        }

        private static Func<TItem, T> GetFuncOrDefault<T>(string name)
        {
            Func<TItem, T> func;
            try
            {
                var parameterExpression = Expression.Parameter(typeof(TItem), "item");
                var propertyExpression = Expression.Property(parameterExpression, name);

                var lambdaExpression = Expression.Lambda<Func<TItem, T>>(propertyExpression, parameterExpression);
                func = lambdaExpression.Compile();
            }
            catch
            {
                func = item => default;
            }

            return func;
        }
    }
}
