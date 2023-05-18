using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TestDataGrid_01
{
    public static class Tag_helper_methods
    {
        // Metodo per creare tag delle immagini
        public static InlineUIContainer CreateImagePlaceholder(int imageNumber)
        {
            var textBlock = new TextBlock
            {
                Text = $"Img. {imageNumber}",
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 10,
                FontFamily = new FontFamily("Arial"),
            };

            var border = new Border
            {
                Child = textBlock,
                Background = new SolidColorBrush(Color.FromArgb(255, 255, 30, 255)), // Light purple background
                CornerRadius = new CornerRadius(6), // Rounded corners
                Padding = new Thickness(2),
                Margin = new Thickness(2),
            };
            var inlineUIContainer = new InlineUIContainer(border)
            {
                BaselineAlignment = BaselineAlignment.Bottom // Align to the bottom border of the text
            };
            return inlineUIContainer;
        }
    }
}