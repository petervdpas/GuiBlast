# GuiBlast

Simple Avalonia-based modal prompts for .NET, LINQPad, and PowerShell.

```csharp
using GuiBlast;

var name = Prompts.Input("GuiBlast Test", "Enter your name:");
if (Prompts.Confirm("Confirm", $"Continue as {name}?"))
{
    Prompts.Message("Done", "Finished testing GuiBlast.");
}
````

* Blocking & async APIs (`Input`, `Confirm`, `Message`)
* Light/Dark theme support via `Theme.Set`
* Runs Avalonia on a hidden UI thread — works in console apps & scripts
