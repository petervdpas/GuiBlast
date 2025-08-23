using Avalonia.Controls;

namespace GuiBlast.Extensions;

/// <summary>
/// Provides extension methods for working with Avalonia <see cref="Control"/> objects,
/// enabling a fluent syntax when configuring layout properties.
/// </summary>
public static class ControlExtensions
{
    /// <summary>
    /// Sets the row and column attached properties for the specified <paramref name="control"/> 
    /// when it is placed inside a <see cref="Grid"/>.
    /// </summary>
    /// <typeparam name="T">The type of control being configured. Must derive from <see cref="Control"/>.</typeparam>
    /// <param name="control">The control to configure.</param>
    /// <param name="row">The grid row index in which to place the control.</param>
    /// <param name="col">The grid column index in which to place the control.</param>
    /// <returns>The same <paramref name="control"/> instance, to allow fluent method chaining.</returns>
    /// <remarks>
    /// This method sets the <see cref="Grid.RowProperty"/> and <see cref="Grid.ColumnProperty"/> 
    /// attached properties on the given control, making it convenient to place controls fluently:
    /// <code>
    /// var grid = new Grid
    /// {
    ///     RowDefinitions = new RowDefinitions("Auto,*"),
    ///     ColumnDefinitions = new ColumnDefinitions("*,*"),
    ///     Children =
    ///     {
    ///         new TextBlock { Text = "Top left" }.WithGrid(0, 0),
    ///         new Button { Content = "Bottom right" }.WithGrid(1, 1)
    ///     }
    /// };
    /// </code>
    /// </remarks>
    public static T WithGrid<T>(this T control, int row, int col) where T : Control
    {
        Grid.SetRow(control, row);
        Grid.SetColumn(control, col);
        return control;
    }
}