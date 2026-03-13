using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
namespace VXAOS_Server.RPGData {
	public class Table{
		public int xsize;
		public int ysize;
		public int zsize;
		[JsonConverter(typeof(ListConverter<int>))]
		public List<int> data;

		public Table(int xsize,int ysize = 1,int zsize = 1) {
			this.xsize = xsize;
			this.ysize = ysize;
			this.zsize = zsize;
			this.data = new List<int>(new int[(xsize * ysize * zsize)]);
		}

		public void Resize(int newXsize,int newYsize = 1,int newZsize = 1) {
			List<int> newData = new List<int>(new int[newXsize * newYsize * newZsize]);

			int minX = Math.Min(xsize,newXsize);
			int minY = Math.Min(ysize,newYsize);
			int minZ = Math.Min(zsize,newZsize);

			for(int z = 0; z < minZ; z++) {
				for(int y = 0; y < minY; y++) {
					for(int x = 0; x < minX; x++) {
						newData[x + y * newXsize + z * newXsize * newYsize] =
								data[x + y * xsize + z * xsize * ysize];
					}
				}
			}

			data = newData;
			xsize = newXsize;
			ysize = newYsize;
			zsize = newZsize;
		}

		public int? this[int x,int y = 0,int z = 0] {
			get {
				if(x >= xsize || y >= ysize)
					return null;
				return data[x + y * xsize + z * xsize * ysize];
			}
			set {
				data[x + y * xsize + z * xsize * ysize] = value.Value;
			}
		}
	}
	public class TableConverter:JsonConverter<Table> {
		public override void WriteJson(JsonWriter writer,Table value,JsonSerializer serializer) {
			var jsonObject = new JObject();
			jsonObject.Add("xsize",value.xsize);
			jsonObject.Add("ysize",value.ysize);
			jsonObject.Add("zsize",value.zsize);
			jsonObject.Add("data",JArray.FromObject(value.data));
			jsonObject.WriteTo(writer);
		}

		public override Table ReadJson(JsonReader reader,Type objectType,Table existingValue,bool hasExistingValue,JsonSerializer serializer) {
			JObject jsonObject = JObject.Load(reader);
			int xsize = jsonObject["xsize"].Value<int>();
			int ysize = jsonObject["ysize"].Value<int>();
			int zsize = jsonObject["zsize"].Value<int>();
			List<int> data = jsonObject["data"].ToObject<List<int>>();

			Table table = new Table(xsize,ysize,zsize) {
				data = data
			};
			return table;
		}
	}
}
