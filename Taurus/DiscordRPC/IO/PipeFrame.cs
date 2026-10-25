
using System;
using System.IO;
using System.Text;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC.IO;
public struct PipeFrame : IEquatable<PipeFrame>
{
  public static readonly int MAX_SIZE = 16384 ;
  public Opcode Opcode { get; set; }
  public uint Length => (uint) this.Data.Length;
  public byte[] Data { get; set; }
  public string Message
  {
    get => this.GetMessage();
    set => this.SetMessage(value);
  }
  public Encoding MessageEncoding => Encoding.UTF8;
  public PipeFrame(Opcode opcode, object data)
  {
    this.Opcode = opcode;
    this.Data = (byte[]) null;
    this.SetObject(data);
  }
  private void SetMessage(string str) => this.Data = this.MessageEncoding.GetBytes(str);
  private string GetMessage() => this.MessageEncoding.GetString(this.Data);
  public void SetObject(object obj) => this.SetMessage(JsonConvert.SerializeObject(obj));
  public void SetObject(Opcode opcode, object obj)
  {
    this.Opcode = opcode;
    this.SetObject(obj);
  }
  public T GetObject<T>() => JsonConvert.DeserializeObject<T>(this.GetMessage());
  public bool ReadStream(Stream stream)
  {
    uint num;
    uint b1;
    if (!this.TryReadUInt32(stream, out num) || !this.TryReadUInt32(stream, out b1))
      return false;
    uint b2 = b1;
    using (MemoryStream memoryStream = new MemoryStream())
    {
      uint length = (uint) this.Min(2048 , b1);
      byte[] buffer = new byte[(int) length];
      int count;
      while ((count = stream.Read(buffer, 0, this.Min(buffer.Length, b2))) > 0)
      {
        b2 -= length;
        memoryStream.Write(buffer, 0, count);
      }
      byte[] array = memoryStream.ToArray();
      if ((long) array.Length != (long) b1)
        return false;
      this.Opcode = (Opcode) num;
      this.Data = array;
      return true;
    }
  }
  private int Min(int a, uint b) => (long) b < (long) a ? (int) b : a;
  private bool TryReadUInt32(Stream stream, out uint value)
  {
    byte[] buffer = new byte[4];
    if (stream.Read(buffer, 0, buffer.Length) != 4)
    {
      value = 0U;
      return false;
    }
    value = BitConverter.ToUInt32(buffer, 0);
    return true;
  }
  public void WriteStream(Stream stream)
  {
    byte[] bytes1 = BitConverter.GetBytes((uint) this.Opcode);
    byte[] bytes2 = BitConverter.GetBytes(this.Length);
    byte[] buffer = new byte[bytes1.Length + bytes2.Length + this.Data.Length];
    bytes1.CopyTo((Array) buffer, 0);
    bytes2.CopyTo((Array) buffer, bytes1.Length);
    this.Data.CopyTo((Array) buffer, bytes1.Length + bytes2.Length);
    stream.Write(buffer, 0, buffer.Length);
  }
  public bool Equals(PipeFrame other)
  {
    return this.Opcode == other.Opcode && (int) this.Length == (int) other.Length && this.Data == other.Data;
  }
}
