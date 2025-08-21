using Avalonia.Controls;
using Avalonia.Headless.XUnit;

namespace GuiBlast.Tests
{
    /// <summary>
    /// Unit tests for verifying the behavior of the <c>ButtonRow</c> UI helper.
    /// </summary>
    public class ButtonTests
    {
        /// <summary>
        /// Ensures that the <c>ButtonRow</c> method correctly arranges two buttons in a row container.
        /// </summary>
        [AvaloniaFact] // provided by Avalonia.Headless
        public void ButtonRow_ShouldHaveButtons()
        {
            var btn1 = new Button { Content = "One" };
            var btn2 = new Button { Content = "Two" };

            var row = UiHelpers.ButtonRow(btn1, btn2);

            Assert.Equal(2, row.Children.Count);
            Assert.Contains(btn1, row.Children);
            Assert.Contains(btn2, row.Children);
        }
    }
}