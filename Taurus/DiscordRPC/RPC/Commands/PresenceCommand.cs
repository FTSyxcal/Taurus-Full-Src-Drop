
using Taurus.DiscordRPC.RPC.Payload;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC.RPC.Commands;
internal class PresenceCommand : ICommand
{
  [JsonProperty("pid")]
  public int PID { get; set; }
  [JsonProperty("activity")]
  public RichPresence Presence { get; set; }
  public IPayload PreparePayload(long nonce)
  {
    ArgumentPayload argumentPayload = new ArgumentPayload((object) this, nonce);
    argumentPayload.Command = Command.SetActivity;
    return (IPayload) argumentPayload;
  }
}
