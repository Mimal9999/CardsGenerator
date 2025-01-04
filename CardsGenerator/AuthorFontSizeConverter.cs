using System;
using System.Globalization;
using System.Windows.Data;

namespace CardsGenerator
{
    public class AuthorFontSizeConverter : IValueConverter
    {
        // Minimum and maximum font sizes
        private const double MinFontSize = 12;   // Minimum font size
        private const double MaxFontSize = 20;   // Maximum font size
        private const int MaxLength = 14;        // Maximum text length for default font size

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string authorName)
            {
                // Calculate font size based on the text length
                double fontSize = MaxFontSize;

                if (authorName.Length > MaxLength)
                {
                    // Adjust font size proportionally for longer text
                    fontSize = MaxFontSize * (double)MaxLength / authorName.Length;

                    // Ensure the font size does not go below the minimum
                    fontSize = Math.Max(fontSize, MinFontSize);
                }

                return fontSize;
            }

            // Return default font size if input is invalid
            return MaxFontSize;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported.");
        }
    }
}
