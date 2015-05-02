using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
			// TODO: entrer en paramètres du constructeur les vendor et product ids
			// Create a selector that gets a HID device using (TODO: VID/PID) manufacturer-defined usage
			DeviceSelector = HidDevice.GetDeviceSelector(usagePage, usageId);

			_numericControls = new List<HidNumericControlDescription>();

			OnDeviceConnected += new TypedEventHandler<DeviceConnection, DeviceInformation>((connection, deviceInfo) =>
			{
				if (IsDeviceConnected)
				{
					for (ushort page = 0; page < 255; page++)
						for (ushort id = 0; id < 255; id++)
							_numericControls.AddRange(_hidDevice.GetNumericControlDescriptions(HidReportType.Input, page, id));
					NumericControls = new ReadOnlyCollection<HidNumericControlDescription>(_numericControls);
					
					_hidDevice.InputReportReceived += new TypedEventHandler<HidDevice, HidInputReportReceivedEventArgs>((device, reportArgs) =>
					{
						Report = reportArgs.Report;
						OnHIDInputReportReceived?.Invoke(this, Report);
					});
				}
			});

			OnDeviceClose += new TypedEventHandler<DeviceConnection, DeviceInformation>((connection, deviceInfo) =>
			{

			});
		}

		protected override async Task<object> GetDeviceAsync(DeviceInformation deviceInfo)
		{
			if (deviceInfo != null)
			{
				// We use FileAccessMode.ReadWrite to open the device because we do not want other apps opening our device and changing the state of our device.
				_hidDevice = await HidDevice.FromIdAsync(deviceInfo.Id, FileAccessMode.ReadWrite);
			}
			return _hidDevice;
		}

		public event TypedEventHandler<HIDDeviceConnection, HidInputReport> OnHIDInputReportReceived;

		public HidInputReport Report { get; private set; }

		public IReadOnlyList<HidNumericControlDescription> NumericControls;
		public List<HidNumericControlDescription> _numericControls;


		private HidDevice _hidDevice;
	}
}

/// <summary>
/// HID Reports Descriptor for "N64 controller" and "Hama PC-Gamepad - Black Force"
/// </summary>
/* Hama PC-Gamepad - Black Force - HID Report Descriptor Joystick
	<Item Tag> (<Value>)						<Raw Data>
	Usage Page (Generic Desktop)				05 01  
	Usage (Joystick)							09 04  
	Collection (Application)						A1 01  
		Collection (Logical)							A1 02  
			Report Size (8)								75 08  
			Report Count (5)							95 05  
			Logical Minimum (0)							15 00  
			Logical Maximum (255)						26 FF 00  
			Physical Minimum (0)						35 00  
			Physical Maximum (255)						46 FF 00  
			Usage (X)									09 30  
			Usage (Y)									09 31  
			Usage (Z)									09 32  
			Usage (Z)									09 32  
			Usage (Rz)									09 35  
			Input (Data,Var,Abs,NWrp,Lin,Pref,NNul,Bit) 81 02  
			Report Size (4)								75 04  
			Report Count (1)							95 01  
			Logical Maximum (7)							25 07  
			Physical Maximum (315)						46 3B 01  
			Unit (Eng Rot: Degree)						65 14  
			Usage (Hat Switch)							09 39  
			Input (Data,Var,Abs,NWrp,Lin,Pref,Null,Bit) 81 42  
			Unit (None)									65 00  
			Report Size (1)								75 01  
			Report Count (12)							95 0C  
			Logical Maximum (1)							25 01  
			Physical Maximum (1)						45 01  
			Usage Page (Button) 						05 09  
			Usage Minimum (Button 1)					19 01  
			Usage Maximum (Button 12)					29 0C  
			Input (Data,Var,Abs,NWrp,Lin,Pref,NNul,Bit) 81 02  
			Usage Page (Vendor-Defined 1)				06 00 FF  
			Report Size (1)								75 01  
			Report Count (8)							95 08  
			Logical Maximum (1)							25 01  
			Physical Maximum (1)						45 01  
			Usage (Vendor-Defined 1)					09 01  
			Input (Data,Var,Abs,NWrp,Lin,Pref,NNul,Bit) 81 02  
		End Collection								C0  
		Collection (Logical)						A1 02  
			Report Size (8)								75 08  
			Report Count (7) 							95 07  
			Physical Maximum (255)						46 FF 00  
			Logical Maximum (255)						26 FF 00  
			Usage (Vendor-Defined 2)					09 02  
			Output (Data,Var,Abs,NWrp,Lin,Pref,NNul,NVol,Bit) 91 02  
		End Collection								C0
	End Collection								C0
	*/

/* N64 Controller - HID Report Descriptor Joystick
<Item Tag> (<Value>)						Raw Data 
Usage Page (Generic Desktop)				05 01  
Usage (Joystick)							09 04  
Collection (Application)					A1 01  
	Usage Page (Generic Desktop)				05 01  
	Usage (Pointer)								09 01  
	Collection (Physical)						A1 00  
		Usage (X)									09 30  
		Usage (Y)									09 31  
		Report Size (8)								75 08  
		Report Count (2)							95 02  
		Input (Data,Var,Abs,NWrp,Lin,Pref,NNul,Bit) 81 02  
		Usage Page (Button)							05 09  
		Usage Minimum (Button 1)					19 01  
		Usage Maximum (Button 16)					29 10  
		Logical Minimum (0)							15 00  
		Logical Maximum (1)							25 01  
		Physical Minimum (0)						36 00 00  
		Physical Maximum (1)						46 01 00  
		Report Size (1)								75 01  
		Report Count (16)							95 10  
		Input (Data,Var,Abs,NWrp,Lin,Pref,NNul,Bit)	81 02  
	End Collection								C0  
End Collection								C0  
*/
