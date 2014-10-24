using System;
using System.Collections.Generic;
using System.Text;

namespace TivaCopterMonitor.Model
{
	public class TivaCopterControl
	{
		public double Throttle { get; set; }

		public double DirectionX { get; set; }
		public double DirectionY { get; set; }

		public double Yaw
		{
			get
			{
				return _yaw;
			}
			set
			{
				_yaw = Saturate(value, -Math.PI, Math.PI);
			}
		}

		private static double Saturate(double val, double min, double max)
		{
			return Math.Min(Math.Max(val, min), max);
		}

		private double _yaw;
	}
}
