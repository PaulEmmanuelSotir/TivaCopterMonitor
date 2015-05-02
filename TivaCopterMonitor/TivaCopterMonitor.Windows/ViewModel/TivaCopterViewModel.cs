using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using TivaCopterMonitor.Common;
using TivaCopterMonitor.DataAccessLayer;
using TivaCopterMonitor.Model;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;

namespace TivaCopterMonitor.ViewModel
{
	public class TivaCopterViewModel : PropertyChangedBase, IDisposable
	{
		public TivaCopterViewModel()
		{
			var UITaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			_bluetoothConnection = new BluetoothDeviceConnection(UITaskScheduler);
			_hidConnection = new HIDDeviceConnection(UITaskScheduler, 0x0001, 0x0004);

			_controlMap = new HIDDataMap<RemoteControl>();

			// TODO: enlever ça ou l'implémenter.
			//_DataHistory = new List<JSONDataSource>();

			ConnectCommand = new RelayCommand(async param =>
			{
				if (_bluetoothConnection != null && SelectedBluetoothDevice != null)
				{
					try
					{
						await _bluetoothConnection.OpenDeviceAsync(SelectedBluetoothDevice);
					}
					catch (System.Exception)
					{
						IsConnectionFailedPopupOpen = true;
					}
				}
			});

			OkConnectionFailedPopupCommand = new RelayCommand(param =>
		   {
			   IsConnectionFailedPopupOpen = false;
		   });

			ChangeControlSettingCommand = new RelayCommand(param =>
			{
				if (_controlMap != null && param is PropertyToHidAttributeBinding && _hidConnection.IsDeviceConnected)
				{
					IsControlsSettingPopupOpen = true;
					_waitingHIDToControlBinding = param as PropertyToHidAttributeBinding;

					HidInputReport lastInputReport;

					_controlsSettingsWaitForHidKey_EventHandler = new EventHandler<object>((sender, arg) =>
					{
						if (((System.Reflection.PropertyInfo)_waitingHIDToControlBinding?.Property).PropertyType == typeof(bool))
						{
							// TODO : savoir si ActivatedBooleanControls peut réellement devenir null
							if (_hidConnection.Report.ActivatedBooleanControls?.Count > 0)
							{
								var ActivatedBooleanControls = _hidConnection.Report.ActivatedBooleanControls;
								if (ActivatedBooleanControls.Count > 0)
								{
									var ActivatedBooleanControl = ActivatedBooleanControls[0];
									_waitingHIDToControlBinding.UsageId = ActivatedBooleanControl.UsageId;
									_waitingHIDToControlBinding.UsagePage = ActivatedBooleanControl.UsagePage;
								}

								CloseControlsSettingPopup();
							}
						}
						else
						{
							// TODO: check changed numeric values by comparing lastInputReport with _hidConnection.Report
							lastInputReport = _hidConnection.Report;
						}
					});
					_timer.Tick += _controlsSettingsWaitForHidKey_EventHandler;
					_hidConnection.OnDeviceClose += CloseControlsSettingPopup;
				}
			});

			CancelControlsSettingPopupCommand = new RelayCommand(param => { CloseControlsSettingPopup(); });

			_hidConnection.OnDeviceConnected += StartHidInputListening;
			_hidConnection.OnDeviceClose += new TypedEventHandler<DeviceConnection, DeviceInformation>((connection, deviceInfo) =>
			 {
				 _timer?.Stop();
			 });

			_bluetoothConnection.OnSocketConnected += StartTivaCopterCommunication;
			_bluetoothConnection.ConsoleBufferChanged += UpdateConsoleBuffer;
			_bluetoothConnection.OnJSONObjectReceived += UpdateReceivedJSONObject;

		}

		#region Methods

		public void StartHidInputListening(DeviceConnection connection, DeviceInformation deviceInfo)
		{
			if (_bluetoothConnection.IsDeviceConnected)
			{
				if (_timer == null)
				{
					_timer = new DispatcherTimer();
					_timer.Tick += new EventHandler<object>(async (sender, arg) =>
					{
						RemoteCtrl = ControlMap.GetDataFromHidInputReport(_hidConnection.Report);
						await _bluetoothConnection.Send(RemoteCtrl);
					});

					_timer.Interval = TimeSpan.FromMilliseconds(50);
				}

				_timer.Start();
			}
		}

		public async void StartTivaCopterCommunication(BluetoothDeviceConnection connection, DeviceInformation deviceInfo)
		{
			// TODO: let user choose the HID device
			await _hidConnection.EnumerateDevicesAsync();
			if (_hidConnection.AvailableDevices.Count > 0)
				await _hidConnection.OpenDeviceAsync(_hidConnection.AvailableDevices[0]);

			if (_hidConnection.IsDeviceConnected)
			{
				StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("HIDControlMap.xml", CreationCollisionOption.OpenIfExists);

				//ControlMap.DataMap[0].UsagePage = 0x0001;
				//ControlMap.DataMap[0].UsageId = 0x0031;
				//ControlMap.DataMap[1].UsagePage = 0x0001;
				//ControlMap.DataMap[1].UsageId = 0x0031;
				//ControlMap.DataMap[2].UsagePage = 0x0001;
				//ControlMap.DataMap[2].UsageId = 0x0030;
				//ControlMap.DataMap[3].UsagePage = 0x0001;
				//ControlMap.DataMap[3].UsageId = 0x0030;
				//ControlMap.DataMap[4].UsagePage = 0x0009;
				//ControlMap.DataMap[4].UsageId = 0x000C;
				//ControlMap.DataMap[5].UsagePage = 0x0009;
				//ControlMap.DataMap[5].UsageId = 0x0001;
				//var ser = new System.Runtime.Serialization.DataContractSerializer(typeof(HIDDataMap<RemoteControl>));
				//ser.WriteObject(await file.OpenStreamForWriteAsync(), ControlMap);

				//serializer.Serialize(System.Xml.XmlWriter.Create(await file.OpenStreamForWriteAsync()), ControlMap);

				var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(HIDDataMap<RemoteControl>));

				var result = serializer.ReadObject(await file.OpenStreamForReadAsync());
				if (result.GetType() == typeof(HIDDataMap<RemoteControl>))
					ControlMap = result as HIDDataMap<RemoteControl>;
			}

			// TODO: faire mieux que ça !!
			await Task.Delay(TimeSpan.FromSeconds(3));
			await _bluetoothConnection.Start();
			await Task.Delay(TimeSpan.FromSeconds(0.1));

		}

		public async void UpdateConsoleBuffer(object sender, EventArgs args)
		{
			// Raise event on UI thread
			await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, ()
				=> OnPropertyChanged("ConsoleBuffer"));
		}

		public void UpdateReceivedJSONObject(BluetoothDeviceConnection conection, JSONDataSource dataSource)
		{
			// TODO: enlever ça ou l'implémenter.
			//_DataHistory.Add(dataSource);

			if (dataSource is IMU)
				IMU = dataSource as IMU;
			else if (dataSource is PID)
				PID = dataSource as PID;
			else if (dataSource is sensors)
				Sensors = dataSource as sensors;
			else if (dataSource is radio)
				Radio = dataSource as radio;
			else if (dataSource is rawEcho)
				RawEcho = dataSource as rawEcho;
		}

		public void CloseControlsSettingPopup(DeviceConnection connection = null, Object arg = null)
		{
			IsControlsSettingPopupOpen = false;
			_waitingHIDToControlBinding = null;
			_timer.Tick -= _controlsSettingsWaitForHidKey_EventHandler;
			_hidConnection.OnDeviceClose -= CloseControlsSettingPopup;
		}

		public async Task RefreshBluetoothDevices()
		{
			await _bluetoothConnection.EnumerateDevicesAsync();

			if (_bluetoothConnection.AvailableDevices.Count != 0)
				SelectedBluetoothDevice = _bluetoothConnection.AvailableDevices[0];
			else
				SelectedBluetoothDevice = null;
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// Dispose managed resources.
					_hidConnection.OnDeviceConnected -= StartHidInputListening;
					_timer.Stop();
					_timer = null;

					_bluetoothConnection.OnSocketConnected -= StartTivaCopterCommunication;
					_bluetoothConnection.ConsoleBufferChanged -= UpdateConsoleBuffer;
					_bluetoothConnection.OnJSONObjectReceived -= UpdateReceivedJSONObject;

					if (IsControlsSettingPopupOpen)
						CloseControlsSettingPopup();

					IsConnectionFailedPopupOpen = false;

					_hidConnection.Dispose();
					_bluetoothConnection.Dispose();
				}

				// Call the appropriate methods to clean upunmanaged resources here. 
				// ...

				disposed = true;
			}
		}

		private bool disposed = false;

		#endregion

		#region Properties

		public ObservableCollection<DeviceInformation> BluetoothPairedDevices => _bluetoothConnection.AvailableDevices;

		public RelayCommand ConnectCommand { get; private set; }

		public bool IsConnectionFailedPopupOpen
		{
			get { return _isConnectionFailedPopupOpen; }
			protected set { _isConnectionFailedPopupOpen = value; OnPropertyChanged(); }
		}

		public RelayCommand OkConnectionFailedPopupCommand { get; private set; }

		public RelayCommand ChangeControlSettingCommand { get; private set; }

		public RelayCommand CancelControlsSettingPopupCommand { get; private set; }

		public bool IsControlsSettingPopupOpen
		{
			get { return _isControlsSettingPopupOpen; }
			protected set { _isControlsSettingPopupOpen = value; OnPropertyChanged(); }
		}

		public DeviceInformation SelectedBluetoothDevice { get; set; }

		public string ConsoleBuffer => _bluetoothConnection.ConsoleBuffer;

		public IMU IMU
		{
			get { return _IMU; }
			protected set { _IMU = value; OnPropertyChanged(); }
		}

		public PID PID
		{
			get { return _PID; }
			protected set { _PID = value; OnPropertyChanged(); }
		}

		public sensors Sensors
		{
			get { return _sensors; }
			protected set { _sensors = value; OnPropertyChanged(); }
		}

		public radio Radio
		{
			get { return _radio; }
			protected set { _radio = value; OnPropertyChanged(); }
		}

		public RemoteControl RemoteCtrl
		{
			get { return _remoteCtrl; }
			protected set { _remoteCtrl = value; OnPropertyChanged(); }
		}

		public rawEcho RawEcho
		{
			get { return _rawEcho; }
			protected set { _rawEcho = value; OnPropertyChanged(); }
		}

		public HIDDataMap<RemoteControl> ControlMap
		{
			get { return _controlMap; }
			protected set { _controlMap = value; OnPropertyChanged(); }
		}

		#endregion

		#region Members

		protected BluetoothDeviceConnection _bluetoothConnection;
		private bool _isConnectionFailedPopupOpen;

		private IMU _IMU;
		private PID _PID;
		private sensors _sensors;
		private radio _radio;
		private RemoteControl _remoteCtrl;
		private rawEcho _rawEcho;

		protected HIDDeviceConnection _hidConnection;
		private DispatcherTimer _timer;

		private HIDDataMap<RemoteControl> _controlMap;
		private PropertyToHidAttributeBinding _waitingHIDToControlBinding;
		private EventHandler<object> _controlsSettingsWaitForHidKey_EventHandler;
		private bool _isControlsSettingPopupOpen;

		// TODO : enlever ça ou l'implémenter.
		//protected IList<JSONDataSource> _DataHistory;

		#endregion

	}
}
