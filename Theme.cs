using System.Threading.Tasks;
using Avalonia;

namespace GuiBlast
{
    /// <summary>
    /// Represents available application theme modes.
    /// </summary>
    public enum ThemeMode
    {
        /// <summary>Light theme variant.</summary>
        Light,

        /// <summary>Dark theme variant.</summary>
        Dark
    }

    /// <summary>
    /// Provides methods to change the Avalonia application theme.
    /// Ensures theme changes are executed on the Avalonia UI thread.
    /// </summary>
    public static class Theme
    {
        /// <summary>
        /// Gets the currently active theme mode.
        /// </summary>
        public static ThemeMode Current { get; private set; } = ThemeMode.Light;

        /// <summary>
        /// Asynchronously sets the application theme.
        /// </summary>
        /// <param name="mode">The <see cref="ThemeMode"/> to apply.</param>
        private static Task SetAsync(ThemeMode mode)
        {
            Current = mode;

            return AvaloniaHost.RunOnUI(async () =>
            {
                var variant = mode == ThemeMode.Dark
                    ? Avalonia.Styling.ThemeVariant.Dark
                    : Avalonia.Styling.ThemeVariant.Light;

                if (Application.Current is { } app)
                    app.RequestedThemeVariant = variant;

                await Task.Yield(); // no real work, but keeps async signature consistent
                return 0;           // dummy return (discarded by caller)
            });
        }

        /// <summary>
        /// Synchronously sets the application theme.
        /// </summary>
        /// <param name="mode">The <see cref="ThemeMode"/> to apply.</param>
        public static void Set(ThemeMode mode) => SetAsync(mode).GetAwaiter().GetResult();
    }
}
