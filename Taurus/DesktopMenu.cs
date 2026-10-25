
using System;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class DesktopMenu : IDisposable
{
  private readonly GameObject root;
  private readonly GameObject menu;
  private readonly Camera camera;
  private readonly RenderTexture texture;
  private readonly AudioSource audio;
  private readonly BoxCollider[] buttons;
  private readonly MenuState state;
  private readonly Vector3 menuPosition;
  private readonly Action<string, AudioSource> press;
  private readonly Action transition;
  private float openedAt;
  private CursorLockMode previousLock;
  private bool previousCursor;
  private bool qHeld;
  private int lastRenderFrame = -1;
  private float closedAt;
  private float closingScale;
  public int RenderCount { get; private set; }
  public MenuView View { get; }
  public bool IsOpen { get; private set; }
  public bool IsClosing { get; private set; }
  public RenderTexture Texture => this.texture;
  public Func<Ray, Event, bool> BrowserInput { get; set; }
  public DesktopMenu(
    GameObject prefab,
    MenuState state,
    Action<string, AudioSource> press,
    Action transition = null)
  {
    this.state = state;
    this.press = press;
    this.transition = transition;
    this.root = new GameObject("ItzGameDesktop");
    this.root.transform.position = new Vector3(0.0f, -1000f, 0.0f);
    if (Application.isPlaying)
      Object.DontDestroyOnLoad((Object) this.root);
    this.menu = Object.Instantiate<GameObject>(prefab, this.root.transform, false);
    this.menuPosition = this.menu.transform.localPosition;
    foreach (Component componentsInChild in this.menu.GetComponentsInChildren<Transform>(true))
      componentsInChild.gameObject.layer = 31 ;
    this.View = new MenuView(this.menu.transform, state);
    this.audio = this.menu.GetComponent<AudioSource>();
    this.audio.spatialBlend = 0.0f;
    this.buttons = this.menu.GetComponentsInChildren<BoxCollider>(true);
    GameObject gameObject = new GameObject("DesktopCamera");
    gameObject.transform.SetParent(this.root.transform, false);
    this.camera = gameObject.AddComponent<Camera>();
    ((Behaviour) this.camera).enabled = false;
    ((Component) this.camera).transform.localPosition = new Vector3(0.7f, 0.0f, 0.035f);
    ((Component) this.camera).transform.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.forward);
    this.camera.orthographic = true;
    this.camera.orthographicSize = 0.345f;
    this.camera.nearClipPlane = 0.01f;
    this.camera.farClipPlane = 2f;
    this.camera.clearFlags = (CameraClearFlags) 2;
    this.camera.backgroundColor = Color.clear;
    this.camera.cullingMask = int.MinValue;
    MenuRender.Configure(this.camera);
    this.camera.useOcclusionCulling = false;
    int num = Mathf.CeilToInt((float) ((double) this.camera.orthographicSize * 2.0 * 4000.0));
    this.texture = new RenderTexture(Mathf.CeilToInt((float) ((double) num * 2.0 / 3.0)), num, 24)
    {
      antiAliasing = 4
    };
    this.camera.targetTexture = this.texture;
    this.camera.aspect = 0.6666667f;
    this.root.SetActive(false);
  }
  public void Toggle()
  {
    this.IsOpen = !this.IsOpen;
    Action transition = this.transition;
    if (transition != null)
      transition();
    if (this.IsOpen)
    {
      this.IsClosing = false;
      this.root.SetActive(true);
      this.previousLock = Cursor.lockState;
      this.previousCursor = Cursor.visible;
      Cursor.lockState = (CursorLockMode) 0;
      Cursor.visible = true;
      this.openedAt = Time.unscaledTime;
      this.lastRenderFrame = -1;
      this.View.Refresh();
    }
    else
    {
      this.IsClosing = true;
      this.closedAt = Time.unscaledTime;
      this.closingScale = this.menu.transform.localScale.x;
      Cursor.lockState = this.previousLock;
      Cursor.visible = this.previousCursor;
      this.View.SetHover((BoxCollider) null);
    }
  }
  public void Tick(float? elapsed = null)
  {
    if (!this.IsOpen && !this.IsClosing)
      return;
    float unscaledTime = Time.unscaledTime;
    float elapsed1 = (float) ((double?) elapsed ?? (double) unscaledTime - (this.IsClosing ? (double) this.closedAt : (double) this.openedAt));
    if (this.IsClosing && (double) elapsed1 >= 0.20000000298023224)
    {
      this.IsClosing = false;
      this.root.SetActive(false);
    }
    else
    {
      if (!elapsed.HasValue && this.lastRenderFrame == Time.frameCount)
        return;
      this.lastRenderFrame = Time.frameCount;
      this.menu.transform.localScale = (Vector3.one * (this.IsClosing ? this.closingScale * MenuStyle.CloseScale(elapsed1) : MenuStyle.OpenScale(elapsed1)));
      this.menu.transform.localPosition = (this.menuPosition + (this.menu.transform.localRotation * Vector3.Scale(this.View.DesignSlide(unscaledTime), this.menu.transform.localScale)));
      float num1 = this.state.Design != 10 || this.state.Category == 11 ? 0.6666667f : 1.02229655f;
      float num2 = this.state.Category == 11 || this.state.Design != 13 ? (this.state.Design != 10 || this.state.Category == 11 ? (this.state.Design != 7 || !this.state.KeyboardOpen ? 0.345f : 0.405f) : 0.141143993f) : 0.465f;
      int num3 = Mathf.CeilToInt((float) ((double) num2 * 2.0 * 4000.0));
      int num4 = Mathf.CeilToInt((float) num3 * num1);
      if (((UnityEngine.Texture) this.texture).width != num4 || ((UnityEngine.Texture) this.texture).height != num3)
      {
        this.texture.Release();
        ((UnityEngine.Texture) this.texture).width = num4;
        ((UnityEngine.Texture) this.texture).height = num3;
        this.texture.Create();
      }
      this.camera.aspect = num1;
      ((Component) this.camera).transform.localPosition = new Vector3(0.7f, 0.0f, this.state.Design == 10 || this.state.Design == 13 ? 0.0f : 0.035f);
      this.camera.orthographicSize = num2;
      this.View.Animate(unscaledTime);
      Physics.SyncTransforms();
      if (!MenuRender.TryRender(this.camera))
        return;
      ++this.RenderCount;
    }
  }
  public void OnGUI()
  {
    Event current = Event.current;
    if (current.type == (EventType) 5 && current.keyCode == (KeyCode) 113)
      this.qHeld = false;
    if (current.type == (EventType) 4 && current.keyCode == (KeyCode) 113 && (this.state.Design != 9 || !this.state.KeyboardOpen))
    {
      if (!this.qHeld)
        this.Toggle();
      this.qHeld = true;
      current.Use();
    }
    if (!this.IsOpen && !this.IsClosing)
      return;
    float num1 = Mathf.Min((float) Screen.height * 0.94f, 900f * this.state.PcScale);
    float num2 = num1 * (this.state.Design != 10 || this.state.Category == 11 ? 0.6666667f : 1.02229655f);
    Rect rect;
    rect = new Rect((float) (((double) Screen.width - (double) num2) / 2.0), (float) (((double) Screen.height - (double) num1) / 2.0), num2, num1);
    if (current.type == (EventType) 7)
      GUI.DrawTexture(rect, (UnityEngine.Texture) this.texture, (ScaleMode) 0, true);
    if (!this.IsOpen || (double) Time.unscaledTime - (double) this.openedAt < 0.31999999284744263)
      return;
    if (current.type == (EventType) 4 && this.state.KeyboardOpen)
    {
      string str = current.keyCode == (KeyCode) 13 || this.state.Design == 9 && current.keyCode == (KeyCode) 27 ? "Done" : (current.keyCode == (KeyCode) 8 ? "Back" : (current.keyCode == (KeyCode) 32 ? "Space" : (char.IsLetterOrDigit(current.character) ? current.character.ToString() : (string) null)));
      if (str != null)
      {
        this.press("Key_" + str, this.audio);
        current.Use();
      }
    }
    Vector3 vector3_1;
    vector3_1 = new Vector3((current.mousePosition.x - (rect).x) / (rect).width, (float) (1.0 - ((double) current.mousePosition.y - (double) (rect).y) / (double) (rect).height), 0.0f);
    if (this.BrowserInput != null && this.BrowserInput(this.camera.ViewportPointToRay(vector3_1), current))
    {
      this.View.SetHover((BoxCollider) null);
      if (!current.isMouse && current.type != (EventType) 6)
        return;
      current.Use();
    }
    else
    {
      BoxCollider button = (BoxCollider) null;
      if ((rect).Contains(current.mousePosition))
      {
        Vector3 vector3_2;
        vector3_2 = new Vector3((current.mousePosition.x - (rect).x) / (rect).width, (float) (1.0 - ((double) current.mousePosition.y - (double) (rect).y) / (double) (rect).height), 0.0f);
        button = this.HitTest(((Vector2)vector3_2));
      }
      this.View.SetHover(button);
      if (current.type != null || current.button != 0 || !((Object) button != (Object) null))
        return;
      this.press(((Object) button).name, this.audio);
      current.Use();
    }
  }
  public BoxCollider HitTest(Vector2 point)
  {
    Ray ray = this.camera.ViewportPointToRay(((Vector2)point));
    BoxCollider boxCollider = (BoxCollider) null;
    float num = float.PositiveInfinity;
    foreach (BoxCollider button in this.buttons)
    {
      RaycastHit raycastHit;
      if (((Collider) button).enabled && ((Component) button).gameObject.activeInHierarchy && ((Collider) button).Raycast(ray, out raycastHit, 2f) && (double) (raycastHit).distance < (double) num)
      {
        num = (raycastHit).distance;
        boxCollider = button;
      }
    }
    return boxCollider;
  }
  public void Dispose()
  {
    if (this.IsOpen)
      this.Toggle();
    this.View.Dispose();
    if (((Object) RenderTexture.active == (Object) this.texture))
      RenderTexture.active = (RenderTexture) null;
    this.texture.Release();
    if (Application.isPlaying)
    {
      Object.Destroy((Object) this.root);
      Object.Destroy((Object) this.texture);
    }
    else
    {
      Object.DestroyImmediate((Object) this.root);
      Object.DestroyImmediate((Object) this.texture);
    }
  }
}
