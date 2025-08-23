using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using GuiBlast.Forms.Model;

namespace GuiBlast.Forms.Rendering;

/// <summary>
/// Applies option-level filtering (based on <see cref="VisibilityRule.Options"/>)
/// to inputs like select, multiselect, and radio.
/// </summary>
public static class DynamicFormOptionVisibility
{
    /// <summary>
    /// Applies option filtering rules to all relevant input controls.
    /// Safe to call repeatedly (e.g., on every change).
    /// </summary>
    /// <param name="rules">Visibility rules (can be null).</param>
    /// <param name="model">Current form model.</param>
    /// <param name="containers">Field key -> container control (as built in UI).</param>
    public static void ApplyOptionFilters(
        List<VisibilityRule>? rules,
        IReadOnlyDictionary<string, object?> model,
        IReadOnlyDictionary<string, Control> containers)
    {
        if (rules is null || rules.Count == 0) return;

        // 1) Aggregate include/exclude tags per target field, considering rule conditions
        var perField = new Dictionary<string, (HashSet<string>? include, HashSet<string> exclude)>(StringComparer.Ordinal);

        foreach (var r in rules)
        {
            if (r.Options is null) continue;

            if (!string.IsNullOrWhiteSpace(r.Field))
            {
                // Evaluate rule condition
                model.TryGetValue(r.Field, out var v);
                var s = v?.ToString();

                var eqOk  = r.Eq  is not null && string.Equals(s, r.Eq, StringComparison.Ordinal);
                var neqOk = r.Neq is not null && !string.Equals(s, r.Neq, StringComparison.Ordinal);

                // If neither eq nor neq specified, treat as "no condition"
                var hasCond = r.Eq is not null || r.Neq is not null;
                var cond = !hasCond || (eqOk || neqOk);
                if (!cond) continue;
            }
            // else: no Field => always apply

            var target = r.Options.For;
            if (string.IsNullOrWhiteSpace(target)) continue;

            if (!perField.TryGetValue(target, out var agg))
                perField[target] = agg = (null, new HashSet<string>(StringComparer.Ordinal));

            if (r.Options.IncludeTags is { Length: > 0 })
            {
                agg.include ??= new HashSet<string>(StringComparer.Ordinal);
                foreach (var t in r.Options.IncludeTags) agg.include.Add(t);
            }

            if (r.Options.ExcludeTags is { Length: > 0 })
            {
                foreach (var t in r.Options.ExcludeTags) agg.exclude.Add(t);
            }

            perField[target] = agg;
        }

        if (perField.Count == 0) return;

        // 2) For each affected field, locate its input control and re-filter items
        foreach (var (fieldKey, (include, exclude)) in perField)
        {
            if (!containers.TryGetValue(fieldKey, out var container)) continue;

            // Input is the 2nd child in the field row (label, input, help?, error)
            if (container is Panel { Children: [_, { } input, ..] })
            {
                // We stashed the original List<Option> in input.Tag earlier
                if (input.Tag is not List<Option> original || original.Count == 0) continue;

                // Build the filtered list
                var filtered = FilterOptions(original, include, exclude);

                switch (input)
                {
                    case ComboBox combo:
                    {
                        var selectedVal = (combo.SelectedItem as Option)?.Value;

                        Dispatcher.UIThread.Post(() =>
                        {
                            combo.Resources["__updatingOptions"] = true;
                            try
                            {
                                combo.ItemsSource = filtered;
                                combo.SelectedItem = filtered.FirstOrDefault(o => o.Value == selectedVal)
                                                     ?? filtered.FirstOrDefault();
                            }
                            finally
                            {
                                combo.Resources.Remove("__updatingOptions");
                            }
                        }, DispatcherPriority.Background);
                        break;
                    }

                    case ListBox list:
                    {
                        var selectedVals = (list.SelectedItems ?? Array.Empty<object>())
                            .OfType<Option>()
                            .Select(o => o.Value)
                            .ToHashSet(StringComparer.Ordinal);

                        Dispatcher.UIThread.Post(() =>
                        {
                            list.Resources["__updatingOptions"] = true;
                            try
                            {
                                list.ItemsSource = filtered;

                                void OnTemplateApplied(object? _, TemplateAppliedEventArgs __)
                                {
                                    list.TemplateApplied -= OnTemplateApplied;
                                    var sel = list.SelectedItems;
                                    if (sel is null) return;

                                    foreach (var o in filtered)
                                        if (selectedVals.Contains(o.Value))
                                            sel.Add(o);
                                }
                                list.TemplateApplied += OnTemplateApplied;
                            }
                            finally
                            {
                                list.Resources.Remove("__updatingOptions");
                            }
                        }, DispatcherPriority.Background);
                        break;
                    }

                    case StackPanel sp: // radio group
                    {
                        // Preserve checked radio by value
                        string? selectedVal = null;
                        foreach (var rb in sp.Children.OfType<RadioButton>())
                            if (rb.IsChecked == true)
                                selectedVal = rb.Tag?.ToString();

                        sp.Children.Clear();
                        foreach (var o in filtered)
                        {
                            var rb = new RadioButton { Content = o.Label, Tag = o.Value };
                            if (o.Value == selectedVal) rb.IsChecked = true;
                            sp.Children.Add(rb);
                        }
                        break;
                    }
                }
            }
        }
    }

    private static List<Option> FilterOptions(
        List<Option> source,
        HashSet<string>? include,
        HashSet<string> exclude)
    {
        bool HasAnyTag(Option o, HashSet<string> tags)
            => o.Tags is { Length: > 0 } && o.Tags.Any(tags.Contains);

        IEnumerable<Option> q = source;

        if (include is not null && include.Count > 0)
            q = q.Where(o => HasAnyTag(o, include));

        if (exclude.Count > 0)
            q = q.Where(o => !(o.Tags is { Length: > 0 } && o.Tags.Any(exclude.Contains)));

        // If include was set but nothing matched, you’ll get an empty list (by design)
        return q.ToList();
    }
}