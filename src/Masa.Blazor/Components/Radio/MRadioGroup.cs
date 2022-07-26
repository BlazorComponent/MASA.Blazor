namespace Masa.Blazor
{
    //TODO:该组件需要完善
    public partial class MRadioGroup<TValue> : MInput<TValue>
    {
        [Parameter]
        public bool Column { get; set; } = true;

        [Parameter]
        public bool Mandatory { get; set; }

        [Parameter]
        public bool Row { get; set; }

        public override int DebounceMilliseconds { get; set; }

        protected List<MRadio<TValue>> Items { get; set; } = new();

        protected override void SetComponentClass()
        {
            base.SetComponentClass();

            var prefix = "m-input";
            CssProvider
                .Merge(cssBuilder =>
                {
                    cssBuilder
                        .Add($"{prefix}--selection-controls")
                        .Add($"{prefix}--radio-group")
                        .AddIf($"{prefix}--radio-group--column", () => Column && !Row)
                        .AddIf($"{prefix}--radio-group--row", () => Row);
                })
                .Apply("radio-group", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input--radio-group__input");
                });

            AbstractProvider
                .Merge(typeof(BInputDefaultSlot<,>), typeof(BRadioGroupDefaultSlot<TValue>));
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            SetActiveRadio();
        }

        private void SetActiveRadio()
        {
            // if no value provided and mandatory
            // assign first item
            if (InternalValue == null)
            {
                if (!Mandatory) return;

                var item = Items.FirstOrDefault(item => !item.Disabled);
                if (item == null) return;

                _ = UpdateItemsState(item);

                return;
            }

            foreach (var radio in Items)
            {
                if (EqualityComparer<TValue>.Default.Equals(radio.Value, InternalValue))
                {
                    radio.Active();
                }
                else
                {
                    radio.DeActive();
                }
            }
        }

        public void AddRadio(MRadio<TValue> radio)
        {
            if (!Items.Contains(radio))
            {
                Items.Add(radio);
                radio.NotifyChange += UpdateItemsState;
            }

            SetActiveRadio();
        }

        public async Task UpdateItemsState(BRadio<TValue> radio)
        {
            foreach (var item in Items)
            {
                if (item == radio)
                {
                    item.Active();
                }
                else
                {
                    item.DeActive();
                }
            }

            // await SetInternalValueAsync(radio.Value);
            InternalValue = radio.Value;
        }
    }
}