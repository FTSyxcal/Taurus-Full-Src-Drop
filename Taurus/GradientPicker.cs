
using System;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class GradientPicker
{
  private readonly MenuState state;
  private readonly Action changed;
  private readonly Action commit;
  private bool dirty;
  private static readonly string[] controls = new string[2]
  {
    "Wheel",
    "Brightness"
  };
  private static readonly MaterialPropertyBlock colors = new MaterialPropertyBlock();
  public GradientPicker(MenuState state, Action changed, Action commit)
  {
    this.state = state;
    this.changed = changed;
    this.commit = commit;
  }
  public static void Refresh(Transform root, MenuState state)
  {
    Transform transform1 = root.Find("ColorPicker");
    ((Component) transform1).gameObject.SetActive(state.ColorPickerOpen);
    if (!state.ColorPickerOpen)
      return;
    GradientPicker.Place(root, new Vector3(0.069f, 0.0f, 0.0f), 1f);
    foreach (string control in GradientPicker.controls)
    {
      Transform transform2 = transform1.Find(control);
      transform2.localPosition = new Vector3(0.069f, 0.0f, control == "Wheel" ? -0.028f : -0.175f);
      transform2.localRotation = Quaternion.identity;
      transform2.localScale = Vector3.one;
      ((Component) transform2).GetComponent<BoxCollider>().size = new Vector3(3f / 500f, 0.22f, control == "Wheel" ? 0.22f : 0.022f);
    }
    uint num1 = state.EditingEnd ? state.GradientEnd : state.GradientStart;
    ((Component) transform1.Find("Caption")).GetComponent<TextMesh>().text = $"{(state.EditingEnd ? "Right" : "Left")} color  #{num1.ToString("X6")}";
    GradientPicker.colors.Clear();
    GradientPicker.colors.SetFloat("_Value", state.PickerValue);
    ((Component) transform1.Find("Wheel")).GetComponent<Renderer>().SetPropertyBlock(GradientPicker.colors);
    GradientPicker.colors.SetFloat("_Hue", state.PickerHue);
    GradientPicker.colors.SetFloat("_Saturation", state.PickerSaturation);
    ((Component) transform1.Find("Brightness")).GetComponent<Renderer>().SetPropertyBlock(GradientPicker.colors);
    float num2 = (float) ((double) state.PickerHue * 3.1415927410125732 * 2.0);
    transform1.Find("WheelMark").localPosition = new Vector3(0.073f, (float) (-(double) Mathf.Cos(num2) * (double) state.PickerSaturation * 0.10999999940395355), (float) ((double) Mathf.Sin(num2) * (double) state.PickerSaturation * 0.10999999940395355 - 0.028000000864267349));
    transform1.Find("BrightnessMark").localPosition = new Vector3(0.073f, (float) (0.10999999940395355 - (double) state.PickerValue * 0.2199999988079071), -0.175f);
  }
  public static void Place(Transform root, Vector3 wheelCenter, float scale)
  {
    Transform transform = root.Find("ColorPicker");
    transform.localRotation = Quaternion.identity;
    transform.localScale = (Vector3.one * scale);
    transform.localPosition = (wheelCenter - (new Vector3(0.069f, 0.0f, -0.028f) * scale));
  }
  private void Select(Transform control, Vector3 point)
  {
    Vector3 vector3 = control.InverseTransformPoint(point);
    float hue = this.state.PickerHue;
    float saturation = this.state.PickerSaturation;
    float num1 = this.state.PickerValue;
    if (((Object) control).name == "Wheel")
    {
      float num2 = (float) (-(double) vector3.y / 0.10999999940395355);
      float num3 = vector3.z / 0.11f;
      saturation = Mathf.Sqrt((float) ((double) num2 * (double) num2 + (double) num3 * (double) num3));
      if ((double) saturation > 1.0)
        return;
      hue = Mathf.Repeat(Mathf.Atan2(num3, num2) / 6.28318548f, 1f);
    }
    else
      num1 = Mathf.Clamp01((float) (0.5 - (double) vector3.y / 0.2199999988079071));
    if (!this.state.SetGradientHSV(hue, saturation, num1))
      return;
    this.dirty = true;
    this.changed();
  }
  public bool Touch(Transform root, Vector3 point)
  {
    if (this.state.ColorPickerOpen)
    {
      foreach (string control1 in GradientPicker.controls)
      {
        Transform control2 = root.Find("ColorPicker/" + control1);
        Vector3 vector3 = (((Collider) ((Component) control2).GetComponent<BoxCollider>()).ClosestPoint(point) - point);
        if ((double) (vector3).sqrMagnitude <= 2.4999999368446879E-05)
        {
          this.Select(control2, point);
          return true;
        }
      }
    }
    this.Release();
    return false;
  }
  public bool Mouse(Transform root, Ray ray, Event input)
  {
    if (input.type == (EventType) 1)
      this.Release();
    if (!this.state.ColorPickerOpen)
      return false;
    foreach (string control1 in GradientPicker.controls)
    {
      Transform control2 = root.Find("ColorPicker/" + control1);
      RaycastHit raycastHit;
      if (((Collider) ((Component) control2).GetComponent<BoxCollider>()).Raycast(ray, out raycastHit, 2f))
      {
        if ((input.type == (EventType) 0 || input.type == (EventType) 3) && input.button == 0)
          this.Select(control2, (raycastHit).point);
        return true;
      }
    }
    return false;
  }
  public void Release()
  {
    if (!this.dirty)
      return;
    this.dirty = false;
    this.commit();
  }
}
