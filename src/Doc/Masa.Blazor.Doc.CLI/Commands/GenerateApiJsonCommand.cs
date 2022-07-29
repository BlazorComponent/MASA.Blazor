using Masa.Blazor.Doc.CLI.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Masa.Blazor.Doc.CLI.Commands
{
    public class GenerateApiJsonCommand : IAppCommand
    {
        private static readonly Dictionary<string, string[]> _dic = new()
        {
            { "MAvatar", new string[] { "Avatars" } },
            { "MBadge", new string[] { "Badges" } },
            { "MBanner", new string[] { "Banners" } },
            { "MBorder", new string[] { "Borders" } },
            { "MBottomNavigation", new string[] { "Bottom Navigation" } },
            { "MBottomSheet", new string[] { "Bottom sheets" } },
            { "MBreadcrumbs", new string[] { "Breadcrumbs" } },
            { "MBreadcrumbsDivider", new string[] { "Breadcrumbs" } },
            { "MBreadcrumbsItem", new string[] { "Breadcrumbs" } },
            { "MButton", new string[] { "Buttons", "Button Groups", "Floating Action Buttons" } },
            { "MButtonGroup", new string[] { "Button groups", "Buttons" } },
            { "MCalendar", new string[] { "Calendars" } },
            { "MCalendarDaily", new string[] { "Calendars" } },
            { "MCalendarMonthly", new string[] { "Calendars" } },
            { "MCalendarWeekly", new string[] { "Calendars" } },
            { "MCard", new string[] { "Cards" } },
            { "MCardActions", new string[] { "Cards" } },
            { "MCardSubtitle", new string[] { "Cards" } },
            { "MCardText", new string[] { "Cards" } },
            { "MCardTitle", new string[] { "Cards" } },
            { "MCarousel", new string[] { "Carousels" } },
            { "MCarouselItem", new string[] { "Carousels" } },
            { "MCascader", new string[] { "Cascaders" } },
            { "MCascaderMenu", new string[] { "Cascaders" } },
            { "MCascaderSelectList", new string[] { "Cascaders" } },
            { "MCascaderSelectOption", new string[] { "Cascaders" } },
            { "MCheckbox", new string[] { "Checkboxes" } },
            { "MChip", new string[] { "Chips", "Chip groups" } },
            { "MChipGroup", new string[] { "Chip groups" } },
            { "MCol", new string[] { "Grid system" } },
            { "MColorPicker", new string[] { "Color pickers" } },
            { "MColorPickerCanvas", new string[] { "Color pickers" } },
            { "MColorPickerEdit", new string[] { "Color pickers" } },
            { "MColorPickerPreview", new string[] { "Color pickers" } },
            { "MContainer", new string[] { "Grid system" } },
            { "MDataFooter", new string[] { "Data iterators", "Data Tables" } },
            { "MDataIterator", new string[] { "Data iterators" } },
            { "MDataTable", new string[] { "Data tables" } },
            { "MDataTableHeader", new string[] { "Data tables" } },
            { "MDataTableRow", new string[] { "Data tables" } },
            { "MDataTableRowGroup", new string[] { "Data tables" } },
            { "MDatePicker", new string[] { "Date pickers", "Date pickers month" } },
            { "MDatePickerDateTable", new string[] { "Date pickers" } },
            { "MDatePickerHeader", new string[] { "Date pickers" } },
            { "MDatePickerMonthTable", new string[] { "Date pickers" } },
            { "MDatePickerTable", new string[] { "Date pickers" } },
            { "MDatePickerTitle", new string[] { "Date pickers" } },
            { "MDatePickerYears", new string[] { "Date pickers" } },
            { "MDialog", new string[] { "Dialogs" } },
            { "MDivider", new string[] { "Dividers" } },
            { "MDragZone", new string[] { "DragZone" } },
            { "MECharts", new string[] { "ECharts" } },
            { "MEditor", new string[] { "Editor" } },
            { "MErrorHandler", new string[] { "Error handler" } },
            { "MMessage", new string[] { "Presets" } },
            { "MExpansionPanel", new string[] { "Expansion panels" } },
            { "MExpansionPanelContent", new string[] { "Expansion panels" } },
            { "MExpansionPanelHeader", new string[] { "Expansion panels" } },
            { "MExpansionPanels", new string[] { "Expansion panels" } },
            { "MFileInput", new string[] { "File inputs" } },
            { "MFooter", new string[] { "Footers" } },
            { "MForm", new string[] { "Forms" } },
            { "MHover", new string[] { "Hover" } },
            { "MIcon", new string[] { "Icons" } },
            { "MImage", new string[] { "Images" } },
            { "MInput", new string[] { "Text fields" } },
            { "MOtpInput", new string[] { "OTP input" } },
            { "MItem", new string[] { "Item groups" } },
            { "MItemGroup", new string[] { "Item groups" } },
            { "MLabel", new string[] { "Text fields" } },
            { "MList", new string[] { "Lists" } },
            { "MListGroup", new string[] { "Lists", "List item groups" } },
            { "MListItem", new string[] { "Lists", "List item groups" } },
            { "MListItemAction", new string[] { "Lists", "List item groups" } },
            { "MListItemActionText", new string[] { "Lists", "List item groups" } },
            { "MListItemAvatar", new string[] { "Lists", "List item groups" } },
            { "MListItemContent", new string[] { "Lists", "List item groups" } },
            { "MListItemGroup", new string[] { "Lists", "List item groups" } },
            { "MListItemIcon", new string[] { "Lists" } },
            { "MListItemSubtitle", new string[] { "Lists", "List item groups" } },
            { "MListItemTitle", new string[] { "Lists", "List item groups" } },
            { "MMain", new string[] { "Application" } },
            { "MMarkdown", new string[] { "Markdown" } },
            { "MMenu", new string[] { "Menus" } },
            { "MMessages", new string[] { "Messages" } },
            { "MNavigationDrawer", new string[] { "Navigation drawers" } },
            { "MOverlay", new string[] { "Overlays" } },
            { "MPagination", new string[] { "Pagination" } },
            { "MProgressCircular", new string[] { "Progress circular" } },
            { "MProgressLinear", new string[] { "Progress linear" } },
            { "MRadio", new string[] { "Radio" } },
            { "MRadioGroup", new string[] { "Radio" } },
            { "MRangeSlider", new string[] { "Range sliders", "Sliders" } },
            { "MRating", new string[] { "Ratings" } },
            { "MResponsive", new string[] { "Aspect ratios" } },
            { "MRow", new string[] { "Grid system" } },
            { "MSelect", new string[] { "Selects" } },
            { "MSelectList", new string[] { "Selects" } },
            { "MSelectOption", new string[] { "Selects" } },
            { "MSheet", new string[] { "Sheets" } },
            { "MSimpleCheckbox", new string[] { "Checkboxes", "Data Tables" } },
            { "MSimpleTable", new string[] { "Simple tables" } },
            { "MSkeletonLoader", new string[] { "Skeleton loaders" } },
            { "MSlideGroup", new string[] { "Slide groups" } },
            { "MSlideItem", new string[] { "Slide groups" } },
            { "MSlider", new string[] { "Sliders", "Range sliders" } },
            { "MSnackbar", new string[] { "Snackbars" } },
            { "MSpacer", new string[] { "Grid system" } },
            { "MSpeedDial", new string[] { "Floating Action Buttons" } },
            { "MStepper", new string[] { "Steppers" } },
            { "MStepperContent", new string[] { "Steppers" } },
            { "MStepperHeader", new string[] { "Steppers" } },
            { "MStepperItems", new string[] { "Steppers" } },
            { "MStepperStep", new string[] { "Steppers" } },
            { "MSubheader", new string[] { "Subheaders" } },
            { "MSwitch", new string[] { "Switches" } },
            { "MSystemBar", new string[] { "System bars" } },
            { "MTab", new string[] { "Tabs" } },
            { "MTabItem", new string[] { "Tabs" } },
            { "MTable", new string[] { "Data tables" } },
            { "MTableCol", new string[] { "Data tables" } },
            { "MTableFooter", new string[] { "Data tables" } },
            { "MTableFooterButton", new string[] { "Data tables" } },
            { "MTableHeader", new string[] { "Data tables" } },
            { "MTableLoading", new string[] { "Data tables" } },
            { "MTabs", new string[] { "Tabs" } },
            { "MTabsBar", new string[] { "Tabs" } },
            { "MTabsItems", new string[] { "Tabs" } },
            { "MTabsSlider", new string[] { "Tabs" } },
            { "MTextarea", new string[] { "Textareas" } },
            { "MTextField", new string[] { "Text fields" } },
            { "MTimeline", new string[] { "Timelines" } },
            { "MTimelineItem", new string[] { "Timelines" } },
            { "MTimePicker", new string[] { "Time pickers" } },
            { "MTimePickerClock", new string[] { "Time pickers" } },
            { "MTimePickerTitle", new string[] { "Time pickers" } },
            { "MToolbar", new string[] { "Toolbars" } },
            { "MToolbarItems", new string[] { "Toolbars" } },
            { "MToolbarTitle", new string[] { "Toolbars" } },
            { "MTooltip", new string[] { "Tooltips" } },
            { "MTreeview", new string[] { "Treeview" } },
            { "MTreeviewNode", new string[] { "Treeview" } },
            { "MUpload", new string[] { "Uploads" } },
            { "MVirtualScroll", new string[] { "Virtual scroller" } },
            { "MWindow", new string[] { "Windows" } },
            { "MWindowItem", new string[] { "Windows" } },
            { "MAlert", new string[] { "Alerts" } },
            { "MAlertDismissButton", new string[] { "Alerts" } },
            { "MAlertIcon", new string[] { "Alerts" } },
            { "MApp", new string[] { "Application" } },
            { "MAppBar", new string[] { "App bars" } },
            { "MAppBarNavIcon", new string[] { "App bars" } },
            { "MAppBarTitle", new string[] { "App bars" } },
            { "MAutocomplete", new string[] { "Autocompletes" } },
        };

        public string Name => "api2json";

        public void Execute(CommandLineApplication command)
        {
            command.Description = "Generate json file for api";
            command.HelpOption();

            var assemblyDirArgument = command.Argument(
                "assembly Path", "[Required] The Path of assembly file.");

            var outputArgument = command.Argument(
                "output", "[Required] The directory where the json file to output");

            command.OnExecute(() =>
            {
                string assemblyPath = assemblyDirArgument.Value;
                string output = outputArgument.Value;

                if (string.IsNullOrEmpty(assemblyPath) || !File.Exists(assemblyPath))
                {
                    Console.WriteLine("Invalid assemblyPath.");
                    return 1;
                }

                if (string.IsNullOrEmpty(output))
                {
                    output = "./";
                }

                GenerateApiFiles(output, new[] { "zh-CN", "en-US" });

                return 0;
            });
        }

        void GenerateApiFiles(string output, string[] languages)
        {
            var assembly = typeof(MApp).Assembly; //Assembly.LoadFile(assemblyPath);
            var componentBaseType = typeof(ComponentBase);
            var componentTypes = assembly.GetTypes().Where(type => componentBaseType.IsAssignableFrom(type) && type.Name.StartsWith("M"));
            var apis = new List<Api>();

            foreach (var componentType in componentTypes)
            {
                var paramterProps = componentType.GetProperties().Where(prop =>
                    prop.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ParameterAttribute)));
                var contentProps = paramterProps.Where(prop => prop.PropertyType == typeof(RenderFragment) || (prop.PropertyType.IsGenericType &&
                    prop.PropertyType == typeof(RenderFragment<>).MakeGenericType(prop.PropertyType.GenericTypeArguments[0])));
                var eventProps = paramterProps.Where(prop => prop.PropertyType == typeof(EventCallback) || (prop.PropertyType.IsGenericType &&
                    prop.PropertyType == typeof(EventCallback<>).MakeGenericType(prop.PropertyType.GenericTypeArguments[0])));
                var defaultProps = paramterProps.Where(props =>
                    contentProps.Any(cprops => cprops == props) is false && eventProps.Any(eprops => eprops == props) is false);

                var title = GetComponentName(componentType);

                if (!_dic.ContainsKey(title))
                {
                    continue;
                }

                _dic.TryGetValue(title, out var components);

                var api = new Api
                {
                    Title = title,
                    Components = components,
                    Props = defaultProps.Where(prop => IgnoreProps(prop.Name)).Select(prop => new Prop
                    {
                        Name = prop.Name,
                        Type = GetType(prop.PropertyType),
                        Description = "",
                    }).OrderBy(p => p.Name).ToArray(),
                    Contents = contentProps.Select(prop => new Content
                    {
                        Name = prop.Name,
                        Type = GetType(prop.PropertyType),
                        Description = ""
                    }).OrderBy(content => content.Name).ToArray(),
                    Events = eventProps.Select(prop => new Event
                    {
                        Name = prop.Name,
                        Type = GetType(prop.PropertyType),
                        Description = ""
                    }).OrderBy(e => e.Name).ToArray(),
                };

                apis.Add(api);
            }

            foreach (var language in languages)
            {
                var files = Directory.GetFiles(output).Where(file => file.EndsWith($".{language}.json"));
                var jsonOption = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var basepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var propDescriptionMap = JsonSerializer
                    .Deserialize<string[][]>(File.ReadAllText($"{basepath}/ApiSettings/propDefaultDescription.{language}.json"))
                    .ToDictionary(prop => prop[0], prop => prop[1]);

                var contentDescriptionMap = JsonSerializer
                    .Deserialize<string[][]>(File.ReadAllText($"{basepath}/ApiSettings/contentDefaultDescription.{language}.json"))
                    .ToDictionary(prop => prop[0], prop => prop[1]);

                foreach (var api in apis)
                {
                    var file = files.FirstOrDefault(f => f.Contains($"{api.Title}.{language}.json"));

                    if (file is not null)
                    {
                        var oldApi = JsonSerializer.Deserialize<Api>(File.ReadAllText(file));
                        foreach (var prop in api.Props)
                        {
                            var oldProp = oldApi.Props.FirstOrDefault(p => p.Name == prop.Name);
                            if (oldProp is not null)
                            {
                                prop.Description = oldProp.Description;
                                prop.Default = oldProp.Default;
                            }
                        }
                    }
                    else file = $"{output}/{api.Title}.{language}.json";

                    foreach (var prop in api.Props)
                    {
                        if (propDescriptionMap.ContainsKey(prop.Name))
                        {
                            prop.Description = propDescriptionMap[prop.Name];
                        }
                    }

                    foreach (var content in api.Contents)
                    {
                        if (contentDescriptionMap.ContainsKey(content.Name))
                        {
                            content.Description = contentDescriptionMap[content.Name];
                        }
                    }

                    File.WriteAllText(file, JsonSerializer.Serialize(api, jsonOption), Encoding.UTF8);

                    //md
                    var mdFile = $"{output}/{api.Title}.{language}.md";
                    var mdContent = JsonSerializer.Deserialize<List<string>>(File.ReadAllText($"{basepath}/ApiSettings/mdContent.{language}.json"));
                    mdContent[mdContent.IndexOf("[title]")] = $"title: {api.Title}";
                    File.WriteAllLines(mdFile, mdContent);
                }
            }
        }

        string GetComponentName(Type componentType)
        {
            return componentType.IsGenericType
                ? componentType.Name.Remove(componentType.Name.IndexOf('`'))
                : componentType.Name;
        }

        bool IgnoreProps(string name)
        {
            return name != "Attributes" && name != "RefBack";
        }

        string GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                if (type == typeof(bool)) return "false";
                if (type == typeof(int)) return "0";
                else return "";
            }
            else return "null";
        }

        string GetType(Type type)
        {
            return type.IsGenericType
                ? type.Name.Split("`")[0] +
                  $"<{string.Join(",", type.GenericTypeArguments.Select(t => t.Name))}>"
                : type.Name;
        }
    }

    public class Api
    {
        public string Title { get; set; }

        public string[] Components { get; set; }

        public Prop[] Props { get; set; }

        public Content[] Contents { get; set; }

        public Event[] Events { get; set; }
    }

    public class Prop
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Default { get; set; }

        public string Description { get; set; }
    }

    public class Content
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }
    }

    public class Event
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }
    }
}