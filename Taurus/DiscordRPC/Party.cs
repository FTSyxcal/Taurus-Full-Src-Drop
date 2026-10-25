
using System;
using Taurus.DiscordRPC.Helper;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC;
[Serializable]
public class Party
{
  private string _partyid;
  [JsonProperty("id")]
  public string ID
  {
    get => this._partyid;
    set => this._partyid = value.GetNullOrString();
  }
  [JsonIgnore]
  public int Size { get; set; }
  [JsonIgnore]
  public int Max { get; set; }
  [JsonProperty("privacy")]
  public Party.PrivacySetting Privacy { get; set; }
  [JsonProperty("size")]
  private int[] _size
  {
    get
    {
      int val1 = Math.Max(1, this.Size);
      return new int[2]{ val1, Math.Max(val1, this.Max) };
    }
    set
    {
      if (value.Length != 2)
      {
        this.Size = 0;
        this.Max = 0;
      }
      else
      {
        this.Size = value[0];
        this.Max = value[1];
      }
    }
  }
  public enum PrivacySetting
  {
    Private,
    Public,
  }
}
