
using System;
using System.Text;
using Taurus.DiscordRPC.Exceptions;
using Taurus.DiscordRPC.Helper;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC;
[JsonObject]
[Serializable]
public class BaseRichPresence
{
  protected internal string _state;
  protected internal string _stateUrl;
  protected internal string _details;
  protected internal string _detailsUrl;
  [JsonProperty("state")]
  public string State
  {
    get => this._state;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._state, false, 128 ))
        throw new StringOutOfRangeException(nameof (State), 0, 128 );
    }
  }
  [JsonProperty("state_url")]
  public string StateUrl
  {
    get => this._stateUrl;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._stateUrl, false, 256 ))
        throw new StringOutOfRangeException(256 );
      if (!BaseRichPresence.ValidateUrl(this._stateUrl))
        throw new ArgumentException("Url must be a valid URI");
    }
  }
  [JsonProperty("details")]
  public string Details
  {
    get => this._details;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._details, false, 128 ))
        throw new StringOutOfRangeException(128 );
    }
  }
  [JsonProperty("details_url")]
  public string DetailsUrl
  {
    get => this._detailsUrl;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._detailsUrl, false, 256 ))
        throw new StringOutOfRangeException(256 );
      if (!BaseRichPresence.ValidateUrl(this._detailsUrl))
        throw new ArgumentException("Url must be a valid URI");
    }
  }
  [JsonProperty("timestamps")]
  public Timestamps Timestamps { get; set; }
  [JsonProperty("assets")]
  public Assets Assets { get; set; }
  [JsonProperty("party")]
  public Party Party { get; set; }
  [JsonProperty("secrets")]
  public Secrets Secrets { get; set; }
  [JsonProperty("type")]
  public ActivityType Type { get; set; }
  [JsonProperty("status_display_type")]
  public StatusDisplayType StatusDisplay { get; set; }
  public bool HasTimestamps()
  {
    if (this.Timestamps == null)
      return false;
    return this.Timestamps.Start.HasValue || this.Timestamps.End.HasValue;
  }
  public bool HasAssets() => this.Assets != null;
  public bool HasParty() => this.Party != null && this.Party.ID != null;
  public bool HasSecrets()
  {
    if (this.Secrets == null)
      return false;
    return this.Secrets.Join != null || this.Secrets.SpectateSecret != null;
  }
  internal static bool ValidateString(
    string str,
    out string result,
    bool useBytes,
    int length,
    Encoding encoding = null)
  {
    result = str;
    if (str == null)
      return true;
    string str1 = str.Trim();
    if (useBytes && !str1.WithinLength(length, encoding) || str1.Length > length)
      return false;
    result = str1.GetNullOrString();
    return true;
  }
  internal static bool ValidateUrl(string url)
  {
    return string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out Uri _);
  }
  public static implicit operator bool(BaseRichPresence presence) => presence != null;
  internal virtual bool Matches(RichPresence other)
  {
    if (other == null || this.State != other.State || this.StateUrl != other.StateUrl || this.Details != other.Details || this.DetailsUrl != other.DetailsUrl || this.Type != other.Type)
      return false;
    if (this.Timestamps != null)
    {
      if (other.Timestamps != null)
      {
        ulong? unixMilliseconds1 = other.Timestamps.StartUnixMilliseconds;
        ulong? unixMilliseconds2 = this.Timestamps.StartUnixMilliseconds;
        if ((long) unixMilliseconds1.GetValueOrDefault() == (long) unixMilliseconds2.GetValueOrDefault() & unixMilliseconds1.HasValue == unixMilliseconds2.HasValue)
        {
          unixMilliseconds2 = other.Timestamps.EndUnixMilliseconds;
          ulong? unixMilliseconds3 = this.Timestamps.EndUnixMilliseconds;
          if ((long) unixMilliseconds2.GetValueOrDefault() == (long) unixMilliseconds3.GetValueOrDefault() & unixMilliseconds2.HasValue == unixMilliseconds3.HasValue)
            goto label_9;
        }
      }
      return false;
    }
    if (other.Timestamps != null)
      return false;
label_9:
    if (this.Secrets != null)
    {
      if (other.Secrets == null || other.Secrets.Join != this.Secrets.Join || other.Secrets.SpectateSecret != this.Secrets.SpectateSecret)
        return false;
    }
    else if (other.Secrets != null)
      return false;
    if (this.Party != null)
    {
      if (other.Party == null || other.Party.ID != this.Party.ID || other.Party.Max != this.Party.Max || other.Party.Size != this.Party.Size || other.Party.Privacy != this.Party.Privacy)
        return false;
    }
    else if (other.Party != null)
      return false;
    if (this.Assets != null)
    {
      if (other.Assets == null || other.Assets.LargeImageKey != this.Assets.LargeImageKey || other.Assets.LargeImageText != this.Assets.LargeImageText || other.Assets.LargeImageUrl != this.Assets.LargeImageUrl || other.Assets.SmallImageKey != this.Assets.SmallImageKey || other.Assets.SmallImageText != this.Assets.SmallImageText || other.Assets.SmallImageUrl != this.Assets.SmallImageUrl)
        return false;
    }
    else if (other.Assets != null)
      return false;
    return true;
  }
  public RichPresence ToRichPresence()
  {
    RichPresence richPresence1 = new RichPresence();
    richPresence1.State = this.State;
    richPresence1.StateUrl = this.StateUrl;
    richPresence1.Details = this.Details;
    richPresence1.DetailsUrl = this.DetailsUrl;
    richPresence1.Type = this.Type;
    richPresence1.StatusDisplay = this.StatusDisplay;
    richPresence1.Party = !this.HasParty() ? this.Party : (Party) null;
    richPresence1.Secrets = !this.HasSecrets() ? this.Secrets : (Secrets) null;
    RichPresence richPresence2 = richPresence1;
    if (this.HasAssets())
      richPresence2.Assets = new Assets()
      {
        SmallImageKey = this.Assets.SmallImageKey,
        SmallImageText = this.Assets.SmallImageText,
        SmallImageUrl = this.Assets.SmallImageUrl,
        LargeImageKey = this.Assets.LargeImageKey,
        LargeImageText = this.Assets.LargeImageText,
        LargeImageUrl = this.Assets.LargeImageUrl
      };
    if (this.HasTimestamps())
    {
      richPresence2.Timestamps = new Timestamps();
      if (this.Timestamps.Start.HasValue)
        richPresence2.Timestamps.Start = this.Timestamps.Start;
      if (this.Timestamps.End.HasValue)
        richPresence2.Timestamps.End = this.Timestamps.End;
    }
    return richPresence2;
  }
}
