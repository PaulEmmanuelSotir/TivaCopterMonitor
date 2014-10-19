using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TivaCopterMonitor.Converters
{
	/// <summary>
	/// Value converter that translates numbers to percentage according to parameter representing max value.
	/// If no parameter is specified, Max value will be 1.
	/// </summary>
	public sealed class NumberToPercentageConverter : IValueConverter
	{
		/// <summary>
		/// Modifies the source data before passing it to the target for display in the UI.
		/// </summary>
		/// <param name="value">The source data being passed to the target.</param>
		/// <param name="targetType">The type of the target property, specified by a helper structure that wraps the type name.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="language">The language of the conversion.</param>
		/// <returns>The value to be passed to the target dependency property.</returns>
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var typeInfo = value.GetType().GetTypeInfo();
			var targetTypeInfo = targetType.GetTypeInfo();

			if (!targetTypeInfo.IsPrimitive || targetTypeInfo.IsPointer)
				throw new ArgumentException("Target type must be a numeric value.");

			if (value.GetType() != targetType || parameter.GetType() != targetType)
				throw new ArgumentException("Parameter type must be the same as value and target type.");

			if (typeInfo.IsPrimitive && !typeInfo.IsPointer)
			{
				if (parameter == null)
					return System.Convert.ChangeType(100.0 * (double)value, value.GetType());
				else
					return System.Convert.ChangeType(100.0 * ((double)value / (double)parameter), value.GetType());
			}

			throw new ArgumentException("Argument must be a numeric value.");
		}

		/// <summary>
		/// Modifies the target data before passing it to the source object. This method is called only in <c>TwoWay</c> bindings. 
		/// </summary>
		/// <param name="value">The target data being passed to the source..</param>
		/// <param name="targetType">The type of the target property, specified by a helper structure that wraps the type name.</param>
		/// <param name="parameter">An optional parameter to be used in the converter logic.</param>
		/// <param name="language">The language of the conversion.</param>
		/// <returns>The value to be passed to the source object.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			var typeInfo = value.GetType().GetTypeInfo();
			var targetTypeInfo = targetType.GetTypeInfo();

			if (!targetTypeInfo.IsPrimitive || targetTypeInfo.IsPointer)
				throw new ArgumentException("Target type must be a numeric value.");

			if (value.GetType() != targetType || parameter.GetType() != targetType)
				throw new ArgumentException("Parameter type must be the same as value and target type.");

			if (typeInfo.IsPrimitive && !typeInfo.IsPointer)
			{
				if (parameter == null)
					return System.Convert.ChangeType((double)value / 100.0, value.GetType());
				else
					return System.Convert.ChangeType((double)parameter * (double)value / 100.0, value.GetType());
			}

			throw new ArgumentException("Argument must be a numeric value.");
		}
	}
}
