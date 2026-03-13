using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXAOS_Server {
	public class ListConverter<T>:JsonConverter<List<T>> {
		public override List<T> ReadJson(JsonReader reader,Type objectType,List<T> existingValue,bool hasExistingValue,JsonSerializer serializer) {
			// Deserialize JSON array into List<T>
			JArray array = JArray.Load(reader);
			List<T> result = new List<T>();

			foreach(JToken token in array) {
				T item = token.ToObject<T>(serializer); // Deserialize each item in the array
				result.Add(item);
			}

			return result;
		}

		public override void WriteJson(JsonWriter writer,List<T> value,JsonSerializer serializer) {
			// Serialize List<T> to JSON array
			JArray array = new JArray();

			foreach(T item in value) {
				JToken token = JToken.FromObject(item,serializer); // Serialize each item

				if(token.Type == JTokenType.Object || token.Type == JTokenType.Array) {
					JObject obj = (JObject)token;
					array.Add(obj);
				} else {
					array.Add(token);
				}
			}

			array.WriteTo(writer);
		}
	}
}
