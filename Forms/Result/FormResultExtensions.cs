namespace GuiBlast.Forms.Result;

/// <summary>
/// Extension methods for converting and exporting <see cref="FormResult"/> values.
/// </summary>
public static class FormResultExtensions
{
    /// <summary>
    /// Converts the form result into a plain text representation.
    /// </summary>
    /// <param name="result">The form result to convert.</param>
    /// <returns>A string containing the formatted field values.</returns>
    public static string ToText(this FormResult result) 
        => FormResultFormatter.ToText(result);

    /// <summary>
    /// Converts the form result into a JSON string.
    /// </summary>
    /// <param name="result">The form result to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string containing the form values.</returns>
    public static string ToJson(this FormResult result, bool indented = true) 
        => FormResultFormatter.ToJson(result, indented);

    /// <summary>
    /// Writes the form result as plain text to the specified <see cref="System.IO.TextWriter"/>.
    /// </summary>
    /// <param name="result">The form result to write.</param>
    /// <param name="writer">
    /// The text writer to output to.  
    /// If <c>null</c>, output is written to <see cref="System.Console.Out"/>.
    /// </param>
    public static void WriteText(this FormResult result, System.IO.TextWriter? writer = null)
        => FormResultFormatter.WriteText(result, writer);
}
