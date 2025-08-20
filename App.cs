using Avalonia;
using Avalonia.Themes.Fluent;

namespace GuiBlast
{
    internal sealed class App : Application
    {
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