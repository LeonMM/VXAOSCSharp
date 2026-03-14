using System;
using System.IO;
using System.Text;

namespace VXAOS_Server {
   public class BufferWriter {
      private MemoryStream stream;
      private BinaryWriter writer;

      public BufferWriter() {
         stream = new MemoryStream();
         writer = new BinaryWriter(stream);
      }

      public void WriteByte(byte value) {
         writer.Write(value);
      }

      public void WriteBoolean(bool value) {
         WriteByte((byte)(value ? 1 : 0));
      }

      public void WriteShort(short value) {
         writer.Write(value);
      }

      public void WriteFloat(float value) {
         writer.Write(value);
      }

      public void WriteDouble(double value) {
         writer.Write(value);
      }

      public void WriteInt(int value) {
         writer.Write(value);
      }

      public void WriteLong(long value) {
         writer.Write(value);
      }

      public void WriteString(string str) {
         byte[] bytes = Encoding.UTF8.GetBytes(str);
         WriteShort((short)bytes.Length);
         writer.Write(bytes);
      }

      public void WriteTime(DateTime time) {
         WriteShort((short)time.Year);
         WriteByte((byte)time.Month);
         WriteByte((byte)time.Day);
      }

      public string ToStringBuffer() {
         byte[] data = stream.ToArray();
         return Encoding.Latin1.GetString(data);
      }
      public byte[] ToArray() {
         return stream.ToArray();
      }
   }

   public class BufferReader {
      private MemoryStream stream;
      private BinaryReader reader;

      public BufferReader(string data) {
         byte[] bytes = Encoding.Latin1.GetBytes(data);
         stream = new MemoryStream(bytes);
         reader = new BinaryReader(stream);
      }

      public byte ReadByte() {
         return reader.ReadByte();
      }

      public bool ReadBoolean() {
         return ReadByte() == 1;
      }

      public short ReadShort() {
         return reader.ReadInt16();
      }

      public float ReadFloat() {
         return reader.ReadSingle();
      }

      public double ReadDouble() {
         return reader.ReadDouble();
      }

      public int ReadInt() {
         return reader.ReadInt32();
      }

      public long ReadLong() {
         return reader.ReadInt64();
      }

      public string ReadString() {
         short size = ReadShort();
         byte[] bytes = reader.ReadBytes(size);
         return Encoding.UTF8.GetString(bytes);
      }

      public DateTime ReadTime() {
         int year = ReadShort();
         int month = ReadByte();
         int day = ReadByte();
         return new DateTime(year, month, day);
      }

      public bool EOF() {
         return stream.Position >= stream.Length;
      }
   }
}
