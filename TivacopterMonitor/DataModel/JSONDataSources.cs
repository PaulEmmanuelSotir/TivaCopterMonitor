using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TivaCopterMonitor.Model
{
	public abstract class JSONDataSource
	{
		//TODO: use stopwatchs rather than datetime
		[JsonIgnore]
		public DateTime CreationTime { get { return _creationTime; } }

		protected DateTime _creationTime = DateTime.Now;
	}

	public sealed class IMU : JSONDataSource
	{
		/// <summary>
		/// Euler angles
		/// The values must be between -PI and PI.
		/// </summary>
		[JsonProperty(Order = 4, Required = Required.Always)]
		public float Yaw { get; set; }
		[JsonProperty(Order = 5, Required = Required.Always)]
		public float Pitch { get; set; }
		[JsonProperty(Order = 6, Required = Required.Always)]
		public float Roll { get; set; }

		/// <summary>
		/// Euler angles in degrees
		/// </summary>
		[JsonIgnore]
		public float YawDegree => 180 * Yaw / (float)Math.PI;
		[JsonIgnore]
		public float PitchDegree => 180 * Pitch / (float)Math.PI;
		[JsonIgnore]
		public float RollDegree => 180 * Roll / (float)Math.PI;

		/// <summary>
		/// Quaternion
		/// The values must be between -1 and 1.
		/// </summary>
		[JsonProperty(Order = 0, Required = Required.Always)]
		public float q0 { get; set; }
		[JsonProperty(Order = 1, Required = Required.Always)]
		public float q1 { get; set; }
		[JsonProperty(Order = 2, Required = Required.Always)]
		public float q2 { get; set; }
		[JsonProperty(Order = 3, Required = Required.Always)]
		public float q3 { get; set; }

		/// <summary>
		/// Position vector
		/// </summary>
		[JsonProperty(Order = 7)]
		public float px { get; set; }
		[JsonProperty(Order = 8)]
		public float py { get; set; }
		[JsonProperty(Order = 9)]
		public float pz { get; set; }
	}

	public sealed class sensors : JSONDataSource
	{
		/// <summary>
		/// Acceleration vector
		/// </summary>
		[JsonProperty(Order = 0, Required = Required.Always)]
		public float ax { get; set; }
		[JsonProperty(Order = 1, Required = Required.Always)]
		public float ay { get; set; }
		[JsonProperty(Order = 2, Required = Required.Always)]
		public float az { get; set; }

		/// <summary>
		/// Gyroscope measurement
		/// </summary>
		[JsonProperty(Order = 3, Required = Required.Always)]
		public float gx { get; set; }
		[JsonProperty(Order = 4, Required = Required.Always)]
		public float gy { get; set; }
		[JsonProperty(Order = 5, Required = Required.Always)]
		public float gz { get; set; }

		/// <summary>
		/// Magnetometer measurement
		/// </summary>
		[JsonProperty(Order = 6)]
		public float mx { get; set; }
		[JsonProperty(Order = 7)]
		public float my { get; set; }
		[JsonProperty(Order = 8)]
		public float mz { get; set; }
	}

	public sealed class PID : JSONDataSource
	{
		/// <summary>
		/// Motors throttle
		/// The values must be between 0 and 1.
		/// </summary>
		[JsonProperty(Order = 0, Required = Required.Always)]
		public float Motor1 { get; set; }
		[JsonProperty(Order = 1, Required = Required.Always)]
		public float Motor2 { get; set; }
		[JsonProperty(Order = 2, Required = Required.Always)]
		public float Motor3 { get; set; }
		[JsonProperty(Order = 3, Required = Required.Always)]
		public float Motor4 { get; set; }

		/// <summary>
		/// Stabilisation PIDs inputs
		/// </summary>
		[JsonProperty(Order = 4, Required = Required.Always)]
		public float YawIn { get; set; }
		[JsonProperty(Order = 5, Required = Required.Always)]
		public float PitchIn { get; set; }
		[JsonProperty(Order = 6, Required = Required.Always)]
		public float RollIn { get; set; }
		[JsonProperty(Order = 7, Required = Required.Always)]
		public float AltitudeIn { get; set; }

		/// <summary>
		/// Stabilisation PIDs output
		/// </summary>
		[JsonProperty(Order = 8, Required = Required.Always)]
		public float YawOut { get; set; }
		[JsonProperty(Order = 9, Required = Required.Always)]
		public float PitchOut { get; set; }
		[JsonProperty(Order = 10, Required = Required.Always)]
		public float RollOut { get; set; }
		[JsonProperty(Order = 11, Required = Required.Always)]
		public float AltitudeOut { get; set; }
	}

	public sealed class radio : JSONDataSource
	{
		/// <summary>
		/// Radio outputs
		/// </summary>
		[JsonProperty(Order = 0, Required = Required.Always)]
		public bool in0 { get; set; }
		[JsonProperty(Order = 1, Required = Required.Always)]
		public bool in1 { get; set; }
		[JsonProperty(Order = 2, Required = Required.Always)]
		public bool in2 { get; set; }
		[JsonProperty(Order = 3, Required = Required.Always)]
		public bool in3 { get; set; }
		[JsonProperty(Order = 4, Required = Required.Always)]
		public bool in4 { get; set; }
	}

	public sealed class rawEcho : JSONDataSource
	{
		[JsonProperty(Required = Required.Always)]
		public string rawInput { get; set; }
	}

}