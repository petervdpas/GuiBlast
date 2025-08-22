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

        // Step 2: Load form JSON spec
        var json = File.ReadAllText("form.json");

        // Step 3: Pre-fill the form with the name
        var overrides = new Dictionary<string, object?>
        {
            ["name"] = userName
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