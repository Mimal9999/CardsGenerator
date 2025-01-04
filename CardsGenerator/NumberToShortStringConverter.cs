using System;
using System.Globalization;
using System.Windows.Data;

namespace CardsGenerator
{
    public class NumberToShortStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long number)
            {
                // Convert the number to a shortened format
                if (number >= 1_000_000_000_000) // Trillions
                    return $"{number / 1_000_000_000_000.0:0.#}T";
                if (number >= 1_000_000_000) // Billions
                    return $"{number / 1_000_000_000.0:0.#}B";
                if (number >= 1_000_000) // Millions
                    return $"{number / 1_000_000.0:0.#}M";
                if (number >= 1_000) // Thousands
                    return $"{number / 1_000.0:0}K";

                // Return the number as is if no formatting is needed
                return number.ToString();
            }

            // Fallback: Return the original value for unknown types
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Reverse conversion is not supported
            throw new NotSupportedException("Conversion back is not supported.");
        }
    }
}
