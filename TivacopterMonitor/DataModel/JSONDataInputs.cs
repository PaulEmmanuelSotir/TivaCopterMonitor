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
		[JsonProperty(PropertyName = "throttle", Order = 0, Required = Required.Always)]
		public float Throttle
		{
			get { return _throttle; }
			set { _throttle = Map(value, 0, 1); }
		}

		/// <summary>
		/// The X component of the direction vector.
		/// The value must be between -1 and 1.
		/// </summary>
		[JsonProperty(PropertyName = "directionX", Order = 1, Required = Required.Always)]
		public float DirectionX
		{
			get { return _directionX; }
			set { _directionX = Map(value, -1, 1); }
		}

		/// <summary>
		/// The Y component of the direction vector.
		/// The value must be between -1 and 1.
		/// </summary>
		[JsonProperty(PropertyName = "directionY", Order = 2, Required = Required.Always)]
		public float DirectionY
		{
			get { return _directionY; }
			set { _directionY = Map(value, -1, 1); }
		}

		/// <summary>
		/// The orientation of quadricopter around z axis.
		/// The value must be between -PI and PI.
		/// </summary>
		[JsonProperty(PropertyName = "yaw", Order = 3)]
		public float Yaw
		{
			get { return _yaw; }
			set { _yaw = Map(value, -(float)Math.PI, (float)Math.PI); }
		}

		/// </summary>
		/// Hold button save current control when pushed and retore saved controls when released (remap numeric controls within new range).
		/// </summary>
		[JsonIgnore]
		public bool Hold { get; set; }

		/// </summary>
		/// Klaxon !
		/// </summary>
		[JsonProperty(PropertyName = "beep", Order = 4)]
		public bool Beep { get; set; }

		private static float Saturate(float val, float min, float max)
		{
			return Math.Min(Math.Max(val, min), max);
		}

		private static float Map(float val, float min, float max)
		{
			return Math.Min(Math.Max(val / 256f, 0f), 1f) * (max - min) + min;
		}

		private float _throttle;
		private float _directionX;
		private float _directionY;
		private float _yaw;
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
