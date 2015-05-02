using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using TivaCopterMonitor.Model;
using System.Reflection;

namespace TivaCopterMonitor.DataAccessLayer
{
	public class BluetoothDeviceConnection : DeviceConnection
	{
		public BluetoothDeviceConnection(TaskScheduler UITaskScheduler)
			: base(UITaskScheduler)
		{
			DeviceSelector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
			_ConsoleBuffer = new StringBuilder(64);

			OnDeviceConnected += new TypedEventHandler<DeviceConnection, DeviceInformation>(async (connection, deviceInfo) =>
			{
				if (IsDeviceConnected)
				{
					// Create a standard networking socket and connect to the target
					_socket = new StreamSocket();

					_connectAction = _socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

					try
					{
						await _connectAction.AsTask().ContinueWith(async (task) =>
						{
							if (task.Status != TaskStatus.RanToCompletion)
							{
								CloseDevice();
								return;
							}

							_writer = new DataWriter(_socket.OutputStream);
							_writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

							_reader = new DataReader(_socket.InputStream);
							_reader.InputStreamOptions = InputStreamOptions.Partial;
							_reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

							// String builder storing the last received JSON string.
							StringBuilder _JSONRawData = new StringBuilder();

							_isSocketConnected = true;
							OnSocketConnected?.Invoke(this, deviceInfo);

							// Receiving loop
							while (_reader != null)
							{
								uint size;
								if (_isJSONCommunicationStarted)
									size = await _reader.LoadAsync(512);
								else
									size = await _reader.LoadAsync(sizeof(byte));

								if (size < sizeof(byte))
								{
									// The underlying socket was closed before we were able to read the whole data 
									CloseDevice();
									break;
								}

								while (_reader.UnconsumedBufferLength > 0)
								{
									var c = (char)_reader.ReadByte();

									if (_isJSONCommunicationStarted)
									{
										if (c == '\n')
										{
											try
											{
												var DeserializedData = JsonConvert.DeserializeObject<JSONDataSource>(_JSONRawData.ToString(), new JsonDataSourceConverter(), new BoolConverter());
												if (DeserializedData != null)
													OnJSONObjectReceived?.Invoke(this, DeserializedData);
											}
											catch (Newtonsoft.Json.JsonReaderException) { }
											finally
											{
												_JSONRawData.Clear();
											}
										}
										else
											_JSONRawData.Append(c);
									}
									else
									{
										// TODO: correct bug: "start" don't append in _ConsoleBuffer (add a boolean 'FirstJSONObjReceived' ?)
										// If the received data isn't JSON objects, add it to _ConsoleBuffer
										_ConsoleBuffer.Append(c);
									}
								}

								// Notify buffer changed
								if (!_isJSONCommunicationStarted)
									ConsoleBufferChanged?.Invoke(this, null);
							}
						}, UITaskScheduler);
					}
					catch (TaskCanceledException)
					{
						CloseDevice();
					}
				}
			});

			OnDeviceClose += new TypedEventHandler<DeviceConnection, DeviceInformation>((connection, deviceInfo) =>
			{
				if (_connectAction?.Status == AsyncStatus.Started)
					_connectAction?.Cancel();
				_connectAction = null;

				_reader?.Dispose();
				_reader = null;

				_writer?.Dispose();
				_writer = null;

				_isSocketConnected = false;
				_socket?.Dispose();
				_socket = null;
				OnSocketClose?.Invoke(this, deviceInfo);

				_isJSONCommunicationStarted = false;
				_connectAction = null;
			});

		}

		public event TypedEventHandler<BluetoothDeviceConnection, JSONDataSource> OnJSONObjectReceived;
		public event TypedEventHandler<BluetoothDeviceConnection, DeviceInformation> OnSocketConnected;
		public event TypedEventHandler<BluetoothDeviceConnection, DeviceInformation> OnSocketClose;
		public event EventHandler ConsoleBufferChanged;

		///<summary>
		/// Stores received data from bluetooth device except JSON objects.
		///</summary>
		public String ConsoleBuffer => _ConsoleBuffer.ToString();

		public void AbortConnection()
		{
			if (_connectAction?.Status == AsyncStatus.Started)
				_connectAction?.Cancel();
		}

		private async Task Send(String str)
		{
			if (_isSocketConnected && IsDeviceConnected)
			{
				_writer.WriteString(str);
				await _writer.StoreAsync();
				await _writer.FlushAsync();
			}
		}

		private async Task Send(Command cmd, params object[] args)
		{
			if (_isSocketConnected && IsDeviceConnected && !_isJSONCommunicationStarted)
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

		public async Task Send(JSONDataInput ctrl)
		{
			if (_isSocketConnected && IsDeviceConnected && _isJSONCommunicationStarted)
			{
				try
				{
					string SerializedData = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(ctrl, new BoolConverter()));
					await Send($"{SerializedData}\n");
				}
				catch (Newtonsoft.Json.JsonSerializationException) { }
			}
		}

		protected override async Task<object> GetDeviceAsync(DeviceInformation deviceInfo)
		{
			if (deviceInfo != null)
			{
				// Initialize the target Bluetooth Device
				_service = await RfcommDeviceService.FromIdAsync(deviceInfo.Id);
			}

			return _service;
		}

		#region CommandLineInterfaceEntries

		public async Task EnableDatasource(Type dsType)
		{
			if (dsType.GetTypeInfo().IsSubclassOf(typeof(JSONDataSource)))
			{
				await Send(Command.enable, dsType.Name);
			}
		}

		public async Task DisableDatasource(Type dsType)
		{
			if (dsType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(JSONDataSource)))
			{
				await Send(Command.disable, dsType.Name);
			}
		}

		public async Task Start()
		{
			await Send(Command.start);
			_isJSONCommunicationStarted = true;
		}

		public async Task Stop()
		{
			// Send CTRL+C character. (ETX=0x03)
			await Send("\x03");
			_isJSONCommunicationStarted = false;
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

		// TODO:
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

		private bool _isJSONCommunicationStarted;

		#endregion

		private IAsyncAction _connectAction;
		private RfcommDeviceService _service;
		private StreamSocket _socket;
		private DataWriter _writer;
		private DataReader _reader;
		private StringBuilder _ConsoleBuffer;

		private bool _isSocketConnected;
	}
}
