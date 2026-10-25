
using System.IO;
using UnityEngine;
namespace Taurus;
public static class MenuSave
{
  public static MenuSaveData Load(string path)
  {
    try
    {
      return MenuSave.Read(path);
    }
    catch
    {
      if (File.Exists(path + ".bak"))
        return MenuSave.Read(path + ".bak");
      throw;
    }
  }
  private static MenuSaveData Read(string path)
  {
    MenuSaveData menuSaveData = new FileInfo(path).Length <= 65536L ? JsonUtility.FromJson<MenuSaveData>(File.ReadAllText(path)) : throw new InvalidDataException("Menu save is too large.");
    return menuSaveData != null && menuSaveData.version == 1 ? menuSaveData : throw new InvalidDataException("Unsupported menu save version.");
  }
  public static void Save(string path, MenuSaveData data)
  {
    Directory.CreateDirectory(Path.GetDirectoryName(path));
    string str = path + ".tmp";
    File.WriteAllText(str, JsonUtility.ToJson((object) data, true));
    if (File.Exists(path))
      File.Replace(str, path, path + ".bak");
    else
      File.Move(str, path);
  }
}
