using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace GuiBlast
{
    /// <summary>
    /// Provides blocking (sync) and asynchronous prompt dialogs for input, confirmation, and messages.
    /// </summary>
    public static class Prompts
    {
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
                            Margin = new Thickness(0,0,0,4),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        input,
                        UiHelpers.ButtonRow(cancel, ok)
                    }
                };

                var win = UiHelpers.NewDialog(title, layout, width, height, canResize);
                var tcs = new TaskCompletionSource<string>();

                void Complete(string s) { if (!tcs.Task.IsCompleted) tcs.TrySetResult(s); win.Close(); }

                ok.Click += (_, _) => Complete(input.Text ?? "");
                cancel.Click += (_, _) => Complete("");

                win.Opened += (_, _) => { input.Focus(); input.CaretIndex = input.Text?.Length ?? 0; };

                await win.ShowDialog(AvaloniaHost.Owner);
                return await tcs.Task;
            });

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
                            Margin = new Thickness(0,0,0,8),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        UiHelpers.ButtonRow(no, yes)
                    }
                };

                var win = UiHelpers.NewDialog(title, layout, width, height, canResize);
                var tcs = new TaskCompletionSource<bool>();

                void Complete(bool v) { if (!tcs.Task.IsCompleted) tcs.TrySetResult(v); win.Close(); }

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
                            Margin = new Thickness(0,0,0,8),
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
