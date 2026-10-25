
using System;
namespace Taurus.DiscordRPC.Message;
public class ConnectionEstablishedMessage : IMessage
{
  public override MessageType Type => MessageType.ConnectionEstablished;
  [Obsolete("The connected pipe is not neccessary information.")]
  public int ConnectedPipe { get; internal set; }
}
