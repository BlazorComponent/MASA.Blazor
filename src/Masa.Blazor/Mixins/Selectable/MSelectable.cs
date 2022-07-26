using Microsoft.AspNetCore.Components.Web;

namespace Masa.Blazor;

// TODO: move to BlazorComponent
public partial class MSelectable : MInput<bool>, ISelectable
{
    [Parameter]
    public bool? Ripple { get; set; }

    public Dictionary<string, object> InputAttrs => new();

    public override bool HasColor => InternalValue;

    public override string ComputedColor => Color ?? (IsDark ? "white" : "primary");

    public bool IsActive
    {
        get
        {
            // TODO: custom trueValue and falseValue.

            return InternalValue;
        }
    }

    protected override bool IsDirty => IsActive;

    public override async Task HandleOnClickAsync(ExMouseEventArgs args)
    {
        await HandleOnChange();

        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(args);
        }
    }

    public async Task HandleOnChange()
    {
        if (!IsInteractive)
        {
            return;
        }

        // TODO: custom trueValue and falseValue.

        var input = InternalValue;
        input = !input;

        await ValidateAsync(true, input);

        // await SetInternalValueAsync(input);
        InternalValue = input;
    }

    public async Task HandleOnBlur(FocusEventArgs args)
    {
        IsFocused = false;
    }

    public async Task HandleOnFocus(FocusEventArgs args)
    {
        IsFocused = true;
    }

    public Task HandleOnKeyDown(KeyboardEventArgs args)
    {
        return Task.CompletedTask;
    }

    protected override void SetComponentClass()
    {
        base.SetComponentClass();

        AbstractProvider
            .Merge(typeof(BLabel), typeof(MLabel), attrs =>
            {
                attrs[nameof(MLabel.Attributes)] = new Dictionary<string, object>()
                {
                    { "__internal_preventDefault_onclick", true }
                };
            });
    }
}
