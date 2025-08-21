namespace GuiBlast.Forms.Result;

public static class FormResultExtensions
{
    public static string ToText(this FormResult result) => FormResultFormatter.ToText(result);
    public static string ToJson(this FormResult result, bool indented = true) => FormResultFormatter.ToJson(result, indented);
    public static void WriteText(this FormResult result, System.IO.TextWriter? writer = null)
        => FormResultFormatter.WriteText(result, writer);
}
