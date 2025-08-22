# GuiBlast

[![NuGet](https://img.shields.io/nuget/v/GuiBlast.svg)](https://www.nuget.org/packages/GuiBlast)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GuiBlast.svg)](https://www.nuget.org/packages/GuiBlast)
[![License](https://img.shields.io/github/license/petervdpas/GuiBlast.svg)](https://opensource.org/licenses/MIT)

![RoadWarrior](assets/icon.png)

**GuiBlast** provides simple, cross-platform **modal prompts and dynamic forms** using [Avalonia](https://avaloniaui.net/).
It works out-of-the-box in **.NET console apps, LINQPad, and PowerShell scripts** — no window management required.

---

## ✨ Features

* 🔹 Minimal blocking & async APIs (`Input`, `Confirm`, `Message`)
* 🔹 JSON-driven **dynamic forms** with validation & visibility rules
* 🔹 Light/Dark theme switching via `Theme.Set`
* 🔹 Runs Avalonia on a hidden UI thread → safe for console and scripting environments

---

## 📦 Installation

```bash
dotnet add package GuiBlast
```

Or install from [NuGet Gallery](https://www.nuget.org/packages/GuiBlast).

---

## 🚀 Quick Example

```csharp
using GuiBlast.Forms.Rendering;
using GuiBlast.Forms.Result;

namespace GuiBlast.TryOut;

class Program
{
    static void Main()
    {
        // Step 1: Prompt for name
        var userName = Prompts.Input("Your Name", "Please enter your name:");

        // Step 2: Load form JSON spec
        var json = File.ReadAllText("form.json");

        // Step 3: Pre-fill the form with the name
        var overrides = new Dictionary<string, object?>
        {
            ["name"] = userName
        };

        // Step 4: Show form
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
```

---

## 📋 JSON Form Specification

GuiBlast forms are defined declaratively in JSON:

```jsonc
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
    { "key": "quota", "type": "number", "label": "Quota (GB)", "min": 0, "max": 100 },
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

---

## 🔧 Supported Field Types

* `text`, `number`, `switch`
* `select`, `multiselect`
* `datetime`

Validation & conditional visibility can be expressed directly in JSON.

---

## 📜 License

[MIT](https://opensource.org/licenses/MIT)
