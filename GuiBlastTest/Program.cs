using System;
using GuiBlast;

class Program
{
    static void Main()
    {
        Theme.Set(ThemeMode.Light);

        // Auto-size (no dimensions)
        var name = Prompts.Input("GuiBlast Test", "Enter your name:", canResize: true);
        Console.WriteLine($"Hello, {name}!");

        // Fixed size
        var confirm = Prompts.Confirm("Confirm", $"Continue as {name}?", width: 320, height: 120);
        Console.WriteLine($"Confirm result: {confirm}");

        // Resizable tall message
        Prompts.Message("Done", "Finished testing GuiBlast.", height: 200, canResize: true);
    }
}