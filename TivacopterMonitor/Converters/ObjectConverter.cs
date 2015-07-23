using System;
using Windows.UI.Xaml.Data;

namespace TivaCopterMonitor.Converters
{
	/// <summary>
	/// Value converter that casts an Object to the specified type.
	/// </summary>
	public sealed class ObjectConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (value?.GetType() == targetType)
				return value;
			return null;
		}
	}
}
