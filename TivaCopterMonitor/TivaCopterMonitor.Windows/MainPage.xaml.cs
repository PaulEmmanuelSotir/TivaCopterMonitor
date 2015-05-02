using System;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TivaCopterMonitor
{
	public sealed partial class MainPage : Page, IDisposable
	{
		public MainPage()
		{
			this.InitializeComponent();

			_tivaCopterVM = new ViewModel.TivaCopterViewModel();
			this.DataContext = _tivaCopterVM;
		}

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.  The Parameter
		/// property is typically used to configure the page.</param>
		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			await _tivaCopterVM?.RefreshBluetoothDevices();
		}

		private void Page_Loaded(object sender, RoutedEventArgs args)
		{
			if(_tivaCopterVM != null)
			{
				BluetoothDevicesSource.Source = _tivaCopterVM.BluetoothPairedDevices;
				ControlsSettingSource.Source = _tivaCopterVM.ControlMap?.DataMap;
				_tivaCopterVM.PropertyChanged += new PropertyChangedEventHandler((s, e) =>
				{
					if (e.PropertyName == nameof(_tivaCopterVM.ControlMap))
						ControlsSettingSource.Source = _tivaCopterVM.ControlMap?.DataMap;
				});
			}
			

			MoonFall.Begin();
		}

		private ViewModel.TivaCopterViewModel _tivaCopterVM;
		private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

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
					_tivaCopterVM.Dispose();
					_tivaCopterVM = null;
				}

				// Call the appropriate methods to clean upunmanaged resources here. 
				// ...

				disposed = true;
			}
		}

		~MainPage()
		{
			Dispose(false);
		}

		private bool disposed = false;

		#endregion

	}
}
