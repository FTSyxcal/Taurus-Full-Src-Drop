
using UnityEngine;
namespace Taurus;
public static class MenuPose
{
  public static Vector3 PointerPosition(Transform controller)
  {
    return controller.TransformPoint(new Vector3(0.0f, -0.1f, 0.0f));
  }
  public static Vector3 RelativePointer(Transform anchor, Transform controller)
  {
    return anchor.InverseTransformPoint(MenuPose.PointerPosition(controller));
  }
  public static Pose Local(Transform hand, Vector3 offset, Vector3 rotation, bool rightHanded)
  {
    Quaternion quaternion = (hand.rotation * Quaternion.Euler((rotation + (rightHanded ? new Vector3(0.0f, 0.0f, 180f) : Vector3.zero))));
    return new Pose((hand.TransformPoint(offset) + ((quaternion * Vector3.right) * 0.02f)), quaternion);
  }
  public static Pose Relative(Transform anchor, Pose pose)
  {
    return new Pose(anchor.InverseTransformPoint(pose.position), (Quaternion.Inverse(anchor.rotation) * pose.rotation));
  }
  public static Pose World(Transform anchor, Pose pose)
  {
    return new Pose(anchor.TransformPoint(pose.position), (anchor.rotation * pose.rotation));
  }
}
