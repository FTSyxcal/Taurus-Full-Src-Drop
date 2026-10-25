
using System;
using System.IO;
namespace Taurus;
public static class MenuDataPaths
{
  public static string DirectoryFor(string gameRoot) => MenuDataPaths.Child(gameRoot, "Taurus");
  public static void Migrate(string gameRoot)
  {
    string str1 = MenuDataPaths.Child(gameRoot, "ItzGameMenu");
    string str2 = MenuDataPaths.DirectoryFor(gameRoot);
    string[] strArray1 = new string[2]{ str1, str2 };
    foreach (string path in strArray1)
    {
      if (Directory.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != (FileAttributes) 0)
        throw new IOException("Menu data directory must not be a link.");
    }
    Directory.CreateDirectory(str2);
    string[] strArray2 = new string[3]
    {
      "settings.json",
      "settings.json.bak",
      "feature-status.json"
    };
    foreach (string name in strArray2)
      MenuDataPaths.MoveFile(MenuDataPaths.Child(str1, name), MenuDataPaths.Child(str2, name));
    MenuDataPaths.MoveSounds(MenuDataPaths.Child(str1, "Sounds"), MenuDataPaths.Child(str2, "Sounds"));
    string[] strArray3 = new string[2]{ str1, str2 };
    foreach (string root in strArray3)
    {
      string[] strArray4 = new string[2]
      {
        "Runtime",
        "DiscordProfile"
      };
      foreach (string name in strArray4)
      {
        string path = MenuDataPaths.Child(root, name);
        if (Directory.Exists(path))
          Directory.Delete(path, (File.GetAttributes(path) & FileAttributes.ReparsePoint) == (FileAttributes) 0);
      }
    }
    if (!Directory.Exists(str1) || Directory.GetFileSystemEntries(str1).Length != 0)
      return;
    Directory.Delete(str1);
  }
  private static string Child(string root, string name)
  {
    string path1 = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar.ToString();
    string fullPath = Path.GetFullPath(Path.Combine(path1, name));
    return fullPath.StartsWith(path1, StringComparison.OrdinalIgnoreCase) ? fullPath : throw new IOException("Menu data path escapes its directory.");
  }
  private static void MoveFile(string source, string target)
  {
    if (!File.Exists(source) || File.Exists(target) || (File.GetAttributes(source) & FileAttributes.ReparsePoint) != (FileAttributes) 0)
      return;
    File.Move(source, target);
  }
  private static void MoveSounds(string source, string target)
  {
    if (!Directory.Exists(source) || (File.GetAttributes(source) & FileAttributes.ReparsePoint) != (FileAttributes) 0)
      return;
    if (Directory.Exists(target) && (File.GetAttributes(target) & FileAttributes.ReparsePoint) != (FileAttributes) 0)
      throw new IOException("Sound directory must not be a link.");
    Directory.CreateDirectory(target);
    foreach (string file in Directory.GetFiles(source))
      MenuDataPaths.MoveFile(file, MenuDataPaths.Child(target, Path.GetFileName(file)));
    foreach (string directory in Directory.GetDirectories(source))
      MenuDataPaths.MoveSounds(directory, MenuDataPaths.Child(target, Path.GetFileName(directory)));
    if (Directory.GetFileSystemEntries(source).Length != 0)
      return;
    Directory.Delete(source);
  }
}
