﻿using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TivaCopterMonitor.DataAccessLayer
{
	public abstract class JsonCreationConverter<T> : JsonConverter
	{
		protected abstract T Create(Type objectType, JObject jsonObject);

		public override bool CanConvert(Type objectType)
		{
			return typeof(T).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jsonObject = JObject.Load(reader);
			var target = Create(objectType, jsonObject);
			serializer.Populate(jsonObject.CreateReader(), target);
			return target;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}

	public class JsonDataSourceConverter : JsonCreationConverter<Model.JSONDataSource>
	{
		protected override Model.JSONDataSource Create(Type objectType, JObject jsonObject)
		{
			if (jsonObject["q0"] != null)
				return new Model.IMU();
			else if (jsonObject["motor1"] != null)
				return new Model.PID();
			else if (jsonObject["in0"] != null)
				return new Model.radio();
			else if (jsonObject["ax"] != null)
				return new Model.sensors();
			else if (jsonObject["rawInput"] != null)
				return new Model.rawEcho();

			return null;
		}
	}

	public class BoolConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((bool)value) ? 1 : 0);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return reader.Value.ToString() == "1";
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(bool);
		}
	}
}
