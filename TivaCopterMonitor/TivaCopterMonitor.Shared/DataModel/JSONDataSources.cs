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
		// Euler angles
		[JsonProperty(Order = 4, Required = Required.Always)]
		public float Yaw { get; set; }
		[JsonProperty(Order = 5, Required = Required.Always)]
		public float Pitch { get; set; }
		[JsonProperty(Order = 6, Required = Required.Always)]
		public float Roll { get; set; }

		[JsonIgnore]
		public float YawDegree  => 180 * Yaw / (float)Math.PI;
		[JsonIgnore]
		public float PitchDegree => 180 * Pitch / (float)Math.PI;
		[JsonIgnore]
		public float RollDegree => 180 * Roll / (float)Math.PI;

		// Quaternion
		[JsonProperty(Order = 0, Required = Required.Always)]
		public float q0 { get; set; }
		[JsonProperty(Order = 1, Required = Required.Always)]
		public float q1 { get; set; }
		[JsonProperty(Order = 2, Required = Required.Always)]
		public float q2 { get; set; }
		[JsonProperty(Order = 3, Required = Required.Always)]
		public float q3 { get; set; }

		// Position
		[JsonProperty(Order = 7)]
		public float px { get; set; }
		[JsonProperty(Order = 8)]
		public float py { get; set; }
		[JsonProperty(Order = 9)]
		public float pz { get; set; }
	}

	public sealed class sensors : JSONDataSource
	{
		// Accelerometer
		[JsonProperty(Order = 0, Required = Required.Always)]
		public float ax { get; set; }
		[JsonProperty(Order = 1, Required = Required.Always)]
		public float ay { get; set; }
		[JsonProperty(Order = 2, Required = Required.Always)]
		public float az { get; set; }

		// Gyroscope
		[JsonProperty(Order = 3, Required = Required.Always)]
		public float gx { get; set; }
		[JsonProperty(Order = 4, Required = Required.Always)]
		public float gy { get; set; }
		[JsonProperty(Order = 5, Required = Required.Always)]
		public float gz { get; set; }

		// Magnetometer
		[JsonProperty(Order = 6)]
		public float mx { get; set; }
		[JsonProperty(Order = 7)]
		public float my { get; set; }
		[JsonProperty(Order = 8)]
		public float mz { get; set; }
	}

	public sealed class PID : JSONDataSource
	{
		// Motors throttle
		[JsonProperty(Order = 0, Required = Required.Always)]
		public float Motor1 { get; set; }
		[JsonProperty(Order = 1, Required = Required.Always)]
		public float Motor2 { get; set; }
		[JsonProperty(Order = 2, Required = Required.Always)]
		public float Motor3 { get; set; }
		[JsonProperty(Order = 3, Required = Required.Always)]
		public float Motor4 { get; set; }

		// Stabilisation PIDs output
		[JsonProperty(Order = 4, Required = Required.Always)]
		public float YawOut { get; set; }
		[JsonProperty(Order = 5, Required = Required.Always)]
		public float PitchOut { get; set; }
		[JsonProperty(Order = 6, Required = Required.Always)]
		public float RollOut { get; set; }

	}

	public sealed class radio : JSONDataSource
	{
		// Radio outputs
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
}
