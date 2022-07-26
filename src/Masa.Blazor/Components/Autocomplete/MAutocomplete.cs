using Microsoft.AspNetCore.Components.Web;
using OneOf.Types;

namespace Masa.Blazor
{
    public class MAutocomplete<TItem, TItemValue, TValue> : MSelect<TItem, TItemValue, TValue>, IAutocomplete<TItem, TItemValue, TValue>
    {
        private Func<TItem, string, string, bool> _filter;

        [Parameter]
        public bool AllowOverflow { get; set; } = true;

        [Parameter]
        public bool AutoSelectFirst { get; set; }

        [Parameter]
        public Func<TItem, string, string, bool> Filter
        {
            get { return _filter ??= (_, query, text) => text.ToLower().IndexOf(query.ToLower(), StringComparison.Ordinal) > -1; }
            set => _filter = value;
        }

        [Parameter]
        public bool HideNoData { get; set; }

        [Parameter]
        public bool NoFilter
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        [Parameter]
        public string SearchInput { get; set; }

        [Parameter]
        public EventCallback<string> OnSearchInputUpdate { get; set; }

        protected override List<TItem> ComputedItems => FilteredItems;

        public override Action<TextFieldNumberProperty> NumberProps { get; set; }

        protected IList<TItemValue> SelectedValues => SelectedItems.Select(GetValue).ToList();

        protected bool HasDisplayedItems => HideSelected ? FilteredItems.Any(item => !HasItem(item)) : FilteredItems.Count > 0;

        protected bool HasItem(TItem item)
        {
            return SelectedValues.IndexOf(GetValue(item)) > -1;
        }

        protected List<TItem> FilteredItems
        {
            get
            {
                return GetComputedValue(() =>
                {
                    if (!IsSearching || NoFilter || InternalSearch is null)
                    {
                        return AllItems;
                    }

                    return AllItems.Where(item => Filter(item, InternalSearch, GetText(item) ?? "")).ToList();
                }, new[]
                {
                    nameof(NoFilter),
                    nameof(InternalSearch),
                    nameof(Items)
                });
            }
        }

        protected bool IsSearching => (Multiple && SearchIsDirty) || (SearchIsDirty && InternalSearch != GetText(SelectedItem));

        protected TItem SelectedItem => SelectedItems.FirstOrDefault();

        protected bool SearchIsDirty => !string.IsNullOrEmpty(InternalSearch);

        protected string InternalSearch
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        protected override Dictionary<string, object> InputAttrs => new()
        {
            { "type", Type },
            { "value", InternalSearch ?? (HasSlot || Multiple ? null : GetText(SelectedItem)) },
            { "autocomplete", "off" }
        };

        protected override BMenuProps GetDefaultMenuProps()
        {
            var props = base.GetDefaultMenuProps();
            props.OffsetY = true;

            // props.OffsetOverflow = true;
            // props.Transition = false;

            return props;
        }

        protected override bool IsDirty => SearchIsDirty || base.IsDirty;

        protected bool HasSlot => HasChips || SelectionContent != null;

        bool IAutocomplete<TItem, TItemValue, TValue>.HasSlot => HasSlot;

        protected override bool MenuCanShow
        {
            get
            {
                if (!IsFocused) return false;

                return HasDisplayedItems || !HideNoData;
            }
        }

        protected override bool EnableSpaceKeDownPreventDefault => false;

        protected override void OnWatcherInitialized()
        {
            base.OnWatcherInitialized();

            Watcher
                .Watch<string>(nameof(SearchInput), val => InternalSearch = val)
                .Watch<List<TItem>>(nameof(FilteredItems), OnFilteredItemsChanged);
        }

        protected override void SetComponentClass()
        {
            base.SetComponentClass();

            CssProvider
                .Merge(cssBuilder =>
                {
                    cssBuilder
                        .Add("m-autocomplete");
                });

            AbstractProvider
                .ApplyAutocompleteDefault<TItem, TItemValue, TValue>()
                .Merge(typeof(BSelectList<,,>), typeof(MSelectList<TItem, TItemValue, TValue>), attrs =>
                {
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.SearchInput)] = InternalSearch;
                    attrs[nameof(MSelectList<TItem, TItemValue, TValue>.NoFilter)] = NoFilter || !IsSearching || FilteredItems.Count == 0;
                });
        }

        protected override async Task OnIsFocusedChange(bool val)
        {
            if (val)
            {
                // TODO: input.select()
            }
            else
            {
                await Blur();
                UpdateSelf();
            }
        }

        protected override async Task SelectItem(TItem item)
        {
            await base.SelectItem(item);
            SetSearch();
        }

        protected override void SetSelectedItems()
        {
            base.SetSelectedItems();

            if (!IsFocused)
            {
                SetSearch();
            }
        }

        public override async Task HandleOnInputAsync(ChangeEventArgs args)
        {
            if (SelectedIndex > -1)
            {
                return;
            }

            var value = args.Value?.ToString();

            if (value is not null)
            {
                ActivateMenu();
            }

            if (!Multiple && string.IsNullOrEmpty(value))
            {
                await DeleteCurrentItem();
            }

            InternalSearch = value;

            if (OnSearchInputUpdate.HasDelegate)
            {
                await OnSearchInputUpdate.InvokeAsync(InternalSearch);
            }
        }

        private async Task DeleteCurrentItem()
        {
            var curIndex = SelectedIndex;
            var curItem = SelectedItems.ElementAtOrDefault(curIndex);

            if (!IsInteractive || GetDisabled(curItem))
            {
                return;
            }

            var lastIndex = SelectedItems.Count - 1;

            if (SelectedIndex == -1 && lastIndex != 0)
            {
                SelectedIndex = lastIndex;
                return;
            }

            var length = SelectedItems.Count;
            var nextIndex = curIndex != length - 1 ? curIndex : curIndex - 1;
            var nextItem = SelectedItems.ElementAtOrDefault(nextIndex);

            if (nextItem is null)
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
            }
            else
            {
                await SelectItem(curItem);
            }

            SelectedIndex = nextIndex;
        }

        public override async Task HandleOnKeyDownAsync(KeyboardEventArgs args)
        {
            var keyCode = args.Code;

            if (args.CtrlKey || !new[] { KeyCodes.Home, KeyCodes.End }.Contains(keyCode))
            {
                await base.HandleOnKeyDownAsync(args);
            }

            await ChangeSelectedIndex(keyCode);
        }

        protected override async Task OnTabDown(KeyboardEventArgs args)
        {
            await base.OnTabDown(args);
            UpdateSelf();
        }

        protected override Task OnUpDown(string code)
        {
            ActivateMenu();
            return Task.CompletedTask;
        }

        public override async Task HandleOnClickAsync(ExMouseEventArgs args)
        {
            if (!IsInteractive) return;

            if (SelectedIndex > -1)
            {
                SelectedIndex = -1;
            }
            else
            {
                await HandleOnFocusAsync(new FocusEventArgs());
            }

            if (!await IsAppendInner(args.Target))
            {
                ActivateMenu();
            }
        }

        public override async Task HandleOnClearClickAsync(MouseEventArgs args)
        {
            if (!string.IsNullOrEmpty(InternalSearch))
            {
                InternalSearch = null;
            }

            await base.HandleOnClearClickAsync(args);
        }

        private async void OnFilteredItemsChanged(IList<TItem> val, IList<TItem> oldVal)
        {
            if (Equals(val, oldVal))
            {
                return;
            }

            val ??= new List<TItem>();
            oldVal ??= new List<TItem>();

            if (!AutoSelectFirst)
            {
                var preSelectedItem = oldVal.ElementAtOrDefault(MenuListIndex);
                if (preSelectedItem is not null)
                {
                    await SetMenuIndex(val.IndexOf(preSelectedItem));
                }
                else
                {
                    await SetMenuIndex(-1);
                }
            }

            StateHasChanged();

            NextTick(async () =>
            {
                if (string.IsNullOrEmpty(InternalSearch) || (val.Count != 1 && !AutoSelectFirst))
                {
                    await SetMenuIndex(-1);
                }

                if (AutoSelectFirst && val.Count > 0)
                {
                    await SetMenuIndex(0);
                }

                StateHasChanged();
            });
        }

        private async Task ChangeSelectedIndex(string keyCode)
        {
            if (SearchIsDirty) return;

            if (Multiple && keyCode == KeyCodes.ArrowLeft)
            {
                if (SelectedIndex == -1)
                {
                    SelectedIndex = SelectedItems.Count - 1;
                }
                else
                {
                    SelectedIndex--;
                }
            }
            else if (Multiple && keyCode == KeyCodes.ArrowRight)
            {
                if (SelectedIndex >= SelectedItems.Count - 1)
                {
                    SelectedIndex = -1;
                }
                else
                {
                    SelectedIndex++;
                }
            }
            else if (keyCode is KeyCodes.Backspace or KeyCodes.Delete)
            {
                await DeleteCurrentItem();
            }
        }

        private void UpdateSelf()
        {
            if (!SearchIsDirty && !EqualityComparer<TValue>.Default.Equals(InternalValue, default))
            {
                return;
            }

            if (!Multiple && !string.Equals(InternalSearch, GetText(SelectedItem)))
            {
                SetSearch();
            }
        }

        private void SetSearch()
        {
            NextTick(() =>
            {
                if (!Multiple || string.IsNullOrEmpty(InternalSearch) || !IsMenuActive)
                {
                    InternalSearch = (SelectedItems.Count == 0 || Multiple || HasSlot) ? null : GetText(SelectedItem);
                }

                StateHasChanged();

                return Task.CompletedTask;
            });
        }
    }
}
