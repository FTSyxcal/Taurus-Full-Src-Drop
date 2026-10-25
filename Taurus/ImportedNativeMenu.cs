
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class ImportedNativeMenu : IDisposable
{
  private readonly Transform root;
  private readonly int design;
  private readonly Dictionary<Transform, Vector3> scales = new Dictionary<Transform, Vector3>();
  private readonly Dictionary<Transform, float> animated = new Dictionary<Transform, float>();
  private readonly Dictionary<string, float> pulses = new Dictionary<string, float>();
  private float pulseTime;
  private NativeUiSnapshot snapshot;
  private ImportedTheme theme;
  private ImportedPicker picker;
  private PortNotification notification;
  private MenuState state;
  private string signature;
  private bool active;
  private int rendered;
  private int lastAnimation = -1;
  public ImportedSurface Surface { get; private set; }
  public int RenderCount
  {
    get
    {
      int rendered = this.rendered;
      ImportedSurface surface = this.Surface;
      int renderCount = surface != null ? surface.RenderCount : 0;
      return rendered + renderCount;
    }
  }
  public ImportedNativeMenu(Transform root, int design)
  {
    this.root = root;
    this.design = design;
  }
  public void SetActive(bool value)
  {
    this.active = value;
    this.Surface?.SetActive(value);
  }
  public void Refresh(MenuState state)
  {
    if (!this.active)
      return;
    this.state = state;
    StringBuilder stringBuilder = new StringBuilder().Append(state.Category).Append('/').Append(state.Page).Append('/').Append(state.Query).Append('/').Append(state.KeyboardOpen).Append('/').Append(state.ColorPickerOpen).Append('/').Append(state.EditingEnd).Append('/').Append(state.Rounded).Append('/').Append(state.InRoom).Append('/').Append(state.PastCategoryPage);
    for (int row = 0; row < state.VisibleRows; ++row)
      stringBuilder.Append(((Component) this.root.Find($"Row{row.ToString()}/Label")).GetComponent<TextMesh>().text).Append(((Component) this.root.Find($"Row{row.ToString()}/State")).GetComponent<TextMesh>().text).Append(state.IsOn(state.EntryAt(row))).Append(state.IsFavorite(state.EntryAt(row)));
    string str = stringBuilder.ToString();
    if (this.signature != str)
    {
      this.signature = str;
      this.lastAnimation = -1;
      this.Release();
      this.snapshot = this.design == 5 ? new NativeIi().Build(state, this.root) : (this.design == 6 ? new NativeUntitled().Build(state, this.root) : (this.design == 8 ? new NativePast().Build(state, this.root) : (this.design == 9 ? new NativeRexon().Build(state, this.root) : new NativeNxo().Build(state, this.root))));
      Canvas.ForceUpdateCanvases();
      foreach (NativeUiClick componentsInChild in this.snapshot.Source.GetComponentsInChildren<NativeUiClick>(true))
      {
        componentsInChild.Rebase();
        if (componentsInChild.HasPulse)
        {
          float num;
          componentsInChild.ApplyPulse(this.pulses.TryGetValue(componentsInChild.Name, out num) ? num : componentsInChild.PulseTarget, state.Theme == "ItzGame");
        }
      }
      Transform transform1 = this.snapshot.Source.transform;
      transform1.localScale = (transform1.localScale * this.snapshot.PhysicalScale);
      Transform transform2;
      this.notification = new PortNotification(this.root, this.snapshot.Source.transform, ((Object) this.snapshot.NotificationTemplate != (Object) null) ? this.snapshot.NotificationTemplate : (this.snapshot.Controls.TryGetValue("Row0", out transform2) ? transform2 : this.snapshot.Controls["ButtonHome"]), ((Object) this.snapshot.HeaderLegacy != (Object) null) ? (Component) (object) this.snapshot.HeaderLegacy : (Component) (object) this.snapshot.Header, ImportedSurface.BoundsOf(((Object) this.snapshot.NotificationFrame != (Object) null) ? this.snapshot.NotificationFrame : this.snapshot.Source.transform), this.snapshot.Forward, this.snapshot.Up, true);
      Transform transform3;
      if (this.design == 6 && !state.KeyboardOpen && this.snapshot.Controls.TryGetValue("ButtonSearch", out transform3))
        transform3.SetParent(this.snapshot.Content.parent, true);
      Vector3 vector3_1 = Vector3.Cross(this.snapshot.Up, this.snapshot.Forward);
      Vector3 normalized = (vector3_1).normalized;
      Vector3 position = this.snapshot.Source.transform.position;
      Bounds? pickerRegion = this.snapshot.PickerRegion;
      this.Surface = new ImportedSurface(this.root, this.snapshot.Source, this.snapshot.Forward, this.snapshot.Up, true);
      this.theme = new ImportedTheme(this.snapshot.Source.transform, normalized, this.snapshot.Up);
      Bounds content;
      if (!pickerRegion.HasValue)
      {
        content = new Bounds((this.snapshot.Source.transform.position + (new Vector3(0.05f, 0.0f, 0.0f) * this.snapshot.PhysicalScale)), (new Vector3(0.02f, this.design == 6 ? 0.2752f : 0.23575f, this.design == 6 ? 0.3424f : 0.279f) * this.snapshot.PhysicalScale));
      }
      else
      {
        Bounds bounds = pickerRegion.Value;
        Vector3 vector3_2 = (((bounds).center + this.snapshot.Source.transform.position) - position);
        bounds = pickerRegion.Value;
        Vector3 size = (bounds).size;
        content = new Bounds(vector3_2, size);
      }
      this.picker = new ImportedPicker(this.root, this.snapshot.Source.transform, content, this.snapshot.Forward, normalized, this.snapshot.Up, new Action<Transform, Transform>(this.Surface.Map));
      this.scales.Clear();
      this.animated.Clear();
      foreach (Transform key in this.snapshot.Controls.Values)
        this.scales[key] = key.localScale;
      this.Surface.SetActive(true);
      this.Surface.Dirty();
    }
    this.Surface.HideOriginals(state);
    foreach (Renderer componentsInChild in ((Component) this.root.Find("Keyboard")).GetComponentsInChildren<Renderer>(true))
      componentsInChild.enabled = false;
    foreach (Transform transform in this.root.Find("Keyboard"))
    {
      if (((Object) ((Component) transform).GetComponent<BoxCollider>() != (Object) null) && !this.snapshot.Controls.ContainsKey("Keyboard/" + ((Object) transform).name))
        ((Component) transform).gameObject.SetActive(false);
    }
    ((Component) this.root.Find("DesignSettings")).gameObject.SetActive(false);
    string[] strArray = new string[5]
    {
      "ButtonSearch",
      "ButtonHome",
      "ButtonPrevious",
      "ButtonNext",
      "ButtonDisconnect"
    };
    foreach (string key in strArray)
    {
      Transform proxy = this.root.Find(key);
      Transform control;
      if (this.snapshot.Controls.TryGetValue(key, out control))
        this.Surface.Map(proxy, control);
      else
        ((Component) proxy).gameObject.SetActive(false);
    }
    foreach (KeyValuePair<string, Transform> control in this.snapshot.Controls)
    {
      Transform proxy = this.root.Find(control.Key);
      if (((Object) proxy != (Object) null))
      {
        if (control.Key.StartsWith("Keyboard/"))
          ((Component) proxy).gameObject.SetActive(state.KeyboardOpen);
        this.Surface.Map(proxy, control.Value);
      }
    }
    if (((Object) this.snapshot.Content != (Object) null))
      ((Component) this.snapshot.Content).gameObject.SetActive((!state.KeyboardOpen || this.design == 9) && !state.ColorPickerOpen);
    if (this.picker.Refresh(state))
      this.Surface.Dirty();
    MenuButtonEffects.BindRows(this.snapshot.Source.transform, state, (IEnumerable<KeyValuePair<string, Transform>>) this.snapshot.Controls);
    this.Tick();
  }
  public void Tick()
  {
    if (!this.active || this.Surface == null)
      return;
    if (this.notification.Refresh(this.state))
      this.Surface.Dirty();
    bool flag = this.design != 9 && this.theme.Apply(this.state, Time.unscaledTime);
    if (flag)
      this.Surface.Dirty();
    if (MenuFonts.Apply(this.Surface.Source.transform, this.state.FontIndex))
      this.Surface.Dirty();
    if (this.snapshot.Update != null && this.snapshot.Update(Time.unscaledTime))
      this.Surface.Dirty();
    int num1 = Mathf.FloorToInt(Time.unscaledTime * 30f);
    if (this.snapshot.Animate != null && this.lastAnimation != num1 | flag)
    {
      this.lastAnimation = num1;
      this.snapshot.Animate(Time.unscaledTime);
      this.Surface.Dirty();
    }
    foreach (Material pinwheel in this.snapshot.Pinwheels)
    {
      pinwheel.SetColor("_COLOR1", MenuPalette.Left(this.state, Time.unscaledTime));
      pinwheel.SetColor("_COLOR2", MenuPalette.Right(this.state, Time.unscaledTime));
      pinwheel.SetFloat("_Speed", this.state.IsOn("Freeze gradient") ? 0.0f : -0.5f);
    }
    foreach (KeyValuePair<string, Transform> control in this.snapshot.Controls)
    {
      Transform transform = this.root.Find(control.Key);
      if (!((Object) transform == (Object) null))
      {
        NativeUiClick nativeUiClick1;
        if (((Component) control.Value).TryGetComponent<NativeUiClick>(out nativeUiClick1) && nativeUiClick1.HasPulse)
        {
          float num2;
          double num3 = this.pulses.TryGetValue(nativeUiClick1.Name, out num2) ? (double) num2 : (double) nativeUiClick1.PulseTarget;
          float progress = Mathf.MoveTowards((float) num3, nativeUiClick1.PulseTarget, Mathf.Max(0.0f, Time.unscaledTime - this.pulseTime) / 0.15f);
          nativeUiClick1.ApplyPulse(progress, this.state.Theme == "ItzGame");
          this.pulses[nativeUiClick1.Name] = progress;
          if (!Mathf.Approximately((float) num3, progress))
            this.Surface.Dirty();
        }
        float x = transform.localScale.x;
        float num4;
        if (!this.animated.TryGetValue(control.Value, out num4) || !Mathf.Approximately(num4, x))
        {
          NativeUiClick nativeUiClick2;
          if (((Component) control.Value).TryGetComponent<NativeUiClick>(out nativeUiClick2))
          {
            nativeUiClick2.ApplyFactor(x);
          }
          else
          {
            Vector3 scale = this.scales[control.Value];
            control.Value.localScale = control.Value is RectTransform ? new Vector3(scale.x * x, scale.y * x, scale.z) : new Vector3(scale.x, scale.y * x, scale.z * x);
          }
          this.animated[control.Value] = x;
          this.Surface.Dirty();
        }
      }
    }
    this.pulseTime = Time.unscaledTime;
    this.Surface.Tick();
  }
  private void Release()
  {
    this.notification?.Dispose();
    this.notification = (PortNotification) null;
    if (this.Surface != null)
    {
      this.rendered += this.Surface.RenderCount;
      this.Surface.SetActive(false);
      this.Surface.Dispose();
      this.Surface = (ImportedSurface) null;
    }
    this.snapshot?.Dispose();
    this.snapshot = (NativeUiSnapshot) null;
  }
  public void Dispose() => this.Release();
}
