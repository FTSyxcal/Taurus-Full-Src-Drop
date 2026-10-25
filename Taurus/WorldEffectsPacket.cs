
using System;
using System.IO;
namespace Taurus;
public sealed class WorldEffectsPacket
{
  public const int MaxPlatforms = 74;
  public const int MaxShots = 6;
  public const int MaxLength = 4096 ;
  public int Timestamp;
  public byte Design;
  public uint Background;
  public uint Left;
  public uint Right;
  public float[] Gun;
  public bool Locked;
  public bool Pressed;
  public float[][] Platforms;
  public WorldEffectsPacket.Shot[] Shots = Array.Empty<WorldEffectsPacket.Shot>();
  public byte[] Encode()
  {
    using (MemoryStream output = new MemoryStream())
    {
      using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
      {
        binaryWriter.Write(827868756U);
        binaryWriter.Write(this.Timestamp);
        binaryWriter.Write(this.Design);
        binaryWriter.Write((byte) ((this.Gun != null ? 1 : 0) | (this.Locked ? 2 : 0) | (this.Pressed ? 4 : 0)));
        binaryWriter.Write(this.Background);
        binaryWriter.Write(this.Left);
        binaryWriter.Write(this.Right);
        binaryWriter.Write(this.Platforms == null ? byte.MaxValue : (byte) this.Platforms.Length);
        binaryWriter.Write((byte) this.Shots.Length);
        if (this.Gun != null)
        {
          foreach (float num in this.Gun)
            binaryWriter.Write(num);
        }
        if (this.Platforms != null)
        {
          foreach (float[] platform in this.Platforms)
          {
            foreach (float num in platform)
              binaryWriter.Write(num);
          }
        }
        foreach (WorldEffectsPacket.Shot shot in this.Shots)
        {
          binaryWriter.Write(shot.Serial);
          binaryWriter.Write(shot.Kind);
          binaryWriter.Write(shot.Timestamp);
          foreach (float num in shot.Position)
            binaryWriter.Write(num);
          foreach (float num in shot.Velocity)
            binaryWriter.Write(num);
          binaryWriter.Write(shot.Scale);
        }
        byte[] array = output.ToArray();
        return WorldEffectsPacket.TryDecode(array, this.Timestamp, out WorldEffectsPacket _) ? array : throw new ArgumentException("Invalid world effects snapshot.");
      }
    }
  }
  public static bool TryDecode(byte[] bytes, int now, out WorldEffectsPacket packet)
  {
    packet = (WorldEffectsPacket) null;
    if (bytes != null && bytes.Length >= 24)
    {
      if (bytes.Length <= 4096 )
      {
        try
        {
          using (MemoryStream input = new MemoryStream(bytes, false))
          {
            using (BinaryReader reader = new BinaryReader((Stream) input))
            {
              if (reader.ReadUInt32() != 827868756U)
                return false;
              WorldEffectsPacket worldEffectsPacket = new WorldEffectsPacket()
              {
                Timestamp = reader.ReadInt32(),
                Design = reader.ReadByte()
              };
              int num1 = now - worldEffectsPacket.Timestamp;
              byte num2 = reader.ReadByte();
              if (num1 < -2000 || num1 > 6000 || worldEffectsPacket.Design >= (byte) 14 || num2 > (byte) 7)
                return false;
              worldEffectsPacket.Locked = ((uint) num2 & 2U) > 0U;
              worldEffectsPacket.Pressed = ((uint) num2 & 4U) > 0U;
              worldEffectsPacket.Background = reader.ReadUInt32();
              worldEffectsPacket.Left = reader.ReadUInt32();
              worldEffectsPacket.Right = reader.ReadUInt32();
              if (worldEffectsPacket.Background > 16777215U || worldEffectsPacket.Left > 16777215U || worldEffectsPacket.Right > 16777215U )
                return false;
              byte length1 = reader.ReadByte();
              byte length2 = reader.ReadByte();
              if (length1 != byte.MaxValue && length1 > (byte) 74 || length2 > (byte) 6)
                return false;
              int num3 = 24 + (((int) num2 & 1) != 0 ? 24 : 0) + (length1 == byte.MaxValue ? 0 : (int) length1 * 28) + (int) length2 * 40;
              if (bytes.Length != num3)
                return false;
              if (((int) num2 & 1) != 0)
              {
                worldEffectsPacket.Gun = WorldEffectsPacket.Read(reader, 6, 10000f);
                double num4 = 0.0;
                for (int index = 0; index < 3; ++index)
                  num4 += Math.Pow((double) worldEffectsPacket.Gun[index + 3] - (double) worldEffectsPacket.Gun[index], 2.0);
                if (num4 > 15625.0)
                  return false;
              }
              if (length1 != byte.MaxValue)
              {
                worldEffectsPacket.Platforms = new float[(int) length1][];
                for (int index1 = 0; index1 < (int) length1; ++index1)
                {
                  float[] numArray = WorldEffectsPacket.Read(reader, 7, 10000f);
                  double num5 = 0.0;
                  for (int index2 = 3; index2 < 7; ++index2)
                    num5 += (double) numArray[index2] * (double) numArray[index2];
                  if (num5 < 0.9 || num5 > 1.1)
                    return false;
                  worldEffectsPacket.Platforms[index1] = numArray;
                }
              }
              worldEffectsPacket.Shots = new WorldEffectsPacket.Shot[(int) length2];
              for (int index = 0; index < (int) length2; ++index)
              {
                WorldEffectsPacket.Shot shot = new WorldEffectsPacket.Shot()
                {
                  Serial = reader.ReadUInt32(),
                  Kind = reader.ReadUInt32(),
                  Timestamp = reader.ReadInt32(),
                  Position = WorldEffectsPacket.Read(reader, 3, 10000f),
                  Velocity = WorldEffectsPacket.Read(reader, 3, 100f),
                  Scale = reader.ReadSingle()
                };
                int num6 = now - shot.Timestamp;
                if (num6 < -2000 || num6 > 6000 || !WorldEffectsPacket.Finite(shot.Scale) || (double) shot.Scale < 0.05000000074505806 || (double) shot.Scale > 4.0)
                  return false;
                worldEffectsPacket.Shots[index] = shot;
              }
              packet = worldEffectsPacket;
              return true;
            }
          }
        }
        catch (IOException ex)
        {
          return false;
        }
        catch (ArgumentException ex)
        {
          return false;
        }
      }
    }
    return false;
  }
  private static float[] Read(BinaryReader reader, int count, float limit)
  {
    float[] numArray = new float[count];
    for (int index = 0; index < count; ++index)
    {
      numArray[index] = reader.ReadSingle();
      if (!WorldEffectsPacket.Finite(numArray[index]) || (double) Math.Abs(numArray[index]) > (double) limit)
        throw new ArgumentException("Invalid effect coordinate.");
    }
    return numArray;
  }
  private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
  public static uint Kind(string name)
  {
    uint num = 2166136261;
    foreach (char ch in name.Replace("(Clone)", "").Trim().ToLowerInvariant())
      num = (uint) (((int) num ^ (int) ch) * 16777619);
    return num;
  }
  public static bool Same(byte[] first, byte[] second)
  {
    if (first == null || second == null || first.Length != second.Length)
      return false;
    for (int index = 0; index < first.Length; ++index)
    {
      if ((index < 4 || index >= 8) && (int) first[index] != (int) second[index])
        return false;
    }
    return true;
  }
  public struct Shot
  {
    public uint Serial;
    public uint Kind;
    public int Timestamp;
    public float[] Position;
    public float[] Velocity;
    public float Scale;
  }
}
