
using System;
using System.Text;
using Taurus.DiscordRPC.Exceptions;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC;
public class Button
{
  private string _label;
  private string _url;
  [JsonProperty("label")]
  public string Label
  {
    get => this._label;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._label, true, 31 , Encoding.UTF8))
        throw new StringOutOfRangeException(31 );
    }
  }
  [JsonProperty("url")]
  public string Url
  {
    get => this._url;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._url, false, 512 ))
        throw new StringOutOfRangeException(512 );
      if (!BaseRichPresence.ValidateUrl(this._url))
        throw new ArgumentException("Url must be a valid URI");
    }
  }
}
