using System;
using System.Windows.Input;

namespace TivaCopterMonitor.Common
{
	public class RelayCommand : ICommand
	{
		// Event that fires when the enabled/disabled state of the cmd changes
		public event EventHandler CanExecuteChanged;

		// Delegate for method to call when the cmd needs to be executed        
		private readonly Action<object> _targetExecuteMethod;

		// Delegate for method that determines if cmd is enabled/disabled        
		private readonly Predicate<object> _targetCanExecuteMethod;

		public bool CanExecute(object parameter)
		{
			if (_targetCanExecuteMethod != null)
				return _targetCanExecuteMethod(parameter);
			return true;
		}

		public void Execute(object parameter)
		{
			// Call the delegate if it's not null
			if (_targetExecuteMethod != null) _targetExecuteMethod(parameter);
		}

		public RelayCommand(Action<object> executeMethod, Predicate<object> canExecuteMethod = null)
		{
			_targetExecuteMethod = executeMethod;
			_targetCanExecuteMethod = canExecuteMethod;
		}

		public void RaiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null) CanExecuteChanged(this, EventArgs.Empty);
		}
	}
}
