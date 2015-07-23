using System;
using Windows.UI.Xaml.Data;

namespace TivaCopterMonitor.Converters
{
	/// <summary>
	/// Value converter that translates true to false and false to true.
	/// </summary>
	public sealed class BooleanNegationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value is bool && !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return Convert(value, targetType, parameter, language);
        }
	}
}
