
using Taurus.DiscordRPC.RPC.Payload;
namespace Taurus.DiscordRPC.RPC.Commands;
internal interface ICommand
{
  IPayload PreparePayload(long nonce);
}
