using Avalonia;
using Avalonia.Themes.Fluent;

namespace GuiBlast
{
    /// <summary>
    /// Main Avalonia <see cref="Application"/> implementation.
    /// Initializes global styles and applies the current theme.
    /// </summary>
    internal sealed class App : Application
    {
        /// <summary>
        /// Initializes application-wide resources:
        /// <list type="bullet">
        ///   <item>Adds <see cref="FluentTheme"/> so all Avalonia controls have templates.</item>
        ///   <item>Applies the current <see cref="Theme"/> selection (light/dark).</item>
        /// </list>
        /// </summary>
        public override void Initialize()
        {
            // Load Fluent so controls have templates
            Styles.Add(new FluentTheme());

            // Apply whatever the Theme API currently says
            RequestedThemeVariant = Theme.Current == ThemeMode.Dark
                ? Avalonia.Styling.ThemeVariant.Dark
                : Avalonia.Styling.ThemeVariant.Light;
        }
    }
}
