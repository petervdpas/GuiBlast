using System;
using System.IO;
using GuiBlast;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        Theme.Set(ThemeMode.Light);

        // quick sanity dialogs
        var name = Prompts.Input("GuiBlast Test", "Enter your name:", canResize: true);
        var confirm = Prompts.Confirm("Confirm", $"Continue as {name}?", width: 320, height: 120);

        if (!confirm)
        {
            Console.WriteLine("User cancelled.");
            return;
        }

        // load form spec and show dynamic form
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "form.json");
        var json = File.ReadAllText(jsonPath);

        var result = DynamicForm.ShowJsonAsync(
            json,
            new Dictionary<string, object?>
            {
                ["name"] = name,
                ["when"] = DateTime.Now
            },
            width: 720,
            height: 520,
            canResize: true
        ).GetAwaiter().GetResult();

        Console.WriteLine($"Submitted: {result.Submitted}");
        Console.WriteLine(result.ToJson());

        Prompts.Message("Done", "Finished testing GuiBlast.", height: 200, canResize: true);
    }
}