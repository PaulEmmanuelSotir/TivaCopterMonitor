using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using Windows.Devices.HumanInterfaceDevice;

namespace TivaCopterMonitor.DataAccessLayer
{
	[DataContract]
	public sealed class HIDDataMap<T> where T : class, new()
	{
		// TODO: savoir si il est possible de rendre cdette liste readOnly
		[DataMember(Name = "HIDUsageMap")]
		public List<PropertyToHidAttributeBinding> DataMap { get; private set; }

		public HIDDataMap()
		{
			// List all public boolean and float properties of T type.
			DataMap = (from prop in typeof(T).GetRuntimeProperties()
					   where (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(bool)) && prop.GetMethod.IsPublic && prop.SetMethod.IsPublic
					   select new PropertyToHidAttributeBinding { Property = prop }).ToList();
		}

		public T GetDataFromHidInputReport(HidInputReport HidReport)
		{
			var outputData = new T();

			foreach (var data in DataMap)
			{
				PropertyInfo prop = data.Property;

				if (prop.PropertyType == typeof(bool))
					prop.SetValue(outputData, HidReport.GetBooleanControl(data.UsagePage, data.UsageId).IsActive);
				else if (prop.PropertyType == typeof(float))
				{
					var test1 = HidReport.GetNumericControl(data.UsagePage, data.UsageId);
					var test2 = HidReport.GetNumericControl(data.UsagePage, data.UsageId).ScaledValue;
					prop.SetValue(outputData, HidReport.GetNumericControl(data.UsagePage, data.UsageId).Value);
				}
			}

			return outputData;
		}
	}

	[DataContract(Name = "HIDInput")]
	public class PropertyToHidAttributeBinding : INotifyPropertyChanged
	{
		[DataMember(Name = "BindedProperty")]
		public PropertyInfoSurrogate Property { get; set; }

		[DataMember]
		public ushort UsagePage
		{
			get { return _usagePage; }
			set { _usagePage = value; OnPropertyChanged(); }
		}

		[DataMember]
		public ushort UsageId
		{
			get { return _usageId; }
			set { _usageId = value; OnPropertyChanged(); }
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public event PropertyChangedEventHandler PropertyChanged;

		private ushort _usagePage;
		private ushort _usageId;
	}

	[DataContract]
	public class PropertyInfoSurrogate
	{
		[DataMember(Name = "Type")]
		public string TypeFullName { get; set; }

		[DataMember]
		public string Name { get; set; }

		public static implicit operator PropertyInfoSurrogate(PropertyInfo value)
		{
			if (value == null)
				return null;

			return new PropertyInfoSurrogate { TypeFullName = value.DeclaringType.FullName, Name = value.Name };
		}

		public static implicit operator PropertyInfo(PropertyInfoSurrogate value) => Type.GetType(value?.TypeFullName)?.GetRuntimeProperty(value?.Name);
	}
}
