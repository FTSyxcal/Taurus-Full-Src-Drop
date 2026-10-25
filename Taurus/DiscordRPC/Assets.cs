
using System;
using Taurus.DiscordRPC.Exceptions;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC;
[Serializable]
public class Assets
{
  private const string EXTERNAL_KEY_PREFIX = "mp:external";
  private string _largeimagekey;
  private string _largeimagetext;
  private string _largeimageurl;
  private string _smallimagekey;
  private string _smallimagetext;
  private string _smallimageurl;
  [JsonProperty("large_image")]
  public string LargeImageKey
  {
    get => this._largeimagekey;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._largeimagekey, false, 256 ))
        throw new StringOutOfRangeException(256 );
      this.LargeImageID = (string) null;
    }
  }
  [JsonProperty("large_text")]
  public string LargeImageText
  {
    get => this._largeimagetext;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._largeimagetext, false, 128 ))
        throw new StringOutOfRangeException(128 );
    }
  }
  [JsonProperty("large_url")]
  public string LargeImageUrl
  {
    get => this._largeimageurl;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._largeimageurl, false, 256 ))
        throw new StringOutOfRangeException(256 );
      if (!BaseRichPresence.ValidateUrl(this._largeimageurl))
        throw new ArgumentException("Url must be a valid URI");
    }
  }
  [JsonIgnore]
  public string LargeImageID { get; private set; }
  [JsonIgnore]
  public bool IsLargeImageKeyExternal { get; private set; }
  [JsonProperty("small_image")]
  public string SmallImageKey
  {
    get => this._smallimagekey;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._smallimagekey, false, 256 ))
        throw new StringOutOfRangeException(256 );
      this.SmallImageID = (string) null;
    }
  }
  [JsonProperty("small_text")]
  public string SmallImageText
  {
    get => this._smallimagetext;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._smallimagetext, false, 128 ))
        throw new StringOutOfRangeException(128 );
    }
  }
  [JsonProperty("small_url")]
  public string SmallImageUrl
  {
    get => this._smallimageurl;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._smallimageurl, false, 256 ))
        throw new StringOutOfRangeException(256 );
      if (!BaseRichPresence.ValidateUrl(this._smallimageurl))
        throw new ArgumentException("Url must be a valid URI");
    }
  }
  [JsonIgnore]
  public string SmallImageID { get; private set; }
  [JsonIgnore]
  public bool IsSmallImageKeyExternal { get; private set; }
  internal void Merge(Assets other)
  {
    this._smallimagetext = other._smallimagetext;
    this._smallimageurl = other._smallimageurl;
    this._largeimagetext = other._largeimagetext;
    this._largeimageurl = other._largeimageurl;
    string s1 = other._largeimagekey ?? "";
    ulong result;
    if (s1.StartsWith("mp:external"))
    {
      this.IsLargeImageKeyExternal = true;
      this.LargeImageID = s1;
    }
    else if (ulong.TryParse(s1, out result))
    {
      this.IsLargeImageKeyExternal = false;
      this.LargeImageID = s1;
    }
    else
    {
      this.IsLargeImageKeyExternal = false;
      this.LargeImageID = (string) null;
      this._largeimagekey = s1;
    }
    string s2 = other._smallimagekey ?? "";
    if (s2.StartsWith("mp:external"))
    {
      this.IsSmallImageKeyExternal = true;
      this.SmallImageID = s2;
    }
    else if (ulong.TryParse(s2, out result))
    {
      this.IsSmallImageKeyExternal = false;
      this.SmallImageID = s2;
    }
    else
    {
      this.IsSmallImageKeyExternal = false;
      this.SmallImageID = (string) null;
      this._smallimagekey = s2;
    }
  }
}
