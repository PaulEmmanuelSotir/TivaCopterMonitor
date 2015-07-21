using System;
using System.Reflection;
using Windows.UI.Xaml.Data;

namespace TivaCopterMonitor.Converters
{
	public class AngleToCartesianConverter : IValueConverter
	{
		/// <summary>
		/// Boolean property indicating wether if converter will convert angle to obtain X or Y cartesian coodinate.
		/// </summary>
		public bool IsXAxis { get; set; }

		public bool IsRadians { get; set; }

		/// <summary>
		/// Converts given angle to specified cartesion coordinate
		/// </summary>
		/// <param name="value">The source data being passed to the target.</param>
		/// <param name="targetType">The type of the target property, specified by a helper structure that wraps the type name.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic (scale value).</param>
		/// <param name="language">The language of the conversion.</param>
		/// <returns>The value to be passed to the target dependency property.</returns>
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			double angle = IsRadians ? GetDoubleValue(value, 0.0) : Math.PI / 180.0 * GetDoubleValue(value, 0.0);
			angle *= GetDoubleValue(parameter, 1.0);
			return IsXAxis ? Math.Cos(angle) : Math.Sin(angle);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException("Can't get angle from one cartesian coordinate >.<");
		}

		private static double GetDoubleValue(object value, double defaultValue)
		{
			double rslt;

			if (value == null)
				rslt = defaultValue;
			else if (!value.GetType().GetTypeInfo().IsPrimitive)
				rslt = Double.Parse(value.ToString());
			else
				try
				{
					rslt = System.Convert.ToDouble(value);
				}
				catch
				{
					rslt = defaultValue;
				}

			return rslt;
		}
	}
}
