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
			JArray array = JArray.Load(reader);
			List<T> result = new List<T>();
			foreach(JToken token in array) {
				T item = token.ToObject<T>(serializer); 
				result.Add(item);
			}
			return result;
		}
		public override void WriteJson(JsonWriter writer,List<T> value,JsonSerializer serializer) {
			JArray array = new JArray();
			foreach(T item in value) {
				JToken token = JToken.FromObject(item,serializer);
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
