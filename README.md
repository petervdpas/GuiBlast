# GuiBlast

[![NuGet](https://img.shields.io/nuget/v/GuiBlast.svg)](https://www.nuget.org/packages/GuiBlast)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GuiBlast.svg)](https://www.nuget.org/packages/GuiBlast)
[![License](https://img.shields.io/github/license/petervdpas/GuiBlast.svg)](https://opensource.org/licenses/MIT)

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

class Program
{
    static void Main()
    {
        Theme.Set(ThemeMode.Light);

        // Simple input + confirmation
        var name = Prompts.Input("GuiBlast Test", "Enter your name:", canResize: true);
        if (!Prompts.Confirm("Confirm", $"Continue as {name}?")) return;

        // Load a JSON form specification
        var result = DynamicForm.ShowJsonAsync(File.ReadAllText("form.json")).Result;

        Console.WriteLine($"Submitted: {result.Submitted}");
        Console.WriteLine(result.ToJson());

        Prompts.Message("Done", "Finished testing GuiBlast.");
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
