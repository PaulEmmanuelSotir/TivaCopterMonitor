using System;
using System.Reflection;
using Windows.UI.Xaml.Data;

namespace TivaCopterMonitor.Converters
{
	public class AddScaleConverter : IValueConverter
	{
		public double Factor { get; set; }

		public double Offset { get; set; }

		/// <summary>
		/// Boolean property indicating wether if converter parameter will be interpreated as an offset or a factor (will override corresponding property).
		/// </summary>
		public bool IsParameterOffset { get; set; }

		/// <summary>
		/// Scale input value according to parameter
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (parameter == null)
				return GetDoubleValue(value, 0) * Factor + Offset;
			else if (IsParameterOffset)
				return GetDoubleValue(value, 0) * Factor + GetDoubleValue(parameter, 0);
			else
				return GetDoubleValue(value, 0) * GetDoubleValue(parameter, 1) + Offset;
		}

		/// <summary>
		/// Unscale input value according to parameter 
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (parameter == null)
				return GetDoubleValue(value, 0) * Factor + Offset;
			else if (IsParameterOffset)
				return GetDoubleValue(value, 0) * Factor + GetDoubleValue(parameter, 0);
			else
				return GetDoubleValue(value, 0) * GetDoubleValue(parameter, 1) + Offset;
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
