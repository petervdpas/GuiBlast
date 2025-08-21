namespace GuiBlast.Forms.Model;

/// <summary>
/// Defines an optional size specification (width and height) for a form.
/// </summary>
public sealed class SizeSpec(double? height, double? width)
{
    /// <summary>
    /// Desired width of the form in device-independent pixels.
    /// </summary>
    public double? Width { get; } = width;

    /// <summary>
    /// Desired height of the form in device-independent pixels.
    /// </summary>
    public double? Height { get; } = height;
}
