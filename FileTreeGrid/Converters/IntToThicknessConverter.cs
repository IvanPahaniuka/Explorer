using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FileTreeGrid.Converters
{
    public partial class IntToThicknessConverter : IValueConverter
    {
        //Methods
        //parameter format: x0.3,x0.2,x0.6,x10 || x0.3 x0.1 x0.5 || ...
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int valueInt)
            {
                string paramStr = parameter.ToString();
                var descr = new ParamDescription(paramStr);
                return descr.ToThickness(valueInt);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
 
    }
}
