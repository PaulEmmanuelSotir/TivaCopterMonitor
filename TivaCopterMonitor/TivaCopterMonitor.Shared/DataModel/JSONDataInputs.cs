using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TivaCopterMonitor.Model
{
	public abstract class JSONDataInput
	{
		//TODO: use stopwatchs rather than datetime
		[JsonIgnore]
		public DateTime CreationTime { get { return _creationTime; } }

		protected DateTime _creationTime = DateTime.Now;
	}

	public sealed class RemoteControl : JSONDataInput
	{
		/// <summary>
		/// Global motors throttle.
		/// The value must be between -1 and 1.
		/// </summary>
		[JsonProperty(Order = 0, Required = Required.Always)]
		public double Throttle
		{
			get { return _throttle; }
			set { _throttle = Saturate(value, 0, 1); }
		}

		/// <summary>
		/// The X component of the direction vector.
		/// The value must be between -1 and 1.
		/// </summary>
		[JsonProperty(Order = 1, Required = Required.Always)]
		public double DirectionX
		{
			get { return _directionX; }
			set { _directionX = Saturate(value, -1, 1); }
		}

		/// <summary>
		/// The Y component of the direction vector.
		/// The value must be between -1 and 1.
		/// </summary>
		[JsonProperty(Order = 2, Required = Required.Always)]
		public double DirectionY
		{
			get { return _directionY; }
			set { _directionY = Saturate(value, -1, 1); }
		}

		/// <summary>
		/// The orientation of quadricopter around z axis.
		/// The value must be between -PI and PI.
		/// </summary>
		[JsonProperty(Order = 3)]
		public double Yaw
		{
			get { return _yaw; }
			set { _yaw = Saturate(value, -Math.PI, Math.PI); }
		}

		/// </summary>
		/// Klaxon !
		/// </summary>
		[JsonProperty(Order = 4)]
		public bool Beep { get; set; }

		private static double Saturate(double val, double min, double max)
		{
			return Math.Min(Math.Max(val, min), max);
		}

		private double _throttle;
		private double _directionX;
		private double _directionY;
		private double _yaw;
	}

	// TODO: s'occuper de PIDConfigurationDataInput
	/*public sealed class PIDConfigurationDataInput : JSONDataInput
	{
		public PID YawPID { get; set; }
		public PID PitchPID { get; set; }
		public PID RollPID { get; set; }

        public struct PID
		{
			double Kp;
			double Ki;
			double Kd;

			double ILimit;
		}
	}*/
}
