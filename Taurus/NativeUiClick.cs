
using System.Collections.Generic;
using UnityEngine;
namespace Taurus;
public sealed class NativeUiClick : MonoBehaviour
{
  public string Name;
  private readonly Dictionary<Transform, Vector3> bodies = new Dictionary<Transform, Vector3>();
  private readonly HashSet<Transform> uniforms = new HashSet<Transform>();
  private Transform pulseKnob;
  private Vector3 pulseOffWorld;
  private Vector3 pulseOnWorld;
  private Vector3 pulseOff;
  private Vector3 pulseOn;
  private Renderer pulseRenderer;
  private List<Renderer> pulseParts;
  private Color pulseOffColor;
  private Color pulseOnColor;
  public IEnumerable<Transform> Visuals => (IEnumerable<Transform>) this.bodies.Keys;
  public bool HasPulse => ((Object) this.pulseKnob != (Object) null);
  public float PulseTarget { get; private set; }
  public void RegisterBody(Transform body)
  {
    if (!((Object) body != (Object) null))
      return;
    this.bodies[body] = body.localScale;
  }
  public void RegisterUniform(Transform body)
  {
    this.RegisterBody(body);
    if (!((Object) body != (Object) null))
      return;
    this.uniforms.Add(body);
  }
  public void Rebase()
  {
    foreach (Transform key in new List<Transform>((IEnumerable<Transform>) this.bodies.Keys))
    {
      if (((Object) key != (Object) null))
        this.bodies[key] = key.localScale;
    }
    if (!((Object) this.pulseKnob != (Object) null))
      return;
    this.pulseOff = this.pulseKnob.parent.InverseTransformPoint(this.pulseOffWorld);
    this.pulseOn = this.pulseKnob.parent.InverseTransformPoint(this.pulseOnWorld);
  }
  public void ApplyFactor(float factor)
  {
    foreach (KeyValuePair<Transform, Vector3> body in this.bodies)
    {
      if (((Object) body.Key != (Object) null))
        body.Key.localScale = this.uniforms.Contains(body.Key) ? (body.Value * factor) : new Vector3(body.Value.x, body.Value.y * factor, body.Value.z * factor);
    }
  }
  public void PlayBounce()
  {
  }
  public void SetupPulse(
    Transform knob,
    Renderer renderer,
    float off,
    float on,
    List<Renderer> parts,
    Color offColor,
    Color onColor)
  {
    this.pulseKnob = knob;
    this.pulseRenderer = renderer;
    this.pulseParts = parts;
    this.pulseOffColor = offColor;
    this.pulseOnColor = onColor;
    Vector3 localPosition = knob.localPosition;
    this.PulseTarget = (double) Mathf.Abs(localPosition.y - on) < (double) Mathf.Abs(localPosition.y - off) ? 1f : 0.0f;
    localPosition.y = off;
    this.pulseOffWorld = knob.parent.TransformPoint(localPosition);
    localPosition.y = on;
    this.pulseOnWorld = knob.parent.TransformPoint(localPosition);
  }
  public void ApplyPulse(float progress, bool originalColors)
  {
    if (((Object) this.pulseKnob == (Object) null))
      return;
    this.pulseKnob.localPosition = Vector3.Lerp(this.pulseOff, this.pulseOn, progress);
    if (!originalColors)
      return;
    Color color = Color.Lerp(this.pulseOffColor, this.pulseOnColor, progress);
    if (this.pulseParts != null)
    {
      foreach (Renderer pulsePart in this.pulseParts)
      {
        if (((Object) pulsePart != (Object) null))
          pulsePart.sharedMaterial.color = color;
      }
    }
    if (!((Object) this.pulseRenderer != (Object) null))
      return;
    this.pulseRenderer.sharedMaterial.color = Color.Lerp(color, Color.white, 0.6f);
  }
}
