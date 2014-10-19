using System;
using System.Collections.Generic;
using System.Text;

namespace TivaCopterMonitor.Model
{
	public interface IJSONDataSource
	{
		//TODO: use stopwatchs rather than datetime
		DateTime CreationTime { get; }
	}

	public class IMU : IJSONDataSource
	{
		public DateTime CreationTime { get { return _creationTime; } }

		// Euler angles
		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public float Roll { get; set; }

		// Quaternion
		public float q0 { get; set; }
		public float q1 { get; set; }
		public float q2 { get; set; }
		public float q3 { get; set; }

		protected DateTime _creationTime = DateTime.Now;
	}

	public class sensors : IJSONDataSource
	{
		public DateTime CreationTime { get { return _creationTime; } }

		// Accelerometer
		public float ax { get; set; }
		public float ay { get; set; }
		public float az { get; set; }

		// Gyroscope
		public float gx { get; set; }
		public float gy { get; set; }
		public float gz { get; set; }

		// Magnetometer
		public float mx { get; set; }
		public float my { get; set; }
		public float mz { get; set; }

		protected DateTime _creationTime = DateTime.Now;
	}

	public class PID : IJSONDataSource
	{
		public DateTime CreationTime { get { return _creationTime; } }

		// Stabilisation PIDs output
		public float YawOut { get; set; }
		public float PitchOut { get; set; }
		public float RollOut { get; set; }

		// Motors throttle
		public float Motor1 { get; set; }
		public float Motor2 { get; set; }
		public float Motor3 { get; set; }
		public float Motor4 { get; set; }

		protected DateTime _creationTime = DateTime.Now;
	}

	public class Radio : IJSONDataSource
	{
		public DateTime CreationTime { get { return _creationTime; } }

		// Radio outputs
		public bool ch0 { get; set; }
		public bool ch1 { get; set; }
		public bool ch2 { get; set; }
		public bool ch3 { get; set; }
		public bool ch4 { get; set; }

		protected DateTime _creationTime = DateTime.Now;
	}
}
