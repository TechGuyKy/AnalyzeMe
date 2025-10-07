using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AnalyzeMe.Converters
{
    public class PercentageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                if (percentage < 50)
                    return new SolidColorBrush(Color.FromRgb(0, 255, 255)); //Cyan color
                else if (percentage < 75)
                    return new SolidColorBrush(Color.FromRgb(255, 0, 255)); //Magenta color
                else
                    return new SolidColorBrush(Color.FromRgb(255, 0, 0)); //Red color
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}