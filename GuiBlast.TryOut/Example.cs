using System;
using System.Collections.Generic;
using System.IO;
using GuiBlast.Forms.Rendering;
using GuiBlast.Forms.Result;

namespace GuiBlast.TryOut;

internal static class Example
{
    /// <summary>Runs the old sample.</summary>
    internal static void Run()
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
            initialValue: "Viewer");

        Console.WriteLine($"Role: {role}");

        // Step 1.6: Multi select (interests/tags)
        var interests = Prompts.SelectMany(
            "Interests",
            "Pick one or more interests:",
            ["C#", "F#", "Rust", "Go", "Python", "TypeScript"],
            initialValues: ["C#"]);

        Console.WriteLine($"Interests: {string.Join(", ", interests)}");

        // Ask outside the form which scope to show (goes into visibilityContext)
        var scope = Prompts.Select("Role scope", "Which options should be visible?",
            ["basic", "basic+admin", "all"], "basic") ?? "basic";

        // Load spec
        var json = File.ReadAllText("form.json");

        // Prefill editable model
        var dataOverrides = new Dictionary<string, object?>
        {
            ["name"] = userName,
            ["role"] = role ?? "",
            ["interests"] = interests,
        };

        // Read-only context for rules (not returned)
        var visContext = new Dictionary<string, object?>
        {
            ["@roleScope"] = scope
        };

        // Show form
        Theme.Set(ThemeMode.Light);
        var result = DynamicForm.ShowJsonAsync(json, dataOverrides, visContext).Result;

        // Output
        Console.WriteLine($"Submitted: {result.Submitted}");
        Console.WriteLine("---- TEXT ----");
        Console.WriteLine(result.ToText());

        Console.WriteLine("---- JSON ----");
        Console.WriteLine(result.ToJson(indented: true));

        Console.WriteLine("---- DIRECT TO WRITER ----");
        result.WriteText();
    }
}