using System.Threading.Tasks;
using Avalonia;

namespace GuiBlast
{
    public enum ThemeMode { Light, Dark }

    public static class Theme
    {
        public static ThemeMode Current { get; private set; } = ThemeMode.Light;

        public static Task SetAsync(ThemeMode mode)
        {
            Current = mode;

            // Must run on Avalonia UI thread
            return AvaloniaHost.RunOnUI<object?>(async () =>
            {
                var variant = mode == ThemeMode.Dark
                    ? Avalonia.Styling.ThemeVariant.Dark
                    : Avalonia.Styling.ThemeVariant.Light;

                if (Application.Current is Application app)
                    app.RequestedThemeVariant = variant;

                return await Task.FromResult<object?>(null);
            });
        }

        public static void Set(ThemeMode mode) => SetAsync(mode).GetAwaiter().GetResult();
    }
}