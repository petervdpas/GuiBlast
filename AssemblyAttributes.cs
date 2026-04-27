using System.Reflection;

// Blast-family convention: declare the canonical front-door type(s) of
// this assembly so AI assistants (e.g. TaskBlaster's script helper) can
// identify the entry points without scanning every public type.
// Comma-separated, fully-qualified.
[assembly: AssemblyMetadata("Blast.PrimaryFacade", "GuiBlast.Prompts,GuiBlast.Forms.Rendering.DynamicForm")]
