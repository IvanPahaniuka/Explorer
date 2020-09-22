using FileTreeGrids.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FileTreeGrid.Converters
{
    public class PathToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                if (!PathExtensions.IsDirectory(path))
                {
                    try
                    {
                        using (var icon = Icon.ExtractAssociatedIcon(path))
                        {
                            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        }
                    }
                    catch
                    {
                        return DependencyProperty.UnsetValue;
                    }

                }
                else
                {
                    try
                    {
                        return new BitmapImage(new Uri("pack://application:,,,/FileTreeGrid;component/Resources/folder.png"));
                    }
                    catch
                    {
                        return DependencyProperty.UnsetValue;
                    }
                    
                }
            }
            

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
