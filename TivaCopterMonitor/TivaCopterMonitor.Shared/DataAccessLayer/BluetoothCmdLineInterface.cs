using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.Networking.Sockets;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using System.Runtime.Serialization.Json;
using TivaCopterMonitor.Model;

namespace TivaCopterMonitor.DataAccessLayer
{
	public class BluetoothCmdLineInterface : Common.PropertyChangedBase, IDisposable
	{
		public BluetoothCmdLineInterface()
		{
			PairedDevices = new ObservableCollection<DeviceInformation>();
			_stringBuilder = new StringBuilder(256);
			_cmdLineStringBuilder = new StringBuilder(64);
			_state = BluetoothConnectionState.Disconnected;
		}

		#region Events

		public delegate void BluetoothExDelegate(BluetoothCmdLineInterface sender, Exception ex);

		public event BluetoothExDelegate ExceptionOccured;
		public event TypedEventHandler<BluetoothCmdLineInterface, IJSONDataSource> JSONObjectReceived;

		#endregion

		#region Properties

		public ObservableCollection<DeviceInformation> PairedDevices
		{
			get;
			private set;
		}

		public BluetoothConnectionState State
		{
			get { return _state; }
			private set { _state = value; OnPropertyChanged(); }
		}

		///<summary>
		/// Stores all received data from bluetooth device.
		///</summary>
		public String Buffer
		{
			get
			{
				return _stringBuilder.ToString();
			}
		}

		///<summary>
		/// Stores received data from bluetooth device except JSON objects.
		///</summary>
		public String CmdLineBuffer
		{
			get
			{
				return _cmdLineStringBuilder.ToString();
			}
		}

		public StreamSocketListener Listener
		{
			get;
			private set;
		}

		#endregion

		#region BluetoothCommunication

		public async Task EnumerateDevicesAsync()
		{
			try
			{
				var lastState = State;
				State = BluetoothConnectionState.Enumerating;

				var DeviceInfos = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

				PairedDevices.Clear();

				if (DeviceInfos.Count > 0)
				{
					foreach (var deviceInfo in DeviceInfos)
					{
						PairedDevices.Add(deviceInfo);
					}
				}

				State = lastState;
			}
			catch (Exception ex)
			{
				Disconnect();
				ExceptionOccured(this, ex);
			}
		}

		public async Task ConnectDevice(DeviceInformation DeviceInfo)
		{
			// Close current connection if any
			Disconnect();

			State = BluetoothConnectionState.Connecting;

			try
			{
				// Initialize the target Bluetooth Device
				_service = await RfcommDeviceService.FromIdAsync(DeviceInfo.Id);

				// Create a standard networking socket and connect to the target
				_socket = new StreamSocket();

				_connectAction = _socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

				await _connectAction.AsTask().ContinueWith(async (task) =>
					{
						_writer = new DataWriter(_socket.OutputStream);
						_writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

						_reader = new DataReader(_socket.InputStream);
						_reader.InputStreamOptions = InputStreamOptions.Partial;
						_reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

						// Update State on UI thread
						await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
						{ State = BluetoothConnectionState.Connected; });

						// Receiving loop
						while (true)
						{
							var size = await _reader.LoadAsync(sizeof(byte));

							if (size != sizeof(byte))
								// The underlying socket was closed before we were able to read the whole data 
								throw new Exception("Bluetooth connection lost.");

							char c = (char)_reader.ReadByte();
							_stringBuilder.Append(c);

							// TODO: correct bug: "start" don't append in _cmdLineStringBuilder
							// If the received data isn't JSON objects, add it to _cmdLineStringBuilder
							if (!_JSONCommunicationStarted)
								_cmdLineStringBuilder.Append(c);

							// Notify property changed on UI thread
							await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
							{
								OnPropertyChanged("Buffer");
								if (!_JSONCommunicationStarted)
									OnPropertyChanged("CmdLineBuffer");
							});

						}
					});
			}
			catch (TaskCanceledException)
			{
				Disconnect();
			}
			catch (Exception ex)
			{
				Disconnect();
				ExceptionOccured(this, ex);
			}
		}

		public void AbortConnection()
		{
			if (_connectAction != null && _connectAction.Status == AsyncStatus.Started)
				_connectAction.Cancel();
		}

		public void Disconnect()
		{
			if (_reader != null)
			{
				_reader.DetachStream();
				_reader.Dispose();
				_reader = null;
			}
			if (_writer != null)
			{
				_writer.DetachStream();
				_writer.Dispose();
				_writer = null;
			}
			if (_socket != null)
			{
				_socket.Dispose();
				_socket = null;
			}
			if (_service != null)
			{
				_service = null;
			}
			_JSONCommunicationStarted = false;
			State = BluetoothConnectionState.Disconnected;
		}

		public void Dispose()
		{
			Disconnect();
		}

		private async Task Send(String str)
		{
			if (_state == BluetoothConnectionState.Connected)
			{
				_writer.WriteString(str);
				await _writer.StoreAsync();
				await _writer.FlushAsync();
			}
		}

		private async Task Send(Command cmd, params object[] args)
		{
			if (_state == BluetoothConnectionState.Connected && !_JSONCommunicationStarted)
			{
				string str = cmd.ToString();
				foreach (var arg in args)
				{
					str += " " + arg.ToString();
				}
				str += "\n";

				await Send(str);
			}
		}

		/*public async Task Send(JSONRemoteControl rmctrl)
		{
			if (_state == BluetoothConnectionState.Connected && _JSONStarted)
			{
				//TODO: serialize rmctrl
			}
		}*/

		private IAsyncAction _connectAction;
		private RfcommDeviceService _service;
		private StreamSocket _socket;
		private DataWriter _writer;
		private DataReader _reader;
		private StringBuilder _stringBuilder;
		private StringBuilder _cmdLineStringBuilder;
		private BluetoothConnectionState _state;

		#endregion

		#region CommandLineInterfaceEntries

		public async Task EnableDatasource(Type dsType)
		{
			if (dsType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IJSONDataSource)))
			{
				await Send(Command.enable, dsType.Name);
			}
		}

		public async Task DisableDatasource(Type dsType)
		{
			if (dsType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IJSONDataSource)))
			{
				await Send(Command.disable, dsType.Name);
			}
		}

		public async Task Start()
		{
			await Send(Command.start);
			_JSONCommunicationStarted = true;
		}

		public async Task Stop()
		{
			// Send CTRL+C character. (ETX=0x03)
			await Send("\x03");
			_JSONCommunicationStarted = false;
		}

		public async Task SetProgramaticAccessMode(bool enabled)
		{
			await Send(enabled ? Command.progModeEn : Command.progModeDis);
		}

		public async Task ListSources()
		{
			await Send(Command.listSources);
		}

		public async Task I2CSelect(TivaHardware.I2CBase periph)
		{
			await Send(Command.i2cSelect, periph);
		}

		public async Task I2CRegReadModifyWrite(uint SlaveAddress, uint RegisterAddress, byte NewValue, byte Mask)
		{
			await Send(Command.i2cregrmw, SlaveAddress, RegisterAddress, NewValue, Mask);
		}

		public async Task I2CRegWrite(uint SlaveAddress, uint RegisterAddress, byte NewValue)
		{
			await Send(Command.i2cregw, SlaveAddress, RegisterAddress, NewValue);
		}

		/*public async Task<byte> I2CRegRead(uint SlaveAddress, uint RegisterAddress)
		{
			Send(Command.i2cregr, SlaveAddress, RegisterAddress);

			//await reader... 
			return 0x00;
		}*/

		/*public async Task I2CWrite(uint SlaveAddress, byte[] values)
		{
			Send((String)SlaveAddress);

			foreach(value in values)
			{
				Send((String)value);
			}
		}*/

		private enum Command
		{
			help,
			i2cSelect,
			i2cregr,
			i2cregw,
			i2cregrmw,
			i2cw,
			listSources,
			enable,
			disable,
			start,
			progModeEn,
			progModeDis
		}

		private bool _JSONCommunicationStarted;

		#endregion

	}

	public enum BluetoothConnectionState
	{
		Disconnected,
		Connected,
		Enumerating,
		Connecting
	}
}
