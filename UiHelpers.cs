using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Platform;

namespace GuiBlast
{
    /// <summary>
    /// Utility helpers for building common Avalonia UI patterns.
    /// </summary>
    public static class UiHelpers
    {
        /// <summary>
        /// Creates a horizontal row of buttons, aligned to the right.
        /// </summary>
        /// <param name="buttons">The buttons to include in the row.</param>
        /// <returns>
        /// A <see cref="StackPanel"/> containing the provided buttons in order.
        /// </returns>
        public static StackPanel ButtonRow(params Control[] buttons)
            => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8
            }.Also(sp =>
            {
                foreach (var b in buttons)
                    sp.Children.Add(b);
            });

        /// <summary>
        /// Creates a new dialog <see cref="Window"/> with the given content and title.
        /// </summary>
        /// <param name="title">Dialog window title.</param>
        /// <param name="content">The content control to place inside the dialog.</param>
        /// <param name="width">Optional fixed width; if <c>null</c>, uses content width.</param>
        /// <param name="height">Optional fixed height; if <c>null</c>, uses content height.</param>
        /// <param name="canResize">Whether the dialog window can be resized.</param>
        /// <returns>A new <see cref="Window"/> configured as a dialog.</returns>
        public static Window NewDialog(
            string title,
            Control content,
            double? width = null,
            double? height = null,
            bool canResize = false)
        {
            var w = new Window
            {
                Title = title,
                CanResize = canResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = content
            };

            // assign icon from DLL resources
            try
            {
                var uri = new Uri("avares://GuiBlast/Assets/hacker.ico");
                w.Icon = new WindowIcon(AssetLoader.Open(uri));
            }
            catch
            {
                // fail silently if asset not found
                
            }
            if (width is null && height is null)
            {
                // Auto-fit to content
                w.SizeToContent = SizeToContent.WidthAndHeight;
            }
            else
            {
                if (width is { } ww) w.Width = ww;
                if (height is { } hh) w.Height = hh;
            }

            return w;
        }

        /// <summary>
        /// Extension method to allow fluent "also" initialization.
        /// Executes <paramref name="a"/> with <paramref name="obj"/>
        /// and then returns <paramref name="obj"/>.
        /// </summary>
        private static T Also<T>(this T obj, Action<T> a)
        {
            a(obj);
            return obj;
        }
    }
}
