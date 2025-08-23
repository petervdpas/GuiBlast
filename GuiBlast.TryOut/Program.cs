using System;
using System.Collections.Generic;
using System.IO;
using GuiBlast.Forms.Rendering;
using GuiBlast.Forms.Result;

namespace GuiBlast.TryOut;

class Program
{
    static void Main()
    {
        // Step 0: Set theme
        Theme.Set(ThemeMode.Dark);

        // Step 1: Prompt for name
        var userName = Prompts.Input("Your Name", "Please enter your name:");

        // Step 1.5: Single select (role)
        var role = Prompts.Select(
            "Role",
            "Pick your role:",
            ["Administrator", "Power User", "Viewer"],
            initialValue: "Viewer"); // returns string? (null if canceled)

        Console.WriteLine($"Role: {role}");
        
        // Step 1.6: Multi select (interests/tags)
        var interests = Prompts.SelectMany(
            "Interests",
            "Pick one or more interests:",
            ["C#", "F#", "Rust", "Go", "Python", "TypeScript"],
            initialValues: ["C#"]); // returns string[] (empty if canceled)

        Console.WriteLine($"Interests: {string.Join(", ", interests)}");
        
        // Ask outside the form which scope to show
        var scope = Prompts.Select(
            "Which roles can be chosen?",
            "Pick the scope for the Role dropdown:",
            ["basic", "basic+admin", "all"],
            initialValue: "basic"
        ) ?? "basic";
        
        // Step 2: Load form JSON spec
        var json = File.ReadAllText("form.json");

        // Step 3: Pre-fill the form with the name and our selections
        var overrides = new Dictionary<string, object?>
        {
            ["name"] = userName,                          // text
            ["role"] = role ?? "",                        // single select; coalesce null to empty
            ["interests"] = interests,                     // multi-select array
            ["_roleScope"] = scope
        };

        // Step 4: Show form
        Theme.Set(ThemeMode.Light);
        var result = DynamicForm.ShowJsonAsync(json, overrides).Result;

        // Step 5: Print outcome
        Console.WriteLine($"Submitted: {result.Submitted}");
        Console.WriteLine("---- TEXT ----");
        Console.WriteLine(result.ToText());

        Console.WriteLine("---- JSON ----");
        Console.WriteLine(result.ToJson(indented: true));

        Console.WriteLine("---- DIRECT TO WRITER ----");
        result.WriteText();
    }
}
