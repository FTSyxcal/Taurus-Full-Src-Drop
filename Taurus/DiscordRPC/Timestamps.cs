
using System;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC;
[Serializable]
public class Timestamps
{
  public static Timestamps Now => new Timestamps(DateTime.UtcNow);
  [JsonIgnore]
  public DateTime? Start { get; set; }
  [JsonIgnore]
  public DateTime? End { get; set; }
  [JsonProperty("start")]
  public ulong? StartUnixMilliseconds
  {
    get
    {
      return !this.Start.HasValue ? new ulong?() : new ulong?(Timestamps.ToUnixMilliseconds(this.Start.Value));
    }
    set
    {
      this.Start = value.HasValue ? new DateTime?(Timestamps.FromUnixMilliseconds(value.Value)) : new DateTime?();
    }
  }
  [JsonProperty("end")]
  public ulong? EndUnixMilliseconds
  {
    get
    {
      return !this.End.HasValue ? new ulong?() : new ulong?(Timestamps.ToUnixMilliseconds(this.End.Value));
    }
    set
    {
      this.End = value.HasValue ? new DateTime?(Timestamps.FromUnixMilliseconds(value.Value)) : new DateTime?();
    }
  }
  public static Timestamps FromTimeSpan(double seconds)
  {
    return Timestamps.FromTimeSpan(TimeSpan.FromSeconds(seconds));
  }
  public static Timestamps FromTimeSpan(TimeSpan timespan)
  {
    return new Timestamps()
    {
      Start = new DateTime?(DateTime.UtcNow),
      End = new DateTime?(DateTime.UtcNow + timespan)
    };
  }
  public Timestamps()
  {
    this.Start = new DateTime?();
    this.End = new DateTime?();
  }
  public Timestamps(DateTime start)
  {
    this.Start = new DateTime?(start);
    this.End = new DateTime?();
  }
  public Timestamps(DateTime start, DateTime end)
  {
    this.Start = new DateTime?(start);
    this.End = new DateTime?(end);
  }
  public static DateTime FromUnixMilliseconds(ulong unixTime)
  {
    return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToDouble(unixTime));
  }
  public static ulong ToUnixMilliseconds(DateTime date)
  {
    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    return Convert.ToUInt64((date - dateTime).TotalMilliseconds);
  }
}
