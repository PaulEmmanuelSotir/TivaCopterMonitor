using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Contrôle utilisateur, consultez la page http://go.microsoft.com/fwlink/?LinkId=234236

namespace TivacopterMonitor.View
{
	public sealed partial class NumericPropertyControl : UserControl
	{
		public NumericPropertyControl()
		{
			this.InitializeComponent();
			(this.Content as FrameworkElement).DataContext = this;
		}

		public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(nameof(PropertyName), typeof(string), typeof(NumericPropertyControl), new PropertyMetadata(""));
		public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(nameof(Unit), typeof(string), typeof(NumericPropertyControl), new PropertyMetadata(""));
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericPropertyControl), new PropertyMetadata(0.0, ValueChanged));
		public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(NumericPropertyControl), new PropertyMetadata(100.0));
		public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(NumericPropertyControl), new PropertyMetadata(0.0));
		public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(NumericPropertyControl), null);
		public static readonly DependencyProperty StringValueProperty = DependencyProperty.Register(nameof(StringValue), typeof(string), typeof(NumericPropertyControl), new PropertyMetadata("0"));

		public string PropertyName
		{
			get { return (string)GetValue(PropertyNameProperty); }
			set
			{
				SetValue(PropertyNameProperty, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PropertyName)));
			}
		}

		public string Unit
		{
			get { return (string)GetValue(UnitProperty); }
			set
			{
				SetValue(UnitProperty, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Unit)));
			}
		}

		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set
			{
				SetValue(ValueProperty, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
			}
		}

		public string StringValue { get { return (string)GetValue(StringValueProperty); } }

		public double MaxValue
		{
			get { return (double)GetValue(MaxValueProperty); }
			set
			{
				SetValue(MaxValueProperty, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxValue)));
			}
		}

		public double MinValue
		{
			get { return (double)GetValue(MinValueProperty); }
			set
			{
				SetValue(MinValueProperty, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinValue)));
			}
		}

		public string StringFormat
		{
			get { return (string)GetValue(StringFormatProperty); }
			set
			{
				SetValue(StringFormatProperty, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StringFormat)));
			}
		}

		private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (NumericPropertyControl)d;
			if (ctrl.StringFormat == null)
				ctrl.SetValue(StringValueProperty, e.NewValue);
			else
				ctrl.SetValue(StringValueProperty, string.Format(ctrl.StringFormat, e.NewValue));
			ctrl.PropertyChanged?.Invoke(ctrl, new PropertyChangedEventArgs(nameof(StringValue)));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
