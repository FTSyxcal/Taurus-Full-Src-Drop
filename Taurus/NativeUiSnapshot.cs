
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
internal sealed class NativeUiSnapshot : IDisposable
{
  public GameObject Source;
  public float PhysicalScale = 1f;
  public Vector3 Forward = Vector3.left;
  public Vector3 Up = Vector3.forward;
  public Bounds? PickerRegion;
  public Transform Content;
  public Transform HomeIcon;
  public Transform NotificationTemplate;
  public Transform NotificationFrame;
  public TMP_Text Header;
  public TMP_Text Footer;
  public TMP_Text HomeLabel;
  public Text HeaderLegacy;
  public readonly Dictionary<string, Transform> Controls = new Dictionary<string, Transform>();
  public readonly List<Material> Materials = new List<Material>();
  public readonly List<Mesh> Meshes = new List<Mesh>();
  public readonly List<Material> Pinwheels = new List<Material>();
  public Action<float> Animate;
  public Func<float, bool> Update;
  public void Dispose()
  {
    foreach (Object material in this.Materials)
      NativeUiSnapshot.Destroy(material);
    foreach (Object mesh in this.Meshes)
      NativeUiSnapshot.Destroy(mesh);
  }
  public static void Destroy(UnityEngine.Object item)
  {
    if (Application.isPlaying)
      Object.Destroy(item);
    else
      Object.DestroyImmediate(item);
  }
}
