using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using GuiBlast.Forms.Model;
using GuiBlast.Forms.Result;

namespace GuiBlast.Forms.Rendering;

/// <summary>
/// Renders modal forms from JSON or a <see cref="FormSpec"/> and returns a <see cref="FormResult"/>.
/// </summary>
public static class DynamicForm
{
    private static readonly JsonSerializerOptions CachedJsonOptions = CreateOptions();

    /// <summary>
    /// Renders a modal form from a JSON <see cref="FormSpec"/> and returns the result.
    /// </summary>
    /// <param name="json">JSON text that deserializes to a <see cref="FormSpec"/>.</param>
    /// <param name="dataOverrides">Optional key/value overrides merged into the spec’s <c>data</c> block.</param>
    /// <param name="visibilityContext">
    /// Optional read-only values used only for rule evaluation (not included in the returned result).
    /// </param>
    /// <param name="width">Optional dialog width in device-independent pixels.</param>
    /// <param name="height">Optional dialog height in device-independent pixels.</param>
    /// <param name="canResize">When <c>true</c>, the user can resize the dialog.</param>
    /// <returns>A task that completes with the <see cref="FormResult"/> after submit or cancel.</returns>
    /// <exception cref="ArgumentException">The JSON could not be deserialized into a valid <see cref="FormSpec"/>.</exception>
    /// <exception cref="JsonException">The JSON is invalid or has an unexpected shape.</exception>
    public static Task<FormResult> ShowJsonAsync(
        string json,
        IDictionary<string, object?>? dataOverrides = null,
        IReadOnlyDictionary<string, object?>? visibilityContext = null,
        double? width = null,
        double? height = null,
        bool canResize = false)
    {
        var opts = CachedJsonOptions;
        var spec = JsonSerializer.Deserialize<FormSpec>(json, opts)
                   ?? throw new ArgumentException("Invalid JSON spec.");

        if (dataOverrides is { Count: > 0 })
        {
            spec.Data ??= new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            foreach (var kv in dataOverrides)
                spec.Data[kv.Key] = DynamicFormHelpers.SerializeToElementRelaxed(kv.Value);
        }

        return ShowAsync(spec, visibilityContext ?? new Dictionary<string, object?>(), width, height, canResize);
    }

    /// <summary>
    /// Displays a dynamic form based on a <see cref="FormSpec"/>.
    /// </summary>
    /// <param name="spec">The form specification object.</param>
    /// <param name="visibilityContext">
    /// Optional read-only values used only for rule evaluation (not included in the returned result).
    /// </param>
    /// <param name="width">Optional window width.</param>
    /// <param name="height">Optional window height.</param>
    /// <param name="canResize">If <c>true</c>, the user can resize the window.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that completes with the <see cref="FormResult"/>
    /// after the user submits or cancels the form.
    /// </returns>
    private static Task<FormResult> ShowAsync(
        FormSpec spec,
        IReadOnlyDictionary<string, object?> visibilityContext,
        double? width = null,
        double? height = null,
        bool canResize = false)
        => AvaloniaHost.RunOnUI(async () =>
        {
            var dictionary = (spec.Data ?? new Dictionary<string, JsonElement>()).ToDictionary(pair => pair.Key,
                pair => DynamicFormHelpers.FromJson(pair.Value));
            var model = new Dictionary<string, object?>(
                dictionary,
                StringComparer.Ordinal);

            // read-only merged view for rule evaluation (context overrides model)
            IReadOnlyDictionary<string, object?> eval =
                (visibilityContext is { Count: > 0 })
                    ? new CompositeReadOnly(visibilityContext, model)
                    : model;

            var fieldContainers = new Dictionary<string, Control>(StringComparer.Ordinal);
            var inputGetters = new Dictionary<string, Func<object?>>(StringComparer.Ordinal);
            var errorBlocks = new Dictionary<string, TextBlock>(StringComparer.Ordinal);

            var root = new StackPanel { Margin = new Thickness(12), Spacing = 8 };

            foreach (var f in spec.Fields)
            {
                var initial = model.TryGetValue(f.Key, out var v) ? v : f.Default;
                var row = DynamicFormUi.BuildFieldRow(f, initial, out var getter, out var errorBlock);
                model[f.Key] = initial;
                
                DynamicFormUi.WireChanges(row, () =>
                {
                    model[f.Key] = getter();
                    // evaluate rules against the merged view
                    DynamicFormVisibility.ApplyVisibility(spec.Visibility, eval, fieldContainers);
                    DynamicFormOptionVisibility.ApplyOptionFilters(spec.Visibility, eval, fieldContainers);
                });

                fieldContainers[f.Key] = row;
                inputGetters[f.Key] = getter;
                errorBlocks[f.Key] = errorBlock;
                root.Children.Add(row);
            }

            // Initial application of rules (against merged view)
            DynamicFormVisibility.ApplyVisibility(spec.Visibility, eval, fieldContainers);
            DynamicFormOptionVisibility.ApplyOptionFilters(spec.Visibility, eval, fieldContainers);

            var (okBtn, cancelBtn) = DynamicFormUi.BuildButtons(spec.Actions);
            root.Children.Add(UiHelpers.ButtonRow(cancelBtn, okBtn));

            var w = UiHelpers.NewDialog(spec.Title,
                new ScrollViewer { Content = root, VerticalScrollBarVisibility = ScrollBarVisibility.Auto },
                width ?? spec.Size?.Width, height ?? spec.Size?.Height, canResize);
            var tcs = new TaskCompletionSource<FormResult>();

            okBtn.IsDefault = true;
            cancelBtn.IsCancel = true;

            okBtn.Click += async (_, _) => await CompleteAsync(true);
            cancelBtn.Click += async (_, _) => await CompleteAsync(false);

            await w.ShowDialog(AvaloniaHost.Owner);
            return await tcs.Task;

            async Task CompleteAsync(bool submitted)
            {
                foreach (var kv in inputGetters)
                    model[kv.Key] = kv.Value();

                if (submitted)
                {
                    var valid = DynamicFormValidation.ValidateAll(spec, model, errorBlocks);
                    if (!valid) return;
                }

                // ONLY return user data (no visibilityContext leaked)
                tcs.TrySetResult(new FormResult { Submitted = submitted, Values = model });
                await Task.Yield();
                w.Close();
            }
        });

    /// <summary>
    /// Creates default JSON serialization options for parsing form specifications.
    /// </summary>
    private static JsonSerializerOptions CreateOptions()
    {
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        opts.Converters.Add(new OptionJsonConverter());
        return opts;
    }

    private sealed class CompositeReadOnly(
        IReadOnlyDictionary<string, object?> a,
        IReadOnlyDictionary<string, object?> b)
        : IReadOnlyDictionary<string, object?>
    {
        public bool ContainsKey(string key) => a.ContainsKey(key) || b.ContainsKey(key);
        public IEnumerable<string> Keys => a.Keys.Concat(b.Keys).Distinct(StringComparer.Ordinal);
        public IEnumerable<object?> Values => Keys.Select(k => this[k]);
        public int Count => Keys.Count();

        public bool TryGetValue(string key, out object? value)
        {
            return a.TryGetValue(key, out value) || b.TryGetValue(key, out value);
        }

        public object? this[string key] => a.TryGetValue(key, out var v) ? v : b[key];

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() =>
            Keys.Select(k => new KeyValuePair<string, object?>(k, this[k])).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
