# GuiBlast

Simple Avalonia-based modal prompts for .NET, LINQPad, and PowerShell.

```json
{
  "title": "Peer",
  "data": {
    "name": "Alice",
    "role": "User",
    "enabled": true,
    "quota": 5,
    "tags": ["a", "c"]
  },
  "fields": [
    { "key": "name", "type": "text", "label": "Name", "required": true },
    {
      "key": "role",
      "type": "select",
      "label": "Role",
      "options": [
        { "value": "User", "label": "User" },
        { "value": "Admin", "label": "Admin" },
        { "value": "Guest", "label": "Guest" }
      ]
    },
    { "key": "enabled", "type": "switch", "label": "Enabled" },
    {
      "key": "quota",
      "type": "number",
      "label": "Quota (GB)",
      "min": 0,
      "max": 100
    },
    {
      "key": "tags",
      "type": "multiselect",
      "label": "Tags",
      "options": [
        { "value": "a", "label": "a" },
        { "value": "b", "label": "b" },
        { "value": "c", "label": "c" }
      ]
    },
    { "key": "when", "type": "datetime", "label": "When" }
  ],
  "visibility": [
    { "field": "role", "eq": "Admin", "show": ["quota"] },
    { "field": "role", "neq": "Admin", "hide": ["quota"] }
  ],
  "actions": [
    { "id": "save", "label": "Save", "submit": true },
    { "id": "cancel", "label": "Cancel", "dismiss": true }
  ]
}
```

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using GuiBlast.Forms.Rendering;
using GuiBlast.Forms.Result;

namespace GuiBlast.GuiBlastTest;

public class Program
{
    private static void Main()
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
```

* Blocking & async APIs (`Input`, `Confirm`, `Message`)
* Light/Dark theme support via `Theme.Set`
* Runs Avalonia on a hidden UI thread — works in console apps & scripts

