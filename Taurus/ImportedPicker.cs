
using System;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class ImportedPicker
{
  private readonly Transform root;
  private readonly Transform picker;
  private readonly Action<Transform, Transform> map;
  private readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
  public ImportedPicker(
    Transform root,
    Transform source,
    Bounds content,
    Vector3 forward,
    Vector3 right,
    Vector3 up,
    Action<Transform, Transform> map)
  {
    this.root = root;
    this.map = map;
    Bounds bounds = ImportedSurface.BoundsOf(source);
    this.picker = Object.Instantiate<Transform>(root.Find("ColorPicker"), source, false);
    ((Object) this.picker).name = "DesignColorPicker";
    string[] strArray = new string[2]
    {
      "Wheel",
      "Brightness"
    };
    foreach (string str in strArray)
    {
      Transform transform = this.picker.Find(str);
      transform.localPosition = new Vector3(0.069f, 0.0f, str == "Wheel" ? -0.028f : -0.175f);
      transform.localRotation = Quaternion.identity;
      transform.localScale = Vector3.one;
      ((Component) transform).GetComponent<BoxCollider>().size = new Vector3(3f / 500f, 0.22f, str == "Wheel" ? 0.22f : 0.022f);
    }
    float num1 = Mathf.Min((float) ((double) Vector3.Dot(ImportedPicker.Abs(right), (content).size) / 0.2800000011920929), Vector3.Dot(ImportedPicker.Abs(up), (content).size) / 0.36f);
    this.picker.SetPositionAndRotation((content).center, Quaternion.LookRotation(up, (-right)));
    Transform picker = this.picker;
    Vector3 vector3_1 = source.InverseTransformVector((-forward));
    double magnitude1 = (double) (vector3_1).magnitude;
    Vector3 vector3_2 = source.InverseTransformVector((-right));
    double magnitude2 = (double) (vector3_2).magnitude;
    Vector3 vector3_3 = source.InverseTransformVector(up);
    double magnitude3 = (double) (vector3_3).magnitude;
    Vector3 vector3_4 = (new Vector3((float) magnitude1, (float) magnitude2, (float) magnitude3) * num1);
    picker.localScale = vector3_4;
    float num2 = Vector3.Dot((-forward), (content).center) + Vector3.Dot(ImportedPicker.Abs(forward), (content).extents);
    float num3 = Vector3.Dot((-forward), (bounds).center) + Vector3.Dot(ImportedPicker.Abs(forward), (bounds).extents);
    this.picker.position = (((content).center - (forward * (float) ((double) Mathf.Max(num2, num3) - (double) Vector3.Dot((-forward), (content).center) + 0.001500000013038516))) - this.picker.TransformVector(new Vector3(0.069f, 0.0f, -0.028f)));
    foreach (Component componentsInChild in ((Component) this.picker).GetComponentsInChildren<Transform>(true))
      componentsInChild.gameObject.layer = 31 ;
    foreach (Renderer componentsInChild in ((Component) this.picker).GetComponentsInChildren<Renderer>(true))
      componentsInChild.enabled = true;
    ((Component) this.picker).gameObject.SetActive(false);
  }
  private static Vector3 Abs(Vector3 v)
  {
    return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
  }
  public bool Refresh(MenuState state)
  {
    ((Component) this.picker).gameObject.SetActive(state.ColorPickerOpen);
    if (!state.ColorPickerOpen)
      return false;
    foreach (Transform transform1 in this.picker)
    {
      Transform transform2 = this.root.Find("ColorPicker/" + ((Object) transform1).name);
      ((Component) transform2).GetComponent<Renderer>().GetPropertyBlock(this.properties);
      ((Component) transform1).GetComponent<Renderer>().SetPropertyBlock(this.properties);
      if (((Object) transform1).name.EndsWith("Mark", StringComparison.Ordinal))
        transform1.localPosition = transform2.localPosition;
      TextMesh textMesh;
      if (((Component) transform1).TryGetComponent<TextMesh>(out textMesh))
      {
        textMesh.text = ((Component) transform2).GetComponent<TextMesh>().text;
        textMesh.color = Color.white;
      }
    }
    string[] strArray = new string[2]
    {
      "Wheel",
      "Brightness"
    };
    foreach (string str in strArray)
    {
      Transform transform = this.root.Find("ColorPicker/" + str);
      this.map(transform, this.picker.Find(str));
      float num = (float) ((double) ((Component) transform).GetComponent<BoxCollider>().size.y * (double) transform.localScale.x / 0.2199999988079071);
      transform.localScale = (Vector3.one * num);
      this.map(transform, this.picker.Find(str));
    }
    return true;
  }
}
