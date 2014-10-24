using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TivaCopterMonitor.Common;
using System.Collections.ObjectModel;
using System.Reflection;
using Windows.Devices.Enumeration;
using TivaCopterMonitor.Model;
using Windows.Foundation;
using Windows.Networking.Sockets;

namespace TivaCopterMonitor.ViewModel
{
	public class TivaCopterViewModel : Common.PropertyChangedBase
	{
		public TivaCopterViewModel()
		{
			_bluetoothConnection = new DataAccessLayer.BluetoothCmdLineInterface(TaskScheduler.FromCurrentSynchronizationContext());
			//_DataHistory = new List<JSONDataSource>();

			ConnectCommand = new RelayCommand(async command =>
			{
				if (_bluetoothConnection != null && SelectedBluetoothDevice != null)
					await _bluetoothConnection.ConnectDevice(SelectedBluetoothDevice);
			});

			_bluetoothConnection.StateChanged += new EventHandler(async (sender, args) =>
			{
				//Raise property changed event on UI thread
				await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
				{
					OnPropertyChanged("BluetoothState");
				});

				// If we just get connected to tivacopter, send commands to enable programmatic access mode, enable datasources and start JSON communication.
				if (_bluetoothConnection.State == DataAccessLayer.BluetoothConnectionState.Connected)
				{
					await Task.Delay(TimeSpan.FromSeconds(0.5));
					await _bluetoothConnection.SetProgramaticAccessMode(true);
					await Task.Delay(TimeSpan.FromSeconds(0.1));
					await _bluetoothConnection.EnableDatasource(typeof(IMU));
					await Task.Delay(TimeSpan.FromSeconds(0.1));
					await _bluetoothConnection.EnableDatasource(typeof(PID));
					await Task.Delay(TimeSpan.FromSeconds(0.1));
					await _bluetoothConnection.EnableDatasource(typeof(sensors));
					await Task.Delay(TimeSpan.FromSeconds(0.1));
					await _bluetoothConnection.EnableDatasource(typeof(radio));
					await Task.Delay(TimeSpan.FromSeconds(0.1));
					await _bluetoothConnection.Start();
					await Task.Delay(TimeSpan.FromSeconds(0.1));
				}
			});

			_bluetoothConnection.CmdLineBufferChanged += new EventHandler(async (sender, args) =>
			{
				//Raise event on UI thread
				await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
				{
					OnPropertyChanged("BluetoothBuffer");
				});
			});

			_bluetoothConnection.JSONObjectReceived += new TypedEventHandler<DataAccessLayer.BluetoothCmdLineInterface, JSONDataSource>((sender, data) =>
			{
				//_DataHistory.Add(data);

				if (data is IMU)
					IMU = data as IMU;
				else if (data is PID)
					PID = data as PID;
				else if (data is sensors)
					Sensors = data as sensors;
				else if (data is radio)
					Radio = data as radio;
			});
		}

		public async Task RefreshBluetoothDevices()
		{
			await _bluetoothConnection.EnumerateDevicesAsync();

			if (_bluetoothConnection.PairedDevices.Count != 0)
				SelectedBluetoothDevice = _bluetoothConnection.PairedDevices[0];
			else
				SelectedBluetoothDevice = null;
		}

		#region Properties

		public ObservableCollection<DeviceInformation> BluetoothPairedDevices
		{
			get { return _bluetoothConnection.PairedDevices; }
		}

		public DataAccessLayer.BluetoothConnectionState BluetoothState
		{
			get { return _bluetoothConnection.State; }
		}

		public RelayCommand ConnectCommand { get; set; }

		public DeviceInformation SelectedBluetoothDevice { get; set; }

		public String BluetoothBuffer
		{
			get { return _bluetoothConnection.CmdLineBuffer; }
		}

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

		#endregion

		#region Members

		protected DataAccessLayer.BluetoothCmdLineInterface _bluetoothConnection;

		protected IMU _IMU;
		protected PID _PID;
		protected sensors _sensors;
		protected radio _radio;

		//protected IList<JSONDataSource> _DataHistory;

		#endregion
	}
}
