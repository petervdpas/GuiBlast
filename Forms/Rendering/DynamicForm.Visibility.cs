using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using GuiBlast.Forms.Model;

namespace GuiBlast.Forms.Rendering;

/// <summary>
/// Applies <see cref="VisibilityRule"/> logic to field containers:
/// - key-based show/hide (<see cref="VisibilityRule.Show"/> / <see cref="VisibilityRule.Hide"/>)
/// - tag-based show/hide (<see cref="VisibilityRule.ShowTags"/> / <see cref="VisibilityRule.HideTags"/>)
/// Hide always wins when conflicts occur.
/// </summary>
public static class DynamicFormVisibility
{
    /// <summary>
    /// Applies visibility rules to form field containers.
    /// </summary>
    /// <param name="rules">Visibility rules to evaluate (nullable).</param>
    /// <param name="model">Current form model (field key → value).</param>
    /// <param name="containers">Field key → UI container (container.Tag may carry field tags as string[]).</param>
    /// <remarks>
    /// All containers are reset to visible before rules are applied.
    /// For conflicting outcomes from multiple rules, <c>hide</c> takes precedence over <c>show</c>.
    /// For key/tag show/hide, a controller <see cref="VisibilityRule.Field"/>
    /// with <see cref="VisibilityRule.Eq"/> and/or <see cref="VisibilityRule.Neq"/> is required;
    /// rules without a condition are ignored here (but may still drive <c>Options</c> elsewhere).
    /// </remarks>
    public static void ApplyVisibility(
        List<VisibilityRule>? rules,
        IReadOnlyDictionary<string, object?> model,
        IReadOnlyDictionary<string, Control> containers)
    {
        // Reset all to visible
        foreach (var c in containers.Values)
            c.IsVisible = true;

        if (rules is null || rules.Count == 0)
            return;

        var show = new HashSet<string>(StringComparer.Ordinal);
        var hide = new HashSet<string>(StringComparer.Ordinal);

        foreach (var r in from r in rules
                 let hasController = !string.IsNullOrWhiteSpace(r.Field) && (r.Eq is not null || r.Neq is not null)
                 where hasController
                 select r)
        {
            model.TryGetValue(r.Field, out var v);
            var s = v?.ToString();

            var eqOk = r.Eq is not null && string.Equals(s, r.Eq, StringComparison.Ordinal);
            var neqOk = r.Neq is not null && !string.Equals(s, r.Neq, StringComparison.Ordinal);
            if (!(eqOk || neqOk)) continue;

            // Key-based show/hide
            if (r.Show is { Length: > 0 })
                foreach (var k in r.Show)
                    show.Add(k);

            if (r.Hide is { Length: > 0 })
                foreach (var k in r.Hide)
                    hide.Add(k);

            // Tag-based show/hide
            if (r.ShowTags is { Length: > 0 } || r.HideTags is { Length: > 0 })
            {
                var showTagSet = r.ShowTags is { Length: > 0 }
                    ? new HashSet<string>(r.ShowTags, StringComparer.Ordinal)
                    : null;
                var hideTagSet = r.HideTags is { Length: > 0 }
                    ? new HashSet<string>(r.HideTags, StringComparer.Ordinal)
                    : null;

                foreach (var (key, container) in containers)
                {
                    // Expect tags stored on the container (e.g., row.Tag = fieldSpec.Tags)
                    var tags = container.Tag as string[] ?? [];

                    if (showTagSet is not null && HasAny(tags, showTagSet))
                        show.Add(key);

                    if (hideTagSet is not null && HasAny(tags, hideTagSet))
                        hide.Add(key);
                }
            }
        }

        // Hide takes precedence
        foreach (var k in show)
            if (!hide.Contains(k) && containers.TryGetValue(k, out var c1))
                c1.IsVisible = true;

        foreach (var k in hide)
            if (containers.TryGetValue(k, out var c2))
                c2.IsVisible = false;
    }

    private static bool HasAny(string[] tags, HashSet<string> set)
    {
        return tags.Any(set.Contains);
    }
}
