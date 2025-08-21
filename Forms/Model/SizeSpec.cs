namespace GuiBlast.Forms.Model;

public sealed class SizeSpec(double? height, double? width)
{
    public double? Width { get; } = width;
    public double? Height { get; } = height;
}
