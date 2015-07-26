using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TivaCopterMonitor.Converters
{
	/// <summary>
	/// Value converter that translates true to the value of TrueValue property and false to the value of FalseValue.
	/// A non-null parameter invert converter behavior (negation).
	/// </summary>
	public sealed class BooleanToObjectConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (parameter != null)
				return (value is bool && !(bool)value) ? TrueValue : FalseValue;
			return (value is bool && (bool)value) ? TrueValue : FalseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (parameter != null)
				return value == FalseValue;
			return value == TrueValue;
		}

		public object FalseValue { get; set; }
		public object TrueValue { get; set; }
	}
}
