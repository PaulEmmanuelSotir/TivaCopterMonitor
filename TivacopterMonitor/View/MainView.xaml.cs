using System;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TivaCopterMonitor.ViewModel;

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

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.  The Parameter
		/// property is typically used to configure the page.</param>
		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			await ViewModel?.RefreshBluetoothDevices();
		}

		public TivaCopterViewModel ViewModel { get; private set; }

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

		#endregion

		//private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources"); // TODO: enlever cette ligne si inutile
	}
}
