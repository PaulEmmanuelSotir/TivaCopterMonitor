using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TivaCopterMonitor.ViewModel;
using Windows.UI.Popups;
using System.Threading.Tasks;

namespace TivacopterMonitor.View
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainView : Page, IDisposable
	{
		public MainView()
		{
			InitializeComponent();

			ViewModel = new TivaCopterViewModel();
			DataContext = ViewModel;
		}

		public TivaCopterViewModel ViewModel { get; private set; }

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.  The Parameter
		/// property is typically used to configure the page.</param>
		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			await ViewModel.EnumerateBluetoothDevicesAsync();
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			if (ViewModel != null)
			{
				ControlsSettingSource.Source = ViewModel.ControlMap?.DataMap;
				ViewModel.PropertyChanged += new PropertyChangedEventHandler((s, arg) =>
				{
					if (arg.PropertyName == nameof(ViewModel.ControlMap))
						ControlsSettingSource.Source = ViewModel.ControlMap?.DataMap;
				});
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Task enumertate = ViewModel.EnumerateBluetoothDevicesAsync();
			ConnectionMenuFlyout.Items.Clear();

			foreach (var deviceInfo in ViewModel.BluetoothPairedDevices)
			{
				var deviceMenuItem = new MenuFlyoutItem();
				deviceMenuItem.Text = deviceInfo.Name;
				deviceMenuItem.Command = ViewModel.ConnectToBluetoothDeviceCommand;
				deviceMenuItem.CommandParameter = deviceInfo;
				ConnectionMenuFlyout.Items.Add(deviceMenuItem);
			}
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				// Dispose managed resources.
				if (disposing)
				{
					ViewModel.Dispose();
					ViewModel = null;
				}

				// Call the appropriate methods to clean upunmanaged resources here. 
				// ...

				disposed = true;
			}
		}

		~MainView()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion

		//private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources"); // TODO: enlever cette ligne si inutile
	}
}
