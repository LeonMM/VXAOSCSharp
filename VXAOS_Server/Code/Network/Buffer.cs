using System;
using System.IO;
using System.Text;

namespace VXAOS_Server {
   public class BufferWriter : IDisposable {
      private readonly MemoryStream _stream;
      public BufferWriter(int initialCapacity = 64) {
         _stream = new MemoryStream(initialCapacity);
      }
      public void WriteByte(int value) {
         _stream.WriteByte((byte)value);
      }
      public void WriteBoolean(bool value) {
         WriteByte(value ? 1 : 0);
      }
      public void WriteShort(int value) {
         Span<byte> buffer = stackalloc byte[2];
         BitConverter.TryWriteBytes(buffer, (short)value);
         _stream.Write(buffer);
      }
      public void WriteFloat(float value) {
         Span<byte> buffer = stackalloc byte[4];
         BitConverter.TryWriteBytes(buffer, value);
         _stream.Write(buffer);
      }
      public void WriteDouble(double value) {
         Span<byte> buffer = stackalloc byte[8];
         BitConverter.TryWriteBytes(buffer, value);
         _stream.Write(buffer);
      }
      public void WriteInt(int value) {
         Span<byte> buffer = stackalloc byte[4];
         BitConverter.TryWriteBytes(buffer, value);
         _stream.Write(buffer);
      }
      public void WriteLong(long value) {
         Span<byte> buffer = stackalloc byte[8];
         BitConverter.TryWriteBytes(buffer, value);
         _stream.Write(buffer);
      }
      public void WriteString(string str) {
         if (string.IsNullOrEmpty(str)) {
            WriteShort(0);
            return;
         }
         int byteCount = Encoding.UTF8.GetByteCount(str);
         WriteShort((short)byteCount);
         byte[]? rented = null;
         Span<byte> buffer = byteCount <= 256
            ? stackalloc byte[byteCount]
            : (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(byteCount));

         try {
            Encoding.UTF8.GetBytes(str, buffer);
            _stream.Write(buffer.Slice(0, byteCount));
         } finally {
            if (rented != null) {
               System.Buffers.ArrayPool<byte>.Shared.Return(rented);
            }
         }
      }
      public void WriteTime(DateTimeOffset time) {
         WriteShort(time.Year);
         WriteByte(time.Month);
         WriteByte(time.Day);
      }
      public string ToStringBuffer() {
         byte[] rawBuffer = _stream.GetBuffer();
         return Encoding.Latin1.GetString(rawBuffer, 0, (int)_stream.Length);
      }
      public byte[] ToArray() {
         return _stream.ToArray();
      }
      public void Dispose() {
         _stream.Dispose();
      }
   }

   public class BufferReader {
      private readonly byte[] _data;
      private int _position;
      public BufferReader(string data) {
         _data = Encoding.Latin1.GetBytes(data);
         _position = 0;
      }
      public BufferReader(byte[] data) {
         _data = data;
         _position = 0;
      }
      public byte ReadByte() {
         if (_position >= _data.Length) throw new EndOfStreamException();
         return _data[_position++];
      }
      public bool ReadBoolean() {
         return ReadByte() == 1;
      }
      public short ReadShort() {
         short value = BitConverter.ToInt16(_data, _position);
         _position += 2;
         return value;
      }
      public float ReadFloat() {
         float value = BitConverter.ToSingle(_data, _position);
         _position += 4;
         return value;
      }
      public double ReadDouble() {
         double value = BitConverter.ToDouble(_data, _position);
         _position += 8;
         return value;
      }
      public int ReadInt() {
         int value = BitConverter.ToInt32(_data, _position);
         _position += 4;
         return value;
      }
      public long ReadLong() {
         long value = BitConverter.ToInt64(_data, _position);
         _position += 8;
         return value;
      }
      public string ReadString() {
         short size = ReadShort();
         if (size <= 0) return string.Empty;

         string str = Encoding.UTF8.GetString(_data, _position, size);
         _position += size;
         return str;
      }
      public DateTimeOffset ReadTime() {
         int year = ReadShort();
         int month = ReadByte();
         int day = ReadByte();
         return new DateTimeOffset(new DateTime(year, month, day));
      }
      public bool EOF() {
         return _position >= _data.Length;
      }
   }
}