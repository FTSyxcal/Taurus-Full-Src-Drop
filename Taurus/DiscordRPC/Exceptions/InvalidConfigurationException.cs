
using System;
namespace Taurus.DiscordRPC.Exceptions;
public class InvalidConfigurationException : Exception
{
  internal InvalidConfigurationException(string message)
    : base(message)
  {
  }
}
