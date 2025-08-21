using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace GuiBlast
{
    /// Public API: blocking prompts (sync) + async variants.
    public static class Prompts
    {
        // ---------- INPUT ----------
        public static string Input(
            string title,
            string message,
            string? initialText = "",
            double? width = null,
            double? height = null,
            bool canResize = false)
            => InputAsync(title, message, initialText, width, height, canResize).GetAwaiter().GetResult();

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
        public static bool Confirm(
            string title,
            string message,
            string yesText = "OK",
            string noText = "Cancel",
            double? width = null,
            double? height = null,
            bool canResize = false)
            => ConfirmAsync(title, message, yesText, noText, width, height, canResize).GetAwaiter().GetResult();

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
        public static void Message(
            string title,
            string message,
            double? width = null,
            double? height = null,
            bool canResize = false)
            => MessageAsync(title, message, width, height, canResize).GetAwaiter().GetResult();

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
