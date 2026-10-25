
using System;
namespace Taurus.DiscordRPC;
[Flags]
public enum EventType
{
  None = 0,
  [Obsolete("Spectating is no longer supported by Discord.")] Spectate = 1,
  Join = 2,
  JoinRequest = 4,
}
