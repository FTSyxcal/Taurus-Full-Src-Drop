
namespace Taurus;
public static class MenuClickSounds
{
  public static readonly string[] Names = new string[8]
  {
    "Default",
    "Bark",
    "Bite",
    "Minecraft Click",
    "Click",
    "Pluh",
    "Secret",
    "Creamy Click"
  };
  public static readonly string[] Paths = new string[8]
  {
    "Assets/Audio/menu_click.wav",
    "Assets/Audio/Clicks/Bark.wav",
    "Assets/Audio/Clicks/Bite.ogg",
    "Assets/Audio/Clicks/Minecraft Click.mp3",
    "Assets/Audio/Clicks/Click.mp3",
    "Assets/Audio/Clicks/Pluh.mp3",
    "Assets/Audio/Clicks/Secret.mp3",
    "Assets/Audio/Clicks/CreamyClick.mp3"
  };
  public const float MaxDuration = 1f;
  public static int DefaultIndex(int design)
  {
    switch (design)
    {
      case 10:
        return 7;
      case 12:
      case 13:
        return 1;
      default:
        return 0;
    }
  }
}
