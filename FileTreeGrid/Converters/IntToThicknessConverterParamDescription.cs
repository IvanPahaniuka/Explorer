using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace FileTreeGrid.Converters
{
    public partial class IntToThicknessConverter : IValueConverter
    {
        private struct ParamDescription
        {
            //Fields
            private double[] paramsArr;

            //Properties
            public double Left
            {
                get => paramsArr[0];
                set => paramsArr[0] = value;
            }
            public double Top
            {
                get => paramsArr[1];
                set => paramsArr[1] = value;
            }
            public double Right
            {
                get => paramsArr[2];
                set => paramsArr[2] = value;
            }
            public double Bottom
            {
                get => paramsArr[3];
                set => paramsArr[3] = value;
            }

            //Constructors
            public ParamDescription(string param)
                : this()
            {
                paramsArr = new double[4];

                var paramsArrShort = param
                    .Replace("x", string.Empty)
                    .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => double.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture))
                    .ToArray();

                ConvertShortDescriptionToFull(paramsArrShort);
            }

            //Methods
            public Thickness ToThickness(int value)
            {
                return ToThickness((double)value);
            }
            public Thickness ToThickness(double value)
            {
                return new Thickness(
                    value * Left,
                    value * Top,
                    value * Right,
                    value * Bottom);
            }

            private void ConvertShortDescriptionToFull(double[] shortDescr)
            {
                switch (shortDescr.Length)
                {
                    case 4:
                        Array.Copy(shortDescr, paramsArr, shortDescr.Length);
                        break;
                    case 3:
                        Array.Copy(shortDescr, paramsArr, shortDescr.Length);
                        Bottom = Top;
                        break;
                    case 2:
                        Left = shortDescr[0];
                        Top = shortDescr[1];
                        Right = Left;
                        Bottom = Top;
                        break;
                    case 1:
                        Array.Fill(paramsArr, shortDescr[0]);
                        break;
                    default:
                        Array.Fill(paramsArr, 0);
                        break;
                }
            }
        }
    }
}
