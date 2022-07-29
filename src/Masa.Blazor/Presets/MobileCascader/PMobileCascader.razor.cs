namespace Masa.Blazor.Presets;

// TODO: add class constraints for TItem after c# 7.0 release

public partial class PMobileCascader<TItem, TItemValue> // where TItem : class
{
    [Inject] private I18n I18n { get; set; } = null!;

    [Parameter] public RenderFragment<ActivatorProps> ActivatorContent { get; set; }

    [Parameter] public StringNumber Height { get; set; }

    [Parameter, EditorRequired] public List<TItem> Items { get; set; }

    [Parameter, EditorRequired] public Func<TItem, List<TItem>> ItemChildren { get; set; }

    [Parameter] public Func<TItem, bool> ItemDisabled { get; set; }

    [Parameter, EditorRequired] public Func<TItem, string> ItemText { get; set; }

    [Parameter, EditorRequired] public Func<TItem, TItemValue> ItemValue { get; set; }

    [Parameter] public EventCallback<TItem> OnLoadChildren { get; set; }

    [Parameter] public EventCallback<TItem> OnSelect { get; set; }

    [Parameter] public EventCallback<TItem> OnDeselect { get; set; }

    [Parameter] public string Title { get; set; }

    [Parameter] public List<TItemValue> Value { get; set; }

    [Parameter] public EventCallback<List<TItemValue>> ValueChanged { get; set; }

    [Parameter] public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value) return;

            OnVisibleChanged(value);
            _visible = value;
        }
    }

    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    private int _tabIndex;
    private bool _preVisible;
    private TItem _loadingItem;
    private List<TItem> _loadedItems = new();
    private bool _visible;

    private List<TItem> SelectedItems { get; set; } = new();

    private string PleaseSelectText => I18n.T("$masaBlazor.mobileCascader.pleaseSelect");

    private List<string> ComputedTabs
    {
        get
        {
            if (SelectedItems.Count == 0)
            {
                return new List<string>() { PleaseSelectText };
            }

            var tabs = SelectedItems.Select(t => ItemText(t)).ToList();

            if (HasChildren)
            {
                tabs.Add(PleaseSelectText);
            }

            return tabs;
        }
    }

    private TItem Current => SelectedItems.Count > 0 ? SelectedItems.Last() : default;

    private List<TItem> Children => Current == null ? null : ItemChildren(Current);

    private bool HasChildren => Children is not null && Children.Count > 0;

    private List<TItem> CurrentItems
    {
        get
        {
            if (_tabIndex == 0)
            {
                return Items;
            }

            if (SelectedItems.Count == _tabIndex)
            {
                return ItemChildren(Current);
            }

            return ItemChildren(SelectedItems[_tabIndex - 1]);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        Height ??= "80vh";
        Value ??= new List<TItemValue>();
    }

    private async Task OnItemClick(TItem item)
    {
        if (SelectedItems.Count > _tabIndex)
        {
            var activeTab = SelectedItems[_tabIndex];

            // cancel the selection
            SelectedItems = SelectedItems.Take(_tabIndex).ToList();

            if (EqualityComparer<TItem>.Default.Equals(activeTab, item))
            {
                // cancel the selection and nothing to do
                if (!OnDeselect.HasDelegate) return;

                await OnDeselect.InvokeAsync(item);

                return;
            }
        }

        SelectedItems.Add(item);

        var children = ItemChildren(item);

        if (!_loadedItems.Contains(item) && OnLoadChildren.HasDelegate && (children is null || children.Count == 0))
        {
            _loadingItem = item;
            _loadedItems.Add(item);

            try
            {
                await OnLoadChildren.InvokeAsync(item);
            }
            catch
            {
                _loadingItem = default;
                _loadedItems.Remove(item);
                StateHasChanged();
                throw;
            }
            finally
            {
                _loadingItem = default;
            }

            children = ItemChildren(item);
        }

        if (children is not null && children.Count > 0)
        {
            _tabIndex++;
        }

        if (OnSelect.HasDelegate)
        {
            await OnSelect.InvokeAsync(item);
        }
    }

    private bool IsDisabled(TItem item)
    {
        return (ItemDisabled is not null && ItemDisabled(item)) || item.Equals(_loadingItem);
    }

    private async Task OnCancel()
    {
        if (VisibleChanged.HasDelegate)
        {
            await VisibleChanged.InvokeAsync(false);
        }
        else
        {
            Visible  = false;
        }
    }

    private async Task OnConfirm()
    {
        var value = SelectedItems.Select(ItemValue).ToList();

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }
        else
        {
            Value = value;
        }

        await OnCancel();
    }

    private void Reset()
    {
        SelectedItems.Clear();
        _tabIndex = 0;
    }

    private async Task HandleVisibleChanged(bool val)
    {
        if (VisibleChanged.HasDelegate)
        {
            await VisibleChanged.InvokeAsync(val);
        }
        else
        {
            Visible = val;
        }
    }

    private void OnVisibleChanged(bool visible)
    {
        if (visible)
        {
            if (Value is null || Value.Count <= 0) return;

            var items = Items;

            for (var index = 0; index < Value.Count; index++)
            {
                var v = Value[index];
                if (!TryGetItem(items, v, out var item))
                {
                    break;
                }

                SelectedItems.Add(item);

                _tabIndex = index;

                var children = ItemChildren(item);
                if (children is null || children.Count == 0)
                {
                    break;
                }

                items = children;

                // If it is the last loop and there are child elements,
                // need to activate the next tab
                if (index == Value.Count - 1)
                {
                    _tabIndex++;
                }
            }
        }
        else
        {
            Reset();
        }
    }

    private bool TryGetItem(List<TItem> items, TItemValue value, out TItem item)
    {
        item = default;

        var index = items.FindIndex(item => EqualityComparer<TItemValue>.Default.Equals(ItemValue(item), value));
        if (index == -1)
        {
            return false;
        }

        item = items[index];

        return true;
    }
}
