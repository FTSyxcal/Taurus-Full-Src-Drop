
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC;
internal sealed class RichPresenceResponse : BaseRichPresence
{
  [JsonProperty("application_id")]
  public string ClientID { get; private set; }
  [JsonProperty("name")]
  public string Name { get; private set; }
}
