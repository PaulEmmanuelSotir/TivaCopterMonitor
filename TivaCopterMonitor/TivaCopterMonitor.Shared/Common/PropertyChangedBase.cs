using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TivaCopterMonitor.Common
{
	public class PropertyChangedBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var eventHandler = this.PropertyChanged;
			if (eventHandler != null)
				eventHandler(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
