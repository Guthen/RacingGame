using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RacingGame.Utils.JsonUtils
{
	public class Vector2Converter : JsonConverter
	{
		public override bool CanConvert( Type type )
		{
			return type == typeof( Vector2 );
		}

		public override object ReadJson( JsonReader reader, Type type, object existingValue, JsonSerializer serializer )
		{
			var properties = JObject.Load( reader ).Properties().ToList();
			return new Vector2( (float) properties[0].Value, (float) properties[1].Value );
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			throw new NotImplementedException();
		}
	}

	public class Vector2ArrayConverter : JsonConverter
	{
		public override bool CanConvert( Type type )
		{
			return type == typeof( Vector2[] );
		}

		public override object ReadJson( JsonReader reader, Type type, object existingValue, JsonSerializer serializer )
		{
			if ( !( reader.TokenType == JsonToken.StartArray ) ) return null;

			List<Vector2> vectors = new List<Vector2>();

			string axis = null;
			Vector2 vector = Vector2.Zero;
			while ( !( reader.TokenType == JsonToken.EndArray ) && reader.Read() )
			{
				switch ( reader.TokenType )
				{
					case JsonToken.StartObject:
						vector = new Vector2();
						break;
					case JsonToken.PropertyName:
						axis = (string) reader.Value;
						break;
					case JsonToken.EndObject:
						vectors.Add( vector );
						break;
					default:
						if ( reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer )
							if ( axis == "X" )
								vector.X = (long) reader.Value;
							else
								vector.Y = (long) reader.Value;
						break;
				}
				//Console.WriteLine( reader.TokenType + " " + reader.Value );
			}

			return vectors.ToArray();
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			throw new NotImplementedException();
		}
	}
}
