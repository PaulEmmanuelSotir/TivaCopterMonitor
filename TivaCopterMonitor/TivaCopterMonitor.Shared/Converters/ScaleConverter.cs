using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TivaCopterMonitor.Converters
{
    public class ScaleConverter : IValueConverter
	{
		/// <summary>
		/// Scale input value according to parameter
		/// </summary>
		/// <param name="value">The source data being passed to the target.</param>
		/// <param name="targetType">The type of the target property, specified by a helper structure that wraps the type name.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="language">The language of the conversion.</param>
		/// <returns>The value to be passed to the target dependency property.</returns>
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (parameter == null)
				return value;

			var valueTypeInfo = value.GetType().GetTypeInfo();
			var targetTypeInfo = targetType.GetTypeInfo();
			var parameterTypeInfo = parameter.GetType().GetTypeInfo();

			if (!targetTypeInfo.IsPrimitive || targetTypeInfo.IsPointer)
				throw new ArgumentException("Target type must be a numeric value.");

			if (!valueTypeInfo.IsPrimitive || targetTypeInfo.IsPointer)
				throw new ArgumentException("Value type must be a numeric value.");

			if (!parameterTypeInfo.IsPrimitive || parameterTypeInfo.IsPointer)
				throw new ArgumentException("Value type must be a numeric value.");

			return (double)value * (double)parameter;
		}

		/// <summary>
		/// Unscale input value according to parameter 
		/// </summary>
		/// <param name="value">The target data being passed to the source..</param>
		/// <param name="targetType">The type of the target property, specified by a helper structure that wraps the type name.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="language">The language of the conversion.</param>
		/// <returns>The value to be passed to the source object.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (parameter == null)
				return value;

			var valueTypeInfo = value.GetType().GetTypeInfo();
			var targetTypeInfo = targetType.GetTypeInfo();
			var parameterTypeInfo = parameter.GetType().GetTypeInfo();

			if (!targetTypeInfo.IsPrimitive || targetTypeInfo.IsPointer)
				throw new ArgumentException("Target type must be a numeric value.");

			if (!valueTypeInfo.IsPrimitive || targetTypeInfo.IsPointer)
				throw new ArgumentException("Value type must be a numeric value.");

			if (!parameterTypeInfo.IsPrimitive || parameterTypeInfo.IsPointer)
				throw new ArgumentException("Value type must be a numeric value.");

			return (double)value / (double)parameter;
		}
	}
}
