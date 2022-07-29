namespace Masa.Blazor
{
    public class MasaBlazorComponentTypeMapper : IAbstractComponentTypeMapper
    {
        private readonly Dictionary<Type, Type> _mapper = new()
        {
            { typeof(BButton), typeof(MButton) },
            { typeof(BIcon), typeof(MIcon) },
            { typeof(BAvatar), typeof(MAvatar) },
            { typeof(BBreadcrumbsDivider), typeof(MBreadcrumbsDivider) },
            { typeof(BBreadcrumbsItem), typeof(MBreadcrumbsItem) },
            { typeof(BProgressCircular), typeof(MProgressCircular) },
            { typeof(BProgressLinear), typeof(MProgressLinear) },
            { typeof(BMenu), typeof(MMenu) },
            { typeof(BList), typeof(MList) },
            { typeof(BItemGroup), typeof(MItemGroup) },
            { typeof(BButtonGroup), typeof(MButtonGroup) },
            { typeof(BDataFooter), typeof(MDataFooter) },
            { typeof(BSimpleCheckbox), typeof(MSimpleCheckbox) },
            { typeof(BDataTableRowGroup), typeof(MDataTableRowGroup) },
            { typeof(BDataTableHeader), typeof(MDataTableHeader) },
            { typeof(BSimpleTable), typeof(MSimpleTable) },
            { typeof(BDatePickerHeader), typeof(MDatePickerHeader) },
            { typeof(BDatePickerYears), typeof(MDatePickerYears) },
            { typeof(BPicker), typeof(MPicker) },
            { typeof(BDatePickerTitle), typeof(MDatePickerTitle) },
            { typeof(BOverlay), typeof(MOverlay) },
            { typeof(BChip), typeof(MChip) },
            { typeof(BLabel), typeof(MLabel) },
            { typeof(BMessages), typeof(MMessages) },
            { typeof(BListItem), typeof(MListItem) },
            { typeof(BListItemIcon), typeof(MListItemIcon) },
            { typeof(BListItemContent), typeof(MListItemContent) },
            { typeof(BListItemAction), typeof(MListItemAction) },
            { typeof(BListItemTitle), typeof(MListItemTitle) },
            { typeof(BSlideGroup), typeof(MTabsBar) }, //We will remove this when Decorator come up
            { typeof(BWindow), typeof(MWindow) },
            { typeof(BWindowItem), typeof(MWindowItem) },
            { typeof(BCounter), typeof(MCounter) },
            { typeof(BTimePickerClock), typeof(MTimePickerClock) },
            { typeof(BTimePickerTitle), typeof(MTimePickerTitle) },
            { typeof(BTooltip), typeof(MTooltip) },
            { typeof(BDivider), typeof(MDivider) },
            { typeof(BSubheader), typeof(MSubheader) },
            { typeof(BTab), typeof(MTab) },
            { typeof(BResponsive), typeof(MImage) }
        };

        private readonly Dictionary<Type, Type> _genericMapper = new()
        {
            { typeof(BSelectList<,,>), typeof(MSelectList<,,>) },
            { typeof(BCascaderList<,>), typeof(MCascaderList<,>) },
            { typeof(BDataTableRow<>), typeof(MDataTableRow<>) },
            { typeof(BTreeviewNode<,>), typeof(MTreeviewNode<,>) },
        };

        public Type Map(Type keyType)
        {
            if (_mapper.ContainsKey(keyType))
            {
                return _mapper[keyType];
            }

            if (keyType.IsGenericType && _genericMapper.ContainsKey(keyType.GetGenericTypeDefinition()))
            {
                var genericType = _genericMapper[keyType.GetGenericTypeDefinition()];
                return genericType.MakeGenericType(keyType.GetGenericArguments());
            }

            return keyType;
        }
    }
}
