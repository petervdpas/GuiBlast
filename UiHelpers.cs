using System;
using Avalonia.Controls;
using Avalonia.Layout;

namespace GuiBlast
{
    internal static class UiHelpers
    {
        public static StackPanel ButtonRow(params Control[] buttons)
            => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8
            }.Also(sp => { foreach (var b in buttons) sp.Children.Add(b); });

        // NEW: optional width/height; falls back to auto-size if not provided
        public static Window NewDialog(string title, Control content,
                                       double? width = null, double? height = null,
                                       bool canResize = false)
        {
            var w = new Window
            {
                Title = title,
                CanResize = canResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = content
            };

            if (width is null && height is null)
            {
                // Auto-fit to content
                w.SizeToContent = SizeToContent.WidthAndHeight;
            }
            else
            {
                if (width  is { } ww) w.Width  = ww;
                if (height is { } hh) w.Height = hh;
            }
            return w;
        }

        private static T Also<T>(this T obj, Action<T> a) { a(obj); return obj; }
    }
}
