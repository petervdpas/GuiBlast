<Query Kind="Program">
  <NuGetReference>GuiBlast</NuGetReference>
  <Namespace>Avalonia</Namespace>
  <Namespace>Avalonia.Controls</Namespace>
  <Namespace>Avalonia.Controls.ApplicationLifetimes</Namespace>
  <Namespace>Avalonia.Threading</Namespace>
  <Namespace>GuiBlast</Namespace>
  <Namespace>GuiBlast.Forms.Rendering</Namespace>
  <Namespace>GuiBlast.Forms.Result</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <RuntimeVersion>8.0</RuntimeVersion>
</Query>

async Task Main()
{
	// Step 1: Prompt for name
	var userName = Prompts.Input("Your Name", "Please enter your name:");

	// Step 2: Load form JSON spec
	var queryDir = Path.GetDirectoryName(Util.CurrentQueryPath)!;
	var jsonPath = Path.Combine(queryDir, "form.json");
	var json = File.ReadAllText(jsonPath);

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