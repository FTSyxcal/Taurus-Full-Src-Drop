
using System;
namespace Taurus;
[Serializable]
public sealed class MenuSaveData
{
  public int version = 1;
  public string[] enabledMods;
  public string[] favoriteMods;
  public string theme = "ItzGame";
  public string design = "Taurus";
  public string font = "Minecraft";
  public bool rightHanded;
  public bool rounded = true;
  public float flySpeed = 12f;
  public float vrScale = 0.65f;
  public bool discordAccepted;
  public float soundVolume = 0.5f;
  public float micVolume = 0.65f;
  public float pcScale = 1.3f;
  public float clickVolume = 0.7f;
  public string clickSound = "Default";
  public float whooshVolume = 0.2f;
  public uint gradientStart = 16730955;
  public uint gradientEnd = 16751423;
  public float gradientPeriod = 20f;
}
