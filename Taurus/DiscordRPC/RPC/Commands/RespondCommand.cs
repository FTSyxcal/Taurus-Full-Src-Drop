
using Taurus.DiscordRPC.RPC.Payload;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC.RPC.Commands;
internal class RespondCommand : ICommand
{
  [JsonProperty("user_id")]
  public string UserID { get; set; }
  [JsonIgnore]
  public bool Accept { get; set; }
  public IPayload PreparePayload(long nonce)
  {
    ArgumentPayload argumentPayload = new ArgumentPayload((object) this, nonce);
    argumentPayload.Command = this.Accept ? Command.SendActivityJoinInvite : Command.CloseActivityJoinRequest;
    return (IPayload) argumentPayload;
  }
}
