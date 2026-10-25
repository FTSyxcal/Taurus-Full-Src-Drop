
using System;
namespace Taurus.DiscordRPC.Exceptions;
public class BadPresenceException : Exception
{
  internal BadPresenceException(string message)
    : base(message)
  {
  }
}
