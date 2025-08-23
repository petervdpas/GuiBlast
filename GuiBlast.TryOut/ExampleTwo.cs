using System;
using System.IO;
using GuiBlast.Forms.Rendering;
using GuiBlast.Forms.Result;

namespace GuiBlast.TryOut;

internal static class ExampleTwo
{
    /// <summary>Runs the cascade sample.</summary>
    internal static void Run()
    {
        Theme.Set(ThemeMode.Light);

        // Load and show the cascading-select form
        var json = File.ReadAllText("cascade.json");

        Theme.Set(ThemeMode.Light);
        var result = DynamicForm.ShowJsonAsync(json).Result;

        Console.WriteLine($"Submitted: {result.Submitted}");
        Console.WriteLine("---- JSON ----");
        Console.WriteLine(result.ToJson(indented: true));
    }
}
