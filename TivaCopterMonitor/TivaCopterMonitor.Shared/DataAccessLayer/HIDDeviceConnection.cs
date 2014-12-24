using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace TivaCopterMonitor.DataAccessLayer
{
	public class HIDDeviceConnection : DeviceConnection
	{
		public HIDDeviceConnection(TaskScheduler UITaskScheduler, UInt16 usagePage, UInt16 usageId)
			: base(UITaskScheduler)
		{
			//DeviceSelector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);

			OnDeviceConnected += new TypedEventHandler<DeviceConnection, DeviceInformation>((connection, deviceInfo) =>
			{

			});

			OnDeviceClose += new TypedEventHandler<DeviceConnection, DeviceInformation>((connection, deviceInfo) =>
			{

			});
		}

		protected override async Task<object> GetDeviceAsync(DeviceInformation deviceInfo)
		{
			// We use FileAccessMode.ReadWrite to open the device because we do not want other apps opening our device and changing the state of our device.
			return await HidDevice.FromIdAsync(deviceInfo.Id, FileAccessMode.ReadWrite);
		}

	}
}