using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace TivaCopterMonitor.DataAccessLayer
{
	public abstract class DeviceConnection : IDisposable
	{
		protected DeviceConnection(TaskScheduler UITaskScheduler)
		{
			this.UITaskScheduler = UITaskScheduler;
			AvailableDevices = new ObservableCollection<DeviceInformation>();
			IsEnabledAutoReconnect = true;
		}

		public event TypedEventHandler<DeviceConnection, DeviceInformation> OnDeviceClose;
		public event TypedEventHandler<DeviceConnection, DeviceInformation> OnDeviceConnected;

		#region Properties

		public bool IsDeviceConnected => _device != null;

		public ObservableCollection<DeviceInformation> AvailableDevices { get; private set; }

		/// <summary>
		/// This DeviceInformation represents which device is connected or which device will be reconnected when
		/// the device is plugged in again (if IsEnabledAutoReconnect is true);.
		/// </summary>
		public DeviceInformation DeviceInformation { get; private set; }

		/// <summary>
		/// Returns DeviceAccessInformation for the device that is currently connected using this DeviceConnection object.
		/// </summary>
		public DeviceAccessInformation DeviceAccessInformation { get; private set; }

		/// <summary>
		/// DeviceSelector AQS used to find this device
		/// </summary>
		public string DeviceSelector { get; protected set; }

		/// <summary>
		/// True if DeviceConnection will attempt to reconnect to the device once it is available to the computer again
		/// </summary>
		public bool IsEnabledAutoReconnect { get; set; }

		public TaskScheduler UITaskScheduler { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// This method opens the device (using the abstract method GetDeviceAsync).
		/// </summary>
		/// <param name="deviceInfo">Device information of the device to be opened</param>
		/// <returns>True if the device was successfully opened, false if the device could not be opened for well known reasons.
		/// An exception may be thrown if the device could not be opened for extraordinary reasons.</returns>
		/// <remarks>This method should be called from UI thread. (may show authorization popup)</remarks>
		public async Task OpenDeviceAsync(DeviceInformation deviceInfo)
		{
			_device = await GetDeviceAsync(deviceInfo);

			// Device could have been blocked by user or the device has already been opened by another app.
			if (IsDeviceConnected)
			{
				DeviceInformation = deviceInfo;

				if (_appSuspendEventHandler == null || _appResumeEventHandler == null)
					RegisterForAppEvents();

				// User can block the device after it has been opened in the Settings charm. We can detect this by registering for the DeviceAccessInformation.AccessChanged event
				if (_deviceAccessEventHandler == null)
					RegisterForDeviceAccessStatusChange();

				// Create and register device watcher events for the device to be opened unless we're reopening the device
				if (_deviceWatcher == null)
				{
					//		try
					//		{
					_deviceWatcher = DeviceInformation.CreateWatcher(DeviceSelector);
					//		}
					//		catch (UnauthorizedAccessException)
					//		{
					//			// TODO: show error message / throw error event
					//			return;
					//		}

					RegisterForDeviceWatcherEvents();
				}

				if (!_watcherStarted)
				{
					// Start the device watcher after we made sure that the device is opened.
					StartDeviceWatcher();
				}

				// Notify registered callback handle that the device has been opened
				OnDeviceConnected?.Invoke(this, DeviceInformation);
			}
		}

		/// <summary>
		/// Closes the device, stops the device watcher, stops listening for app events, and resets object state to before a device was ever connected.
		/// </summary>
		public void CloseDevice()
		{
			if (IsDeviceConnected)
				CloseCurrentlyConnectedDevice();

			if (_deviceWatcher != null)
			{
				if (_watcherStarted)
				{
					StopDeviceWatcher();
					UnregisterFromDeviceWatcherEvents();
				}

				_deviceWatcher = null;
			}

			if (DeviceAccessInformation != null)
			{
				UnregisterFromDeviceAccessStatusChange();
				DeviceAccessInformation = null;
			}

			if (_appSuspendEventHandler != null || _appResumeEventHandler != null)
				UnregisterFromAppEvents();

			DeviceInformation = null;

			// TODO avant la ligne était : "IsEnabledAutoReconnect = true;", comprendre pourquoi!
			IsEnabledAutoReconnect = false;
		}

		public async Task EnumerateDevicesAsync()
		{
			try
			{
				var DeviceInfos = await DeviceInformation.FindAllAsync(DeviceSelector);

				AvailableDevices.Clear();

				if (DeviceInfos.Count > 0)
					foreach (var deviceInfo in DeviceInfos)
						AvailableDevices.Add(deviceInfo);
			}
			catch (Exception)
			{
				CloseDevice();
			}
		}

		/// <summary>
		/// Must set _device handle by calling 'FromIdAsync'
		/// </summary>
		protected abstract Task<object> GetDeviceAsync(DeviceInformation deviceInfo);

		/// <summary>
		/// Closes the device
		/// </summary>
		/// <remarks>When the device is closing, it will cancel all IO operations that are still pending (not complete).</remarks>
		private void CloseCurrentlyConnectedDevice()
		{
			if (IsDeviceConnected)
			{
				// Notify callback that we're about to close the device
				OnDeviceClose?.Invoke(this, DeviceInformation);

				// This closes the handle to the device if device is disposable
				// TODO: comprendre pourquoi disposer les device cause un exception et décommenter ça :
				//		if (_device is IDisposable)
				//			(_device as IDisposable).Dispose();

				_device = null;
			}
		}

		/// <summary>
		/// Register for app suspension/resume events
		/// </summary>
		private void RegisterForAppEvents()
		{
			_appSuspendEventHandler = new SuspendingEventHandler(this.OnAppSuspension);
			_appResumeEventHandler = new EventHandler<Object>(this.OnAppResume);

			// This event is raised when the app is exited and when the app is suspended

			Application.Current.Suspending += _appSuspendEventHandler;
			Application.Current.Resuming += _appResumeEventHandler;
		}

		private void UnregisterFromAppEvents()
		{
			// This event is raised when the app is exited and when the app is suspended
			Application.Current.Suspending -= _appSuspendEventHandler;
			_appSuspendEventHandler = null;

			Application.Current.Resuming -= _appResumeEventHandler;
			_appResumeEventHandler = null;
		}

		/// <summary>
		/// Register for Added and Removed events.
		/// Note that, when disconnecting the device, the device may be closed by the system before the OnDeviceRemoved callback is invoked.
		/// </summary>
		private void RegisterForDeviceWatcherEvents()
		{
			_deviceAddedEventHandler = new TypedEventHandler<DeviceWatcher, DeviceInformation>(this.OnDeviceAdded);
			_deviceRemovedEventHandler = new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(this.OnDeviceRemoved);

			_deviceWatcher.Added += _deviceAddedEventHandler;
			_deviceWatcher.Removed += _deviceRemovedEventHandler;
		}

		private void UnregisterFromDeviceWatcherEvents()
		{
			_deviceWatcher.Added -= _deviceAddedEventHandler;
			_deviceAddedEventHandler = null;

			_deviceWatcher.Removed -= _deviceRemovedEventHandler;
			_deviceRemovedEventHandler = null;
		}

		/// <summary>
		/// Listen for any change in device access permission. The user can block access to the device while the device is in use.
		/// If the user blocks access to the device while the device is opened, the device's handle will be closed automatically by
		/// the system; it is still a good idea to close the device explicitly so that resources are cleaned up.
		/// </summary>
		private void RegisterForDeviceAccessStatusChange()
		{
			DeviceAccessInformation = DeviceAccessInformation.CreateFromId(DeviceInformation.Id);

			_deviceAccessEventHandler = new TypedEventHandler<DeviceAccessInformation, DeviceAccessChangedEventArgs>(this.OnDeviceAccessChanged);
			DeviceAccessInformation.AccessChanged += _deviceAccessEventHandler;
		}

		private void UnregisterFromDeviceAccessStatusChange()
		{
			DeviceAccessInformation.AccessChanged -= _deviceAccessEventHandler;

			_deviceAccessEventHandler = null;
		}

		private void StartDeviceWatcher()
		{
			_watcherStarted = true;

			if ((_deviceWatcher.Status != DeviceWatcherStatus.Started) && (_deviceWatcher.Status != DeviceWatcherStatus.EnumerationCompleted))
				_deviceWatcher.Start();
		}

		private void StopDeviceWatcher()
		{
			if ((_deviceWatcher.Status == DeviceWatcherStatus.Started) || (_deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted))
				_deviceWatcher.Stop();

			_watcherStarted = false;
		}

		/// <summary>
		/// If a device object has been instantiated (a handle to the device is opened), we must close it before the app 
		/// goes into suspension because the API automatically closes it for us if we don't. When resuming, the API will
		/// not reopen the device automatically, so we need to explicitly open the device in the app (Scenario1_DeviceConnect).
		/// 
		/// Since we have to reopen the device ourselves when the app resumes, it is good practice to explicitly call the close
		/// in the app as well (For every open there is a close).
		/// 
		/// We must stop the DeviceWatcher because it will continue to raise events even if
		/// the app is in suspension, which is not desired (drains battery). We resume the device watcher once the app resumes again.
		/// </summary>
		private void OnAppSuspension(Object sender, SuspendingEventArgs args)
		{
			if (_watcherStarted)
			{
				_watcherSuspended = true;
				StopDeviceWatcher();
			}
			else
				_watcherSuspended = false;

			CloseCurrentlyConnectedDevice();
		}

		/// <summary>
		/// When resume into the application, we should reopen a handle to the device again. This will automatically
		/// happen when we start the device watcher again; the device will be re-enumerated and we will attempt to reopen it
		/// if IsEnabledAutoReconnect property is enabled.
		/// </summary>
		private void OnAppResume(Object sender, Object args)
		{
			if (_watcherSuspended)
			{
				_watcherSuspended = false;
				StartDeviceWatcher();
			}
		}

		/// <summary>
		/// Close the device that is opened so that all pending operations are canceled properly.
		/// </summary>
		private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
		{
			if (IsDeviceConnected && (deviceInformationUpdate.Id == DeviceInformation.Id))
			{
				// The main reasons to close the device explicitly is to clean up resources, to properly handle errors, and stop talking to the disconnected device.
				CloseCurrentlyConnectedDevice();
			}
		}

		/// <summary>
		/// Open the device that the user wanted to open if it hasn't been opened yet and auto reconnect is enabled.
		/// </summary>
		private async void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
		{
			if (DeviceInformation != null)
				if ((deviceInfo.Id == DeviceInformation.Id) && !IsDeviceConnected && IsEnabledAutoReconnect)
					await OpenDeviceAsync(DeviceInformation);
		}

		/// <summary>
		/// Close the device if the device access was denied by anyone (system or the user) and reopen it if permissions are allowed again
		/// </summary>
		private async void OnDeviceAccessChanged(DeviceAccessInformation sender, DeviceAccessChangedEventArgs eventArgs)
		{
			if ((eventArgs.Status == DeviceAccessStatus.DeniedBySystem) || (eventArgs.Status == DeviceAccessStatus.DeniedByUser))
				CloseCurrentlyConnectedDevice();
			else if ((eventArgs.Status == DeviceAccessStatus.Allowed) && (DeviceInformation != null) && IsEnabledAutoReconnect)
				await OpenDeviceAsync(DeviceInformation);
		}

		#endregion

		#region Members

		private object _device;
		private DeviceWatcher _deviceWatcher;

		private SuspendingEventHandler _appSuspendEventHandler;
		private EventHandler<Object> _appResumeEventHandler;

		private TypedEventHandler<DeviceWatcher, DeviceInformation> _deviceAddedEventHandler;
		private TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> _deviceRemovedEventHandler;
		private TypedEventHandler<DeviceAccessInformation, DeviceAccessChangedEventArgs> _deviceAccessEventHandler;

		private bool _watcherSuspended = false;
		private bool _watcherStarted = false;

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
					CloseDevice();
				}

				// Call the appropriate methods to clean upunmanaged resources here. 
				// ...

				disposed = true;
			}
		}

		private bool disposed = false;

		#endregion
	}
}
