using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using GuiBlast.Forms.Model;

namespace GuiBlast.Forms.Rendering;

public static class DynamicFormUi
{
    // ---------- UI Builders ----------
    public static Control BuildFieldRow(FieldSpec f, object? initial,
        out Func<object?> getter, out TextBlock errorBlock)
    {
        var label = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(f.Label) ? f.Key : f.Label,
            Margin = new Thickness(0, 0, 0, 2)
        };

        Control input = BuildInput(f, initial, out getter);

        var help = string.IsNullOrWhiteSpace(f.Description)
            ? null
            : new TextBlock
            {
                Text = f.Description,
                Foreground = Brushes.Gray,
                FontSize = 12,
                Margin = new Thickness(0, 2, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

        errorBlock = new TextBlock
        {
            Foreground = Brushes.IndianRed,
            FontSize = 12,
            Margin = new Thickness(0, 2, 0, 0),
            IsVisible = false
        };

        var panel = new StackPanel { Spacing = 2 };
        panel.Children.Add(label);
        panel.Children.Add(input);
        if (help is not null) panel.Children.Add(help);
        panel.Children.Add(errorBlock);
        panel.Tag = f.Key; // for visibility toggling
        return panel;
    }

    private static Control BuildInput(FieldSpec f, object? initial, out Func<object?> getter)
    {
        switch (f.Type.ToLowerInvariant())
        {
            case "text":
            case "email":
            case "password":
            {
                var tb = new TextBox { Watermark = f.Placeholder ?? "" };
                if (f.Type == "password") tb.PasswordChar = '•';
                tb.Text = initial?.ToString() ?? "";
                getter = () => tb.Text;
                return tb;
            }

            case "textarea":
            {
                var tb = new TextBox { AcceptsReturn = true, MinHeight = 60, Watermark = f.Placeholder ?? "" };
                if (f.Rows is { } r and > 0) tb.MinHeight = 20 * r;
                tb.Text = initial?.ToString() ?? "";
                getter = () => tb.Text;
                return tb;
            }

            case "number":
            {
                var tb = new TextBox { Text = initial?.ToString() ?? "" };
                getter = () => double.TryParse(tb.Text, out var d) ? d : null;
                return tb;
            }

            case "checkbox":
            {
                var cb = new CheckBox { IsChecked = DynamicFormHelpers.ToBool(initial) };
                getter = () => cb.IsChecked ?? false;
                return cb;
            }

            case "switch":
            {
                var sw = new ToggleSwitch { IsChecked = DynamicFormHelpers.ToBool(initial) };
                getter = () => sw.IsChecked ?? false;
                return sw;
            }

            case "select":
            {
                var opts = DynamicFormHelpers.NormalizeOptions(f);
                var combo = new ComboBox
                {
                    ItemsSource = opts,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                var init = initial?.ToString() ?? "";
                combo.SelectedItem = opts.FirstOrDefault(o => o.Value == init) ?? opts.FirstOrDefault();
                getter = () => (combo.SelectedItem as Option)?.Value;
                return combo;
            }


            case "multiselect":
            {
                var opts = DynamicFormHelpers.NormalizeOptions(f);
                var list = new ListBox
                {
                    ItemsSource = opts,
                    SelectionMode = SelectionMode.Multiple,
                    Height = 120
                };

                HashSet<string>? wanted = null;
                if (initial is IEnumerable<object?> initSeq)
                    wanted = initSeq.Select(x => x?.ToString() ?? "").ToHashSet();

                if (wanted is { Count: > 0 })
                {
                    void OnTemplateApplied(object? _, TemplateAppliedEventArgs __)
                    {
                        // Run once
                        list.TemplateApplied -= OnTemplateApplied;

                        // SelectedItems should be non-null now, but guard to satisfy the compiler
                        var selected = list.SelectedItems;
                        if (selected is null) return;

                        foreach (var o in opts)
                            if (wanted.Contains(o.Value))
                                selected.Add(o);
                    }

                    list.TemplateApplied += OnTemplateApplied;
                }

                getter = () =>
                {
                    var sel = list.SelectedItems ?? Array.Empty<object>();
                    return sel.OfType<Option>().Select(o => o.Value).ToArray();
                };
                return list;
            }

            case "radio":
            {
                var opts = DynamicFormHelpers.NormalizeOptions(f);
                var sp = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
                foreach (var o in opts)
                {
                    var rb = new RadioButton { Content = o.Label, Tag = o.Value };
                    if ((initial?.ToString() ?? "") == o.Value) rb.IsChecked = true;
                    sp.Children.Add(rb);
                }

                getter = () =>
                {
                    foreach (var child in sp.Children.OfType<RadioButton>())
                        if (child.IsChecked == true)
                            return child.Tag?.ToString();
                    return null;
                };
                return sp;
            }

            case "slider":
            case "range":
            {
                var s = new Slider
                {
                    Minimum = f.Min ?? 0,
                    Maximum = f.Max ?? 100,
                    TickFrequency = f.Step ?? 1,
                    IsSnapToTickEnabled = true,
                    Value = DynamicFormHelpers.ConvertToDouble(initial) ?? (f.Min ?? 0)
                };
                getter = () => s.Value;
                return s;
            }

            case "date":
            {
                var dp = new DatePicker();
                if (DynamicFormHelpers.TryToDate(initial, out var dt))
                    dp.SelectedDate = new DateTimeOffset(dt.Date);
                getter = () => dp.SelectedDate?.DateTime;
                return dp;
            }

            case "time":
            {
                var tp = new TimePicker();
                if (DynamicFormHelpers.TryToTime(initial, out var ts)) tp.SelectedTime = ts;
                getter = () => tp.SelectedTime;
                return tp;
            }

            case "datetime":
            {
                var dp = new DatePicker();
                var tp = new TimePicker();
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                row.Children.Add(dp);
                row.Children.Add(tp);

                if (initial is DateTime dt)
                {
                    dp.SelectedDate = new DateTimeOffset(dt.Date);
                    tp.SelectedTime = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
                }

                getter = () =>
                {
                    if (dp.SelectedDate is null || tp.SelectedTime is null) return null;
                    var d = dp.SelectedDate.Value.Date;
                    var t = tp.SelectedTime.Value;
                    return new DateTime(d.Year, d.Month, d.Day, t.Hours, t.Minutes, t.Seconds, DateTimeKind.Local);
                };
                return row;
            }

            case "file":
            {
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
                var tb = new TextBox { IsReadOnly = true, Width = 320, Text = initial?.ToString() ?? "" };
                var btn = new Button { Content = "Browse…" };
                btn.Click += async (_, _) =>
                {
                    var top = TopLevel.GetTopLevel(AvaloniaHost.Owner);
                    var files = await top!.StorageProvider.OpenFilePickerAsync(
                        new Avalonia.Platform.Storage.FilePickerOpenOptions { AllowMultiple = false });

                    if (files.Count > 0)
                    {
                        var uri = files[0].Path; // Avalonia URI
                        var text =
                            uri.IsAbsoluteUri && uri.Scheme == Uri.UriSchemeFile
                                ? uri.LocalPath // real filesystem path
                                : uri.ToString(); // fallback (e.g., non-file providers)

                        tb.Text = text;
                    }
                };
                panel.Children.Add(tb);
                panel.Children.Add(btn);
                getter = () => tb.Text;
                return panel;
            }

            case "color":
            {
                var tb = new TextBox { Text = (initial?.ToString() ?? "#000000") };
                getter = () => tb.Text;
                return tb;
            }

            default:
            {
                var def = new TextBox { Text = initial?.ToString() ?? "" };
                getter = () => def.Text;
                return def;
            }
        }
    }

    public static void WireChanges(Control c, Action onChange)
    {
        switch (c)
        {
            case TextBox tb:
                tb.PropertyChanged += (_, e) =>
                {
                    if (e.Property == TextBox.TextProperty) onChange();
                };
                break;
            case CheckBox cb:
                cb.PropertyChanged += (_, e) =>
                {
                    if (e.Property == ToggleButton.IsCheckedProperty) onChange();
                };
                break;
            case ToggleSwitch ts:
                ts.PropertyChanged += (_, e) =>
                {
                    if (e.Property == ToggleButton.IsCheckedProperty) onChange();
                };
                break;
            case ComboBox combo:
                combo.SelectionChanged += (_, _) => onChange();
                break;
            case ListBox list:
                list.SelectionChanged += (_, _) => onChange();
                break;
            case DatePicker dp:
                dp.PropertyChanged += (_, e) =>
                {
                    if (e.Property == DatePicker.SelectedDateProperty) onChange();
                };
                break;
            case TimePicker tp:
                tp.PropertyChanged += (_, e) =>
                {
                    if (e.Property == TimePicker.SelectedTimeProperty) onChange();
                };
                break;
            case Panel panel:
                foreach (var child in panel.Children.OfType<Control>())
                    WireChanges(child, onChange);
                break;
        }
    }

    public static (Button ok, Button cancel) BuildButtons(List<ActionSpec>? actions)
    {
        // Defaults if no actions provided
        if (actions == null || actions.Count == 0)
            return (new Button { Content = "OK", MinWidth = 80 }, new Button { Content = "Cancel", MinWidth = 80 });

        Button? ok = null;
        Button? cancel = null;

        foreach (var a in actions)
        {
            var b = new Button { Content = a.Label, MinWidth = 80 };
            if (a.Submit || a.Primary) ok ??= b;
            if (a.Dismiss) cancel ??= b;
        }

        ok ??= new Button { Content = "OK", MinWidth = 80 };
        cancel ??= new Button { Content = "Cancel", MinWidth = 80 };
        return (ok, cancel);
    }
}