
namespace Taurus;
public sealed class MenuEntry
{
  public readonly string Name;
  public readonly int Group;
  public readonly int Target;
  public readonly bool Toggle;
  public readonly bool Available;
  public readonly string Reason;
  public readonly bool RequiresAntiban;
  public MenuEntry(
    string name,
    int group,
    int target,
    bool toggle,
    bool available,
    string reason,
    bool requiresAntiban)
  {
    this.Name = name;
    this.Group = group;
    this.Target = target;
    this.Toggle = toggle;
    this.Available = available;
    this.Reason = reason;
    this.RequiresAntiban = requiresAntiban;
  }
}
