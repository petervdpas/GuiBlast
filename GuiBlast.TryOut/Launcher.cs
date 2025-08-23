using System;

namespace GuiBlast.TryOut;

internal static class Launcher
{
    /// <summary>Entry point. Pick a demo and run it.</summary>
    private static void Main()
    {
        Theme.Set(ThemeMode.Dark);

        var pick = Prompts.Select(
            "Demo",
            "Choose which sample to run:",
            ["Old (form.json)", "New (cascade.json)"],
            initialValue: "Old (form.json)");

        if (string.Equals(pick, "New (cascade.json)", StringComparison.Ordinal))
            ExampleTwo.Run();
        else
            Example.Run();
    }
}