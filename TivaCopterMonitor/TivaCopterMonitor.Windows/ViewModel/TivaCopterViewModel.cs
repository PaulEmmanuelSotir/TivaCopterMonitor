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
			_bluetoothConnection = new DataAccessLayer.BluetoothCmdLineInterface();

			ConnectCommand = new RelayCommand(async command =>
			{
				if (_bluetoothConnection != null && SelectedBluetoothDevice != null)
					await _bluetoothConnection.ConnectDevice(SelectedBluetoothDevice);
			});

			_bluetoothConnection.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(async (sender, arg) =>
			{
				if (arg != null)
				{
					if (arg.PropertyName == "State")
					{
						OnPropertyChanged("BluetoothState");

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
							await _bluetoothConnection.EnableDatasource(typeof(Radio));
							await Task.Delay(TimeSpan.FromSeconds(0.1));
							await _bluetoothConnection.Start();
							await Task.Delay(TimeSpan.FromSeconds(0.1));
						}
					}
					if(arg.PropertyName == "CmdLineBuffer")
					{
						OnPropertyChanged("BluetoothBuffer");
					}
				}
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

		public Radio Radio
		{
			get { return _radio; }
			protected set { _radio = value; OnPropertyChanged(); }
		}

		#endregion

		#region Members

		protected DataAccessLayer.BluetoothCmdLineInterface _bluetoothConnection;

		protected IMU _IMU;
		protected IList<IMU> _IMUHistory;

		protected PID _PID;
		protected IList<PID> _PIDHistory;

		protected sensors _sensors;
		protected IList<sensors> _sensorsHistory;

		protected Radio _radio;
		protected IList<sensors> _radioHistory;

		#endregion
	}
}
