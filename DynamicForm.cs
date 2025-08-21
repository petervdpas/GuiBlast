using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Primitives;

namespace GuiBlast
{
    public sealed class FormResult
    {
        public bool Submitted { get; init; }
        public Dictionary<string, object?> Values { get; init; } = new();
        public object? this[string key] => Values.GetValueOrDefault(key);
    }

    // ---------- Spec DTOs ----------
    public sealed class FormSpec
    {
        public string? Id { get; init; }
        public string Title { get; init; } = "Form";
        public SizeSpec? Size { get; init; }
        public Dictionary<string, JsonElement>? Data { get; set; }
        public List<FieldSpec> Fields { get; init; } = [];
        public List<VisibilityRule>? Visibility { get; init; }
        public Dictionary<string, ValidationRule>? Validation { get; init; }
        public List<ActionSpec>? Actions { get; init; }
    }

    public sealed class SizeSpec { public double? Width { get; set; } public double? Height { get; set; } }

    public sealed class FieldSpec
    {
        public string Key { get; set; } = "";
        // text, number, textarea, password, email, select, multiselect, checkbox, switch, radio, slider, range, date, time, datetime, file, color
        public string Type { get; set; } = "text";
        public string? Label { get; set; }
        public string? Placeholder { get; set; }
        public bool? Required { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? Step { get; set; }
        public int? Rows { get; set; }
        public string? Pattern { get; set; }
        public bool? Email { get; set; }
        public List<Option>? Options { get; set; } // for select/radio
        public string? Description { get; set; }
        public object? Default { get; set; }
    }

    public sealed class Option
    {
        [JsonConstructor]
        public Option(string? value = null, string? label = null)
        {
            Value = value ?? label ?? "";
            Label = label ?? value ?? "";
        }
        public string Value { get; set; } = "";
        public string Label { get; set; } = "";

        public override string ToString() => Label; // <- important
    }

    public sealed class ActionSpec
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = "";
        public bool Primary { get; set; }
        public bool Submit { get; set; }
        public bool Dismiss { get; set; }
    }

    public sealed class ValidationRule
    {
        public bool? Required { get; set; }
        public int? MinLen { get; set; }
        public int? MaxLen { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public string? Regex { get; set; }
        public bool? Email { get; set; }
    }

    public sealed class VisibilityRule
    {
        public string Field { get; set; } = "";
        public string? Eq { get; set; }
        public string? Neq { get; set; }
        public string[]? Show { get; set; }
        public string[]? Hide { get; set; }
    }

    // ---------- Renderer ----------
    public static partial class DynamicForm
    {
        public static Task<FormResult> ShowJsonAsync(
            string json,
            IDictionary<string, object?>? dataOverrides,
            double? width = null, double? height = null, bool canResize = false)
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            opts.Converters.Add(new OptionJsonConverter());

            var spec = JsonSerializer.Deserialize<FormSpec>(json, opts)
                       ?? throw new ArgumentException("Invalid JSON spec.");

            if (dataOverrides is { Count: > 0 })
            {
                spec.Data ??= new Dictionary<string, JsonElement>(StringComparer.Ordinal);
                foreach (var kv in dataOverrides)
                    spec.Data[kv.Key] = SerializeToElementRelaxed(kv.Value);
            }

            return ShowAsync(spec, width, height, canResize);
        }

        public static Task<FormResult> ShowAsync(FormSpec spec,
            double? width = null, double? height = null, bool canResize = false)
        => AvaloniaHost.RunOnUI(async () =>
        {
            var model = new Dictionary<string, object?>(
                (spec.Data ?? new()).ToDictionary(kv => kv.Key, kv => FromJson(kv.Value)));

            // UI bookkeeping
            var fieldContainers = new Dictionary<string, Control>();
            var inputGetters = new Dictionary<string, Func<object?>>();
            var errorBlocks = new Dictionary<string, TextBlock>();

            // Layout
            var root = new StackPanel { Margin = new Thickness(12), Spacing = 8 };

            // Build fields
            foreach (var f in spec.Fields)
            {
                var initial = model.TryGetValue(f.Key, out var v) ? v : f.Default;
                var row = BuildFieldRow(f, initial, out var getter, out var errorBlock);
                WireChanges(row, () =>
                {
                    model[f.Key] = getter();
                    ApplyVisibility(spec.Visibility, model, fieldContainers);
                });
                fieldContainers[f.Key] = row;
                inputGetters[f.Key] = getter;
                errorBlocks[f.Key] = errorBlock;

                root.Children.Add(row);
            }

            // Initial visibility
            ApplyVisibility(spec.Visibility, model, fieldContainers);

            // Buttons
            var (okBtn, cancelBtn) = BuildButtons(spec.Actions);
            root.Children.Add(UiHelpers.ButtonRow(cancelBtn, okBtn));

            // Window
            var w = UiHelpers.NewDialog(spec.Title, new ScrollViewer
            {
                Content = root,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            }, width ?? spec.Size?.Width, height ?? spec.Size?.Height, canResize);

            var tcs = new TaskCompletionSource<FormResult>();

            okBtn.Click += (_, _) => Complete(true);
            cancelBtn.Click += (_, _) => Complete(false);

            // Default buttons behavior
            okBtn.IsDefault = true;
            cancelBtn.IsCancel = true;

            await w.ShowDialog(AvaloniaHost.Owner);
            return await tcs.Task;

            async void Complete(bool submitted)
            {
                // Refresh model from getters before submit/cancel
                foreach (var kv in inputGetters)
                    model[kv.Key] = kv.Value();

                if (submitted)
                {
                    var valid = ValidateAll(spec, model, errorBlocks);
                    if (!valid) return; // keep the dialog open
                }

                tcs.TrySetResult(new FormResult { Submitted = submitted, Values = model });
                await Task.Yield();
                w.Close();
            }
        });

        // ---------- UI Builders ----------
        private static Control BuildFieldRow(FieldSpec f, object? initial,
            out Func<object?> getter, out TextBlock errorBlock)
        {
            var label = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(f.Label) ? f.Key : f.Label,
                Margin = new Thickness(0, 0, 0, 2)
            };

            Control input = BuildInput(f, initial, out getter);

            var help = string.IsNullOrWhiteSpace(f.Description) ? null :
                new TextBlock
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
                        var cb = new CheckBox { IsChecked = ToBool(initial) };
                        getter = () => cb.IsChecked ?? false;
                        return cb;
                    }

                case "switch":
                    {
                        var sw = new ToggleSwitch { IsChecked = ToBool(initial) };
                        getter = () => sw.IsChecked ?? false;
                        return sw;
                    }

                case "select":
                    {
                        var opts = NormalizeOptions(f);
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
                        var opts = NormalizeOptions(f);
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
                        var opts = NormalizeOptions(f);
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
                                if (child.IsChecked == true) return child.Tag?.ToString();
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
                            Value = ConvertToDouble(initial) ?? (f.Min ?? 0)
                        };
                        getter = () => s.Value;
                        return s;
                    }

                case "date":
                    {
                        var dp = new DatePicker();
                        if (TryToDate(initial, out var dt))
                            dp.SelectedDate = new DateTimeOffset(dt.Date);
                        getter = () => dp.SelectedDate?.DateTime;
                        return dp;
                    }

                case "time":
                    {
                        var tp = new TimePicker();
                        if (TryToTime(initial, out var ts)) tp.SelectedTime = ts;
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
                                        ? uri.LocalPath           // real filesystem path
                                        : uri.ToString();         // fallback (e.g., non-file providers)

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

        private static void WireChanges(Control c, Action onChange)
        {
            switch (c)
            {
                case TextBox tb:
                    tb.PropertyChanged += (_, e) => { if (e.Property == TextBox.TextProperty) onChange(); };
                    break;
                case CheckBox cb:
                    cb.PropertyChanged += (_, e) => { if (e.Property == ToggleButton.IsCheckedProperty) onChange(); };
                    break;
                case ToggleSwitch ts:
                    ts.PropertyChanged += (_, e) => { if (e.Property == ToggleButton.IsCheckedProperty) onChange(); };
                    break;
                case ComboBox combo:
                    combo.SelectionChanged += (_, _) => onChange();
                    break;
                case ListBox list:
                    list.SelectionChanged += (_, _) => onChange();
                    break;
                case DatePicker dp:
                    dp.PropertyChanged += (_, e) => { if (e.Property == DatePicker.SelectedDateProperty) onChange(); };
                    break;
                case TimePicker tp:
                    tp.PropertyChanged += (_, e) => { if (e.Property == TimePicker.SelectedTimeProperty) onChange(); };
                    break;
                case Panel panel:
                    foreach (var child in panel.Children.OfType<Control>())
                        WireChanges(child, onChange);
                    break;
            }
        }

        private static (Button ok, Button cancel) BuildButtons(List<ActionSpec>? actions)
        {
            // Defaults if no actions provided
            if (actions == null || actions.Count == 0)
                return (new Button { Content = "OK", MinWidth = 80 }, new Button { Content = "Cancel", MinWidth = 80 });

            Button? ok = null; Button? cancel = null;

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

        // ---------- Visibility & Validation ----------
        private static void ApplyVisibility(List<VisibilityRule>? rules,
            Dictionary<string, object?> model,
            Dictionary<string, Control> containers)
        {
            foreach (var c in containers.Values) c.IsVisible = true; // reset

            if (rules is null) return;

            var show = new HashSet<string>(StringComparer.Ordinal);
            var hide = new HashSet<string>(StringComparer.Ordinal);

            foreach (var r in rules)
            {
                var val = model.TryGetValue(r.Field, out var v) ? v?.ToString() : null;
                var cond = (r.Eq is not null && string.Equals(val, r.Eq, StringComparison.Ordinal)) ||
                           (r.Neq is not null && !string.Equals(val, r.Neq, StringComparison.Ordinal));
                if (!cond) continue;

                if (r.Show is not null) foreach (var k in r.Show) show.Add(k);
                if (r.Hide is not null) foreach (var k in r.Hide) hide.Add(k);
            }

            // Hide takes precedence
            foreach (var k in show) if (!hide.Contains(k) && containers.TryGetValue(k, out var c1)) c1.IsVisible = true;
            foreach (var k in hide) if (containers.TryGetValue(k, out var c2)) c2.IsVisible = false;
        }

        private static bool ValidateAll(FormSpec spec, Dictionary<string, object?> model,
            Dictionary<string, TextBlock> errorBlocks)
        {
            bool ok = true;

            // Clear previous errors
            foreach (var eb in errorBlocks.Values) { eb.IsVisible = false; eb.Text = ""; }

            foreach (var f in spec.Fields)
            {
                if (!errorBlocks.TryGetValue(f.Key, out var err)) continue;
                var v = model.GetValueOrDefault(f.Key);

                var msg = ValidateField(f, spec.Validation, v);
                
                if (string.IsNullOrEmpty(msg)) continue;
                
                err.Text = msg;
                err.IsVisible = true;
                ok = false;
            }
            return ok;
        }

        private static string? ValidateField(FieldSpec f, Dictionary<string, ValidationRule>? all, object? v)
        {
            // Merge inline flags + named rules
            var rule = (all != null && all.TryGetValue(f.Key, out var vr)) ? vr : new ValidationRule();
            if (f.Required == true && rule.Required is null) rule.Required = true;
            if (f.Min is not null && rule.Min is null) rule.Min = f.Min;
            if (f.Max is not null && rule.Max is null) rule.Max = f.Max;
            if (!string.IsNullOrWhiteSpace(f.Pattern) && string.IsNullOrWhiteSpace(rule.Regex)) rule.Regex = f.Pattern;
            if (f.Email == true && rule.Email is null) rule.Email = true;

            // Required
            if (rule.Required == true)
            {
                switch (v)
                {
                    case null:
                    case string s when string.IsNullOrWhiteSpace(s):
                    case Array { Length: 0 }:
                        return "Required.";
                }
            }

            // Length
            if (v is string sv)
            {
                if (rule.MinLen is { } minL && sv.Length < minL) return $"Min length: {minL}.";
                if (rule.MaxLen is { } maxL && sv.Length > maxL) return $"Max length: {maxL}.";
            }

            // Numeric
            var vd = ConvertToDouble(v);
            if (vd is not null)
            {
                if (rule.Min is { } min && vd < min) return $"Min value: {min}.";
                if (rule.Max is { } max && vd > max) return $"Max value: {max}.";
            }

            // Regex
            if (!string.IsNullOrWhiteSpace(rule.Regex) && v is string rxVal)
            {
                if (!Regex.IsMatch(rxVal, rule.Regex)) return "Invalid format.";
            }

            // Email
            if (rule.Email == true && v is string { Length: > 0 } ev)
            {
                // lightweight check
                if (!MyRegex().IsMatch(ev)) return "Invalid email.";
            }

            return null;
        }

        // ---------- Helpers ----------
        private static List<Option> NormalizeOptions(FieldSpec f)
        {
            var list = new List<Option>();
            if (f.Options is { Count: > 0 }) list.AddRange(f.Options);
            return list.Count > 0 ? list : [new Option("", "(none)")];
        }

        private static object? FromJson(JsonElement e) => e.ValueKind switch
        {
            JsonValueKind.String => e.GetString(),
            JsonValueKind.Number => e.TryGetInt64(out var i) ? i : (object?)e.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => e.EnumerateArray().Select(FromJson).ToArray(),
            JsonValueKind.Null => null,
            _ => e.ToString()
        };

        private static bool ToBool(object? v)
            => v is bool b ? b : (v is string s && bool.TryParse(s, out var bb) && bb);

        private static double? ConvertToDouble(object? v)
        {
            return v switch
            {
                null => null,
                double d => d,
                float f => f,
                int i => i,
                long l => l,
                string s when double.TryParse(s, out var ds) => ds,
                _ => null
            };
        }

        private static bool TryToDate(object? v, out DateTime dt)
        {
            dt = default;
            switch (v)
            {
                case DateTime x:
                    dt = x; return true;
                case string s when DateTime.TryParse(s, out var p):
                    dt = p; return true;
                default:
                    return false;
            }
        }

        private static bool TryToTime(object? v, out TimeSpan ts)
        {
            ts = TimeSpan.Zero;
            switch (v)
            {
                case TimeSpan t:
                    ts = t; return true;
                case string s when TimeSpan.TryParse(s, out var p):
                    ts = p; return true;
                default:
                    return false;
            }
        }

        private static JsonElement SerializeToElementRelaxed(object? value)
        {
            return value switch
            {
                // Already a JsonElement?
                JsonElement je => je,
                // Normalize types System.Text.Json does not love by default
                TimeSpan ts => JsonSerializer.SerializeToElement(ts.ToString("c")),
                DateTime dt => JsonSerializer.SerializeToElement(dt),
                DateTimeOffset dto => JsonSerializer.SerializeToElement(dto),
                _ => JsonSerializer.SerializeToElement(value)
            };

            // Everything else
        }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex MyRegex();
    }
}
