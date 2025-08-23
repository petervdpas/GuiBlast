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
/// Provides the main API for rendering dynamic forms from JSON specifications
/// or <see cref="FormSpec"/> objects.
/// </summary>
public static class DynamicForm
{
    private static readonly JsonSerializerOptions CachedJsonOptions = CreateOptions();

    /// <summary>
    /// Renders a dynamic form from a JSON specification and returns the user’s input as a <see cref="FormResult"/>.
    /// </summary>
    /// <param name="json">The JSON string defining the <see cref="FormSpec"/>.</param>
    /// <param name="dataOverrides">
    /// Optional dictionary of values that override or populate the form’s <c>data</c> block.
    /// </param>
    /// <param name="width">Optional window width.</param>
    /// <param name="height">Optional window height.</param>
    /// <param name="canResize">If <c>true</c>, the window can be resized by the user.</param>
    /// <returns>
    /// A <see cref="Task"/> that completes with the <see cref="FormResult"/>
    /// after the user submits or cancels the form.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if the JSON spec is invalid.</exception>
    public static Task<FormResult> ShowJsonAsync(
        string json,
        IDictionary<string, object?>? dataOverrides,
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

        return ShowAsync(spec, width, height, canResize);
    }

    /// <summary>
    /// Displays a dynamic form based on a <see cref="FormSpec"/>.
    /// </summary>
    /// <param name="spec">The form specification object.</param>
    /// <param name="width">Optional window width.</param>
    /// <param name="height">Optional window height.</param>
    /// <param name="canResize">If <c>true</c>, the window can be resized by the user.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that completes with the <see cref="FormResult"/>
    /// after the user submits or cancels the form.
    /// </returns>
    private static Task<FormResult> ShowAsync(
        FormSpec spec,
        double? width = null,
        double? height = null,
        bool canResize = false)
        => AvaloniaHost.RunOnUI(async () =>
        {
            var model = new Dictionary<string, object?>(
                (spec.Data ?? new()).ToDictionary(
                    kv => kv.Key,
                    kv => DynamicFormHelpers.FromJson(kv.Value)));

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
                var row = DynamicFormUi.BuildFieldRow(f, initial, out var getter, out var errorBlock);
                DynamicFormUi.WireChanges(row, () =>
                {
                    model[f.Key] = getter();
                    DynamicFormVisibility.ApplyVisibility(spec.Visibility, model, fieldContainers);
                    DynamicFormOptionVisibility.ApplyOptionFilters(spec.Visibility, model, fieldContainers); 
                });
                fieldContainers[f.Key] = row;
                inputGetters[f.Key] = getter;
                errorBlocks[f.Key] = errorBlock;

                root.Children.Add(row);
            }

            // Initial visibility
            DynamicFormVisibility.ApplyVisibility(spec.Visibility, model, fieldContainers);
            DynamicFormOptionVisibility.ApplyOptionFilters(spec.Visibility, model, fieldContainers);
            
            // Buttons
            var (okBtn, cancelBtn) = DynamicFormUi.BuildButtons(spec.Actions);
            root.Children.Add(UiHelpers.ButtonRow(cancelBtn, okBtn));

            // Window
            var w = UiHelpers.NewDialog(spec.Title, new ScrollViewer
            {
                Content = root,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            }, width ?? spec.Size?.Width, height ?? spec.Size?.Height, canResize);

            var tcs = new TaskCompletionSource<FormResult>();

            okBtn.Click += async (_, _) => await CompleteAsync(true);
            cancelBtn.Click += async (_, _) => await CompleteAsync(false);

            // Default buttons behavior
            okBtn.IsDefault = true;
            cancelBtn.IsCancel = true;

            await w.ShowDialog(AvaloniaHost.Owner);
            return await tcs.Task;

            async Task CompleteAsync(bool submitted)
            {
                // Refresh model from getters before submit/cancel
                foreach (var kv in inputGetters)
                    model[kv.Key] = kv.Value();

                if (submitted)
                {
                    var valid = DynamicFormValidation.ValidateAll(spec, model, errorBlocks);
                    if (!valid) return; // keep the dialog open
                }

                tcs.TrySetResult(new FormResult { Submitted = submitted, Values = model });
                await Task.Yield(); // yield UI thread if needed
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
}
