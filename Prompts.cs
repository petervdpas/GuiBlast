using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GuiBlast.Extensions;

namespace GuiBlast
{
    /// <summary>
    /// Provides blocking (sync) and asynchronous prompt dialogs for input, confirmation, and messages.
    /// </summary>
    public static class Prompts
    {
        // ---------- helper DTO ----------

        /// <summary>
        /// Internal item used for presenting (label) and returning (value).
        /// </summary>
        private sealed record SelectItem(string Value, string Label);

        // ---------- INPUT ----------

        /// <summary>
        /// Shows a blocking input dialog with a text field and returns the entered string.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="message">Message or label displayed above the input box.</param>
        /// <param name="initialText">Optional initial text to prefill the input field.</param>
        /// <param name="width">Optional dialog width.</param>
        /// <param name="height">Optional dialog height.</param>
        /// <param name="canResize">If true, allows resizing of the dialog window.</param>
        /// <returns>The entered text, or an empty string if canceled.</returns>
        public static string Input(
            string title,
            string message,
            string? initialText = "",
            double? width = null,
            double? height = null,
            bool canResize = false)
            => InputAsync(title, message, initialText, width, height, canResize).GetAwaiter().GetResult();

        /// <summary>
        /// Asynchronously shows an input dialog with a text field.
        /// </summary>
        private static Task<string> InputAsync(
            string title,
            string message,
            string? initialText = "",
            double? width = null,
            double? height = null,
            bool canResize = false)
            => AvaloniaHost.RunOnUI(async () =>
            {
                var input = new TextBox { Margin = new Thickness(0, 8, 0, 8), Text = initialText ?? "" };
                var ok = new Button { Content = "OK", MinWidth = 80, IsDefault = true };
                var cancel = new Button { Content = "Cancel", MinWidth = 80, IsCancel = true };

                var layout = new StackPanel
                {
                    Margin = new Thickness(12),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            Margin = new Thickness(0, 0, 0, 4),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        input,
                        UiHelpers.ButtonRow(cancel, ok)
                    }
                };

                var win = UiHelpers.NewDialog(title, layout, width, height, canResize);
                var tcs = new TaskCompletionSource<string>();

                void Complete(string s)
                {
                    if (!tcs.Task.IsCompleted) tcs.TrySetResult(s);
                    win.Close();
                }

                ok.Click += (_, _) => Complete(input.Text ?? "");
                cancel.Click += (_, _) => Complete("");

                win.Opened += (_, _) =>
                {
                    input.Focus();
                    input.CaretIndex = input.Text?.Length ?? 0;
                };

                await win.ShowDialog(AvaloniaHost.Owner);
                return await tcs.Task;
            });

        // ---------- SELECT (single) ----------

        /// <summary>
        /// Shows a blocking select dialog with a drop-down and returns the selected value.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="message">Message displayed above the selector.</param>
        /// <param name="options">Sequence of (Value, Label) pairs. Value is returned; Label is shown.</param>
        /// <param name="initialValue">Optional value to preselect.</param>
        /// <param name="width">Optional dialog width.</param>
        /// <param name="height">Optional dialog height.</param>
        /// <param name="canResize">If true, allows resizing of the dialog window.</param>
        /// <returns>The selected value, or <c>null</c> if canceled.</returns>
        private static string? SelectCore(
            string title,
            string message,
            IEnumerable<(string Value, string Label)> options,
            string? initialValue = null,
            double? width = null,
            double? height = null,
            bool canResize = false)
            => SelectCoreAsync(title, message, options, initialValue, width, height, canResize).GetAwaiter()
                .GetResult();

        /// <summary>
        /// Asynchronously shows a select dialog with a drop-down and returns the selected value.
        /// </summary>
        private static Task<string?> SelectCoreAsync(
            string title,
            string message,
            IEnumerable<(string Value, string Label)>? options,
            string? initialValue,
            double? width,
            double? height,
            bool canResize)
            => AvaloniaHost.RunOnUI(async () =>
            {
                var items = (options ?? [])
                    .Select(o => new SelectItem(o.Value, o.Label))
                    .ToList();

                if (items.Count == 0)
                    return null;

                // Use bindings in templates (no captured Label!)
                var itemTemplate = new FuncDataTemplate<SelectItem>((_, _) =>
                        new TextBlock
                            { [!TextBlock.TextProperty] = new Avalonia.Data.Binding(nameof(SelectItem.Label)) },
                    supportsRecycling: true);

                var combo = new ComboBox
                {
                    ItemsSource = items,
                    Margin = new Thickness(0, 8, 0, 8),
                    ItemTemplate = itemTemplate,
                    SelectionBoxItemTemplate = itemTemplate,
                    MaxDropDownHeight = 360
                };

                // Select by object (match on Value); fall back to first
                combo.SelectedItem =
                    (!string.IsNullOrWhiteSpace(initialValue)
                        ? items.FirstOrDefault(i => string.Equals(i.Value, initialValue, StringComparison.Ordinal))
                        : null)
                    ?? items.FirstOrDefault();

                var ok = new Button
                    { Content = "OK", MinWidth = 80, IsDefault = true, IsEnabled = combo.SelectedItem != null };
                var cancel = new Button { Content = "Cancel", MinWidth = 80, IsCancel = true };
                combo.SelectionChanged += (_, _) => ok.IsEnabled = combo.SelectedItem != null;

                var layout = new StackPanel
                {
                    Margin = new Thickness(12),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            Margin = new Thickness(0, 0, 0, 4),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        combo,
                        UiHelpers.ButtonRow(cancel, ok)
                    }
                };

                var win = UiHelpers.NewDialog(title, layout, width, height, canResize);
                var tcs = new TaskCompletionSource<string?>();

                void Complete(string? v)
                {
                    if (!tcs.Task.IsCompleted) tcs.TrySetResult(v);
                    win.Close();
                }

                ok.Click += (_, _) => Complete((combo.SelectedItem as SelectItem)?.Value);
                cancel.Click += (_, _) => Complete(null);
                combo.DoubleTapped += (_, _) =>
                {
                    if (combo.SelectedItem is SelectItem s) Complete(s.Value);
                };

                win.Opened += (_, _) => combo.Focus();

                await win.ShowDialog(AvaloniaHost.Owner);
                return await tcs.Task;
            });

        // ---------- SELECT MANY (multi) ----------

        /// <summary>
        /// Shows a blocking multi-select dialog and returns the selected values.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="message">Message displayed above the list.</param>
        /// <param name="options">Sequence of (Value, Label) pairs.</param>
        /// <param name="initialValues">Optional values to preselect.</param>
        /// <param name="width">Optional dialog width.</param>
        /// <param name="height">Optional dialog height.</param>
        /// <param name="canResize">If true, allows resizing of the dialog window.</param>
        /// <returns>Selected values, or an empty array if canceled.</returns>
        private static string[] SelectManyCore(
            string title,
            string message,
            IEnumerable<(string Value, string Label)> options,
            IEnumerable<string>? initialValues = null,
            double? width = null,
            double? height = null,
            bool canResize = false)
            => SelectManyCoreAsync(title, message, options, initialValues, width, height, canResize).GetAwaiter()
                .GetResult();

        /// <summary>
        /// Asynchronously shows a multi-select dialog and returns the selected values.
        /// </summary>
        private static Task<string[]> SelectManyCoreAsync(
            string title,
            string message,
            IEnumerable<(string Value, string Label)>? options,
            IEnumerable<string>? initialValues,
            double? width,
            double? height,
            bool canResize)
            => AvaloniaHost.RunOnUI(async () =>
            {
                var items = (options ?? [])
                    .Select(o => new SelectItem(o.Value, o.Label))
                    .ToList();

                // Nothing to pick -> behave like cancel
                if (items.Count == 0)
                    return [];

                var initial = new HashSet<string>(initialValues ?? [], StringComparer.Ordinal);

                var list = new ListBox
                {
                    ItemsSource = items,
                    SelectionMode = SelectionMode.Multiple,
                    Margin = new Thickness(0, 8, 0, 8),
                    ItemTemplate = new FuncDataTemplate<SelectItem>((x, _) =>
                        new TextBlock { Text = x.Label }, true)
                };

                // Preselect provided values
                list.AttachedToVisualTree += (_, _) =>
                {
                    foreach (var it in items.Where(i => initial.Contains(i.Value)))
                        list.SelectedItems!.Add(it);
                };

                var ok = new Button { Content = "OK", MinWidth = 80, IsDefault = true };
                var cancel = new Button { Content = "Cancel", MinWidth = 80, IsCancel = true };

                var layout = new Grid
                {
                    Margin = new Thickness(12),
                    RowDefinitions = new RowDefinitions("Auto,*,Auto"),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            Margin = new Thickness(0, 0, 0, 4),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        }.WithGrid(0, 0),
                        new Border
                        {
                            BorderThickness = new Thickness(1),
                            Padding = new Thickness(4),
                            Child = list
                        }.WithGrid(1, 0),
                        UiHelpers.ButtonRow(cancel, ok).WithGrid(2, 0)
                    }
                };

                var win = UiHelpers.NewDialog(title, layout, width, height, canResize);
                var tcs = new TaskCompletionSource<string[]>();

                void Complete(string[] v)
                {
                    if (!tcs.Task.IsCompleted) tcs.TrySetResult(v);
                    win.Close();
                }

                ok.Click += (_, _) =>
                {
                    var selected = list.SelectedItems?.OfType<SelectItem>().Select(i => i.Value).ToArray()
                                   ?? [];
                    Complete(selected);
                };
                cancel.Click += (_, _) => Complete([]);

                list.DoubleTapped += (_, _) =>
                {
                    var selected = list.SelectedItems?.OfType<SelectItem>().Select(i => i.Value).ToArray()
                                   ?? [];
                    if (selected.Length > 0) Complete(selected);
                };

                await win.ShowDialog(AvaloniaHost.Owner);
                return await tcs.Task;
            });

        // ---------- convenience overloads ----------

        /// <summary>
        /// Shows a blocking select dialog with a drop-down, where each item is both the label and the value.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="message">Message displayed above the selector.</param>
        /// <param name="items">The list of items to display. Each string is both label and value.</param>
        /// <param name="initialValue">Optional item to preselect.</param>
        /// <param name="width">Optional dialog width.</param>
        /// <param name="height">Optional dialog height.</param>
        /// <param name="canResize">If true, allows resizing of the dialog window.</param>
        /// <returns>The selected item string, or <c>null</c> if canceled.</returns>
        public static string? Select(
            string title,
            string message,
            IEnumerable<string> items,
            string? initialValue = null,
            double? width = null,
            double? height = null,
            bool canResize = false)
            => SelectCore(title, message, items.Select(s => (s, s)), initialValue, width, height, canResize);

        /// <summary>
        /// Shows a blocking multi-select dialog, where each item is both the label and the value.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="message">Message displayed above the list.</param>
        /// <param name="items">The list of items to display. Each string is both label and value.</param>
        /// <param name="initialValues">Optional set of items to preselect.</param>
        /// <param name="width">Optional dialog width.</param>
        /// <param name="height">Optional dialog height.</param>
        /// <param name="canResize">If true, allows resizing of the dialog window.</param>
        /// <returns>The selected items, or an empty array if canceled.</returns>
        public static string[] SelectMany(
            string title,
            string message,
            IEnumerable<string> items,
            IEnumerable<string>? initialValues = null,
            double? width = null,
            double? height = null,
            bool canResize = false)
            => SelectManyCore(title, message, items.Select(s => (s, s)), initialValues, width, height, canResize);

        /// <summary>
        /// Shows a blocking select dialog with a drop-down, using <see cref="GuiBlast.Forms.Model.Option"/> 
        /// instances for value/label pairs.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="message">Message displayed above the selector.</param>
        /// <param name="options">The options to display, each containing a <c>Value</c> and a <c>Label</c>.</param>
        /// <param name="initialValue">Optional value to preselect.</param>
        /// <param name="width">Optional dialog width.</param>
        /// <param name="height">Optional dialog height.</param>
        /// <param name="canResize">If true, allows resizing of the dialog window.</param>
        /// <returns>The <c>Value</c> of the selected option, or <c>null</c> if canceled.</returns>
        public static string? Select(
            string title,
            string message,
            IEnumerable<Forms.Model.Option> options,
            string? initialValue = null,
            double? width = null,
            double? height = null,
            bool canResize = false)
            => SelectCore(title, message, options.Select(o => (o.Value, o.Label)), initialValue, width, height,
                canResize);

        /// <summary>
        /// Shows a blocking multi-select dialog, using <see cref="GuiBlast.Forms.Model.Option"/> 
        /// instances for value/label pairs.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="message">Message displayed above the list.</param>
        /// <param name="options">The options to display, each containing a <c>Value</c> and a <c>Label</c>.</param>
        /// <param name="initialValues">Optional set of values to preselect.</param>
        /// <param name="width">Optional dialog width.</param>
        /// <param name="height">Optional dialog height.</param>
        /// <param name="canResize">If true, allows resizing of the dialog window.</param>
        /// <returns>The <c>Value</c> of each selected option, or an empty array if canceled.</returns>
        public static string[] SelectMany(
            string title,
            string message,
            IEnumerable<Forms.Model.Option> options,
            IEnumerable<string>? initialValues = null,
            double? width = null,
            double? height = null,
            bool canResize = false)
            => SelectManyCore(title, message, options.Select(o => (o.Value, o.Label)), initialValues, width, height,
                canResize);

        // ---------- CONFIRM ----------

        /// <summary>
        /// Shows a blocking yes/no style confirmation dialog.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="message">Message displayed above the buttons.</param>
        /// <param name="yesText">Text for the confirmation button.</param>
        /// <param name="noText">Text for the cancel button.</param>
        /// <param name="width">Optional dialog width.</param>
        /// <param name="height">Optional dialog height.</param>
        /// <param name="canResize">If true, allows resizing of the dialog window.</param>
        /// <returns><c>true</c> if the user clicked confirm, <c>false</c> otherwise.</returns>
        public static bool Confirm(
            string title,
            string message,
            string yesText = "OK",
            string noText = "Cancel",
            double? width = null,
            double? height = null,
            bool canResize = false)
            => ConfirmAsync(title, message, yesText, noText, width, height, canResize).GetAwaiter().GetResult();

        /// <summary>
        /// Asynchronously shows a yes/no style confirmation dialog.
        /// </summary>
        private static Task<bool> ConfirmAsync(
            string title,
            string message,
            string yesText = "OK",
            string noText = "Cancel",
            double? width = null,
            double? height = null,
            bool canResize = false)
            => AvaloniaHost.RunOnUI(async () =>
            {
                var yes = new Button { Content = yesText, MinWidth = 80, IsDefault = true };
                var no = new Button { Content = noText, MinWidth = 80, IsCancel = true };

                var layout = new StackPanel
                {
                    Margin = new Thickness(12),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            Margin = new Thickness(0, 0, 0, 8),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        UiHelpers.ButtonRow(no, yes)
                    }
                };

                var win = UiHelpers.NewDialog(title, layout, width, height, canResize);
                var tcs = new TaskCompletionSource<bool>();

                void Complete(bool v)
                {
                    if (!tcs.Task.IsCompleted) tcs.TrySetResult(v);
                    win.Close();
                }

                yes.Click += (_, _) => Complete(true);
                no.Click += (_, _) => Complete(false);

                await win.ShowDialog(AvaloniaHost.Owner);
                return await tcs.Task;
            });

        // ---------- MESSAGE ----------

        /// <summary>
        /// Shows a blocking message dialog with a single "OK" button.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="message">Message displayed in the dialog.</param>
        /// <param name="width">Optional dialog width.</param>
        /// <param name="height">Optional dialog height.</param>
        /// <param name="canResize">If true, allows resizing of the dialog window.</param>
        public static void Message(
            string title,
            string message,
            double? width = null,
            double? height = null,
            bool canResize = false)
            => MessageAsync(title, message, width, height, canResize).GetAwaiter().GetResult();

        /// <summary>
        /// Asynchronously shows a message dialog with a single "OK" button.
        /// </summary>
        private static async Task MessageAsync(
            string title,
            string message,
            double? width = null,
            double? height = null,
            bool canResize = false)
        {
            await AvaloniaHost.RunOnUI<object?>(async () =>
            {
                var ok = new Button { Content = "OK", MinWidth = 80, IsDefault = true };

                var layout = new StackPanel
                {
                    Margin = new Thickness(12),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            Margin = new Thickness(0, 0, 0, 8),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        UiHelpers.ButtonRow(ok)
                    }
                };

                var win = UiHelpers.NewDialog(title, layout, width, height, canResize);
                var tcs = new TaskCompletionSource<object?>();

                ok.Click += (_, _) =>
                {
                    if (!tcs.Task.IsCompleted) tcs.TrySetResult(null);
                    win.Close();
                };

                await win.ShowDialog(AvaloniaHost.Owner);
                await tcs.Task;
                return null;
            });
        }
    }
}