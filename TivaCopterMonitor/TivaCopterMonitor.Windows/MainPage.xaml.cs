using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			BluetoothDevicesSource.Source = _tivaCopterVM?.BluetoothPairedDevices;

			MoonFall.Begin();
		}

		private ViewModel.TivaCopterViewModel _tivaCopterVM;
		private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

		#region IDisposable

		public void Dispose()
		{
			_tivaCopterVM.Dispose();
			_tivaCopterVM = null;

			GC.SuppressFinalize(this);
		}

		#endregion

	}
}
