
using System;
using Taurus.DiscordRPC.Message;
namespace Taurus.DiscordRPC.Events;
[Obsolete("Spectating is no longer supported by Discord.")]
public delegate void OnSpectateEvent(object sender, SpectateMessage args);
