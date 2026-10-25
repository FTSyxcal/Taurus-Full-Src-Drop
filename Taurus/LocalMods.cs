
using GorillaLocomotion;
using HarmonyLib;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Object = UnityEngine.Object;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;
using Pointer = UnityEngine.InputSystem.Pointer;
namespace Taurus;
public sealed class LocalMods : IDisposable
{
  private readonly MenuState state;
  private readonly GameObject menuPrefab;
  private readonly MaterialPropertyBlock platformColors = new MaterialPropertyBlock();
  private readonly Font font;
  private readonly Material tagMaterial;
  private readonly Harmony harmony = new Harmony("local.gtag.menutemplate.crawl");
  private readonly List<GameObject> platforms = new List<GameObject>();
  private readonly List<GameObject> stickyPlatforms = new List<GameObject>();
  private readonly Dictionary<MeshCollider, bool> noclipColliders = new Dictionary<MeshCollider, bool>();
  private float noclipRefresh;
  private bool previousStickyLeftGrip;
  private bool previousStickyRightGrip;
  private readonly BroomGrabAssist broom;
  public Action<string, bool> Notify;
  private float iceOrig;
  private bool iceCaptured;
  private readonly Dictionary<VRRig, LineRenderer> beaconLines = new Dictionary<VRRig, LineRenderer>();
  private readonly Dictionary<VRRig, TextMesh> distTags = new Dictionary<VRRig, TextMesh>();
  private Material beaconMaterial;
  private readonly Dictionary<GameObject, bool> hiddenGroups = new Dictionary<GameObject, bool>();
  private readonly Dictionary<Renderer, bool> hiddenRenderers = new Dictionary<Renderer, bool>();
  private readonly Dictionary<Collider, bool> hiddenColliders = new Dictionary<Collider, bool>();
  private readonly Dictionary<Light, bool> hiddenLights = new Dictionary<Light, bool>();
  private readonly Dictionary<VRRig, TextMesh> tags = new Dictionary<VRRig, TextMesh>();
  private readonly Dictionary<VRRig, LineRenderer> tracers = new Dictionary<VRRig, LineRenderer>();
  private Material tracerMaterial;
  private GTPlayer speedPlayer;
  private float originalSpeed;
  private float originalMultiplier;
  private Vector3 stationaryPosition;
  private bool stationaryCaptured;
  private bool mouseLookCaptured;
  private Vector2 mouseLookStart;
  private Vector3 mouseLookRotation;
  private Camera firstPerson;
  private Behaviour virtualCamera;
  private bool cameraWasEnabled;
  private Vector3 cameraPosition;
  private Quaternion cameraRotation;
  private float cameraFov;
  private float nextSweep;
  private float nextTagRefresh;
  private Vector3? originalClickPosition;
  private Behaviour clickFollower;
  private bool clickFollowerEnabled;
  public static bool CrawlEnabled;
  private readonly CosmeticPreview cosmetics = new CosmeticPreview();
  private readonly PokrukCameraAdapter pokruk = new PokrukCameraAdapter();
  private readonly DetectedPorts detected;
  private readonly CustomBoards boards;
  private GameObject leftPlat;
  private GameObject rightPlat;
  public event Action<GameObject, Vector3, Vector3, float> SharedProjectile;
  public LocalMods(MenuState state, GameObject menuPrefab, Font font)
  {
    this.state = state;
    this.menuPrefab = menuPrefab;
    this.font = font;
    this.boards = new CustomBoards(state, ((Component) menuPrefab.transform.Find("Background")).GetComponent<Renderer>().sharedMaterial);
    this.tagMaterial = new Material(((Component) menuPrefab.transform.Find("Row0/Label")).GetComponent<Renderer>().sharedMaterial);
    this.tagMaterial.mainTexture = font.material.mainTexture;
    LocalMods.CrawlEnabled = state.IsOn("Spider Body");
    this.harmony.Patch((MethodBase) AccessTools.Method(typeof (VRRig), "PostTick", (Type[]) null, (Type[]) null), (HarmonyMethod) null, new HarmonyMethod(typeof (LocalMods), "CrawlPostfix", (Type[]) null), (HarmonyMethod) null, (HarmonyMethod) null, (HarmonyMethod) null);
    string[] strArray = new string[28]
    {
      "Platforms",
      "Spider Body",
      "Fly",
      "Trigger Fly",
      "Joystick Fly",
      "Speed Boost",
      "Low Gravity",
      "Zero Gravity",
      "High Gravity",
      "WASD Fly",
      "Disable Stationary WASD Fly",
      "NoClip",
      "Long Arms",
      "Sticky Platforms",
      "Wall Walk",
      "Up Down",
      "Iron Monke",
      "Ice Control",
      "First Person Camera",
      "PC Button Click",
      "Alien Stuff Remover (No Colliders)",
      "Alien Stuff Remover (With Colliders)",
      "Mod Nametags",
      "CosmeticX (CS)",
      "Pokruk CameraMod",
      "Tracers",
      "Beacons",
      "Distance Tags"
    };
    foreach (string name in strArray)
      state.RegisterHandler(name, new Action<MenuEntry>(this.Changed));
    this.detected = new DetectedPorts(state);
    this.detected.SharedProjectile += (Action<GameObject, Vector3, Vector3, float>) ((prefab, position, velocity, size) =>
    {
      Action<GameObject, Vector3, Vector3, float> sharedProjectile = this.SharedProjectile;
      if (sharedProjectile == null)
        return;
      sharedProjectile(prefab, position, velocity, size);
    });
    foreach (string name in DetectedPorts.Names)
    {
      if (MenuCatalog.Find(name) >= 0)
      {
        state.RegisterHandler(name, new Action<MenuEntry>(this.detected.Changed));
        if (MenuState.IsFeature(MenuCatalog.Buttons[MenuCatalog.Find(name)]))
          state.SetTestStatus(name, "W");
      }
    }
    this.broom = new BroomGrabAssist(new Action<bool>(this.BroomNoClip), (Action<string, bool>) ((message, error) =>
    {
      Action<string, bool> notify = this.Notify;
      if (notify == null)
        return;
      notify(message, error);
    }));
    state.RegisterHandler("Grab Broom", (Action<MenuEntry>) (entry => this.broom.Reset(LocalMods.RightGrip())));
  }
  public void Changed(MenuEntry entry)
  {
    if (entry == null)
      return;
    string name1 = entry.Name;
    bool flag = this.state.IsOn(name1);
    switch (name1)
    {
      case "CosmeticX (CS)":
        if (flag)
        {
          this.cosmetics.Enable();
          break;
        }
        this.cosmetics.Dispose();
        break;
      case "Pokruk CameraMod":
        if (flag)
        {
          this.state.SetEnabled("First Person Camera", false);
          this.RestoreCamera();
          this.pokruk.Enable();
          break;
        }
        this.pokruk.Dispose();
        break;
      case "Spider Body":
        LocalMods.CrawlEnabled = flag;
        break;
      default:
        if (name1 == "Platforms" && !flag)
        {
          this.ClearPlatforms();
          break;
        }
        if (name1 == "Sticky Platforms" && !flag)
        {
          this.ClearStickyPlatforms();
          break;
        }
        if (name1 == "NoClip" && !flag)
        {
          this.BroomNoClip(this.broom.Active);
          break;
        }
        switch (name1)
        {
          case "Ice Control":
            if (!flag)
            {
              this.RestoreIce();
              break;
            }
            break;
          case "Long Arms":
            try
            {
              if (((Object) GTPlayer.Instance != (Object) null))
              {
                ((Component) GTPlayer.Instance).transform.localScale = flag ? new Vector3(1.2f, 1.2f, 1.2f) : Vector3.one;
                break;
              }
              break;
            }
            catch
            {
              break;
            }
          default:
            if (name1 == "Speed Boost" && !flag)
            {
              this.RestoreSpeed();
              break;
            }
            if (name1 == "WASD Fly" || name1 == "Disable Stationary WASD Fly")
            {
              this.stationaryCaptured = false;
              this.ResetMouseLook();
              break;
            }
            if (name1 == "First Person Camera")
            {
              if (flag)
              {
                this.state.SetEnabled("Pokruk CameraMod", false);
                this.pokruk.Dispose();
                if (!this.CaptureCamera())
                  throw new InvalidOperationException("Shoulder camera is not ready.");
                break;
              }
              this.RestoreCamera();
              break;
            }
            if (name1 == "PC Button Click" && !flag)
            {
              this.RestoreClick();
              break;
            }
            if (name1 == "Mod Nametags" && !flag)
            {
              this.ClearTags();
              break;
            }
            if (name1 == "Beacons" && !flag)
            {
              this.ClearBeacons();
              break;
            }
            if (name1 == "Distance Tags" && !flag)
            {
              this.ClearDistTags();
              break;
            }
            if (name1 == "Tracers" && !flag)
            {
              this.ClearTracers();
              break;
            }
            if (name1.StartsWith("Alien Stuff Remover"))
            {
              this.RestoreAliens();
              if (flag)
              {
                this.state.SetEnabled(name1.Contains("(No Colliders)") ? "Alien Stuff Remover (With Colliders)" : "Alien Stuff Remover (No Colliders)", false);
                this.SweepAliens();
                break;
              }
              break;
            }
            break;
        }
        break;
    }
    if (!flag || !(name1 == "Low Gravity") && !(name1 == "Zero Gravity") && !(name1 == "High Gravity"))
      return;
    string[] strArray = new string[3]
    {
      "Low Gravity",
      "Zero Gravity",
      "High Gravity"
    };
    foreach (string name2 in strArray)
    {
      if (name2 != name1)
        this.state.SetEnabled(name2, false);
    }
  }
  public void FixedTick(bool suppressInput)
  {
    GorillaTagger instance1 = GorillaTagger.Instance;
    if (((Object) instance1 == (Object) null) || ((Object) instance1.rigidbody == (Object) null))
      return;
    Rigidbody rigidbody1 = instance1.rigidbody;
    if (this.state.IsOn("Low Gravity"))
      rigidbody1.AddForce((Vector3.up * 6.66f), (ForceMode) 5);
    if (this.state.IsOn("Zero Gravity"))
      rigidbody1.AddForce((-Physics.gravity), (ForceMode) 5);
    if (this.state.IsOn("High Gravity"))
      rigidbody1.AddForce((Vector3.down * 7.77f), (ForceMode) 5);
    GTPlayer instance2 = GTPlayer.Instance;
    if (this.state.IsOn("Speed Boost") && ((Object) instance2 != (Object) null))
    {
      if (((Object) this.speedPlayer != (Object) instance2))
      {
        this.RestoreSpeed();
        this.speedPlayer = instance2;
        this.originalSpeed = instance2.maxJumpSpeed;
        this.originalMultiplier = instance2.jumpMultiplier;
      }
      instance2.maxJumpSpeed = 9f;
      instance2.jumpMultiplier = 1.6f;
    }
    if (this.state.IsOn("Ice Control") && ((Object) GTPlayer.Instance != (Object) null))
    {
      if (!this.iceCaptured)
      {
        this.iceOrig = GTPlayer.Instance.slideControl;
        this.iceCaptured = true;
      }
      GTPlayer.Instance.slideControl = 1f;
    }
    if (suppressInput || ((Object) instance1.headCollider == (Object) null))
      return;
    if (this.state.IsOn("Wall Walk"))
      this.WallWalk(instance1);
    if (this.state.IsOn("Up Down") || this.state.IsOn("Iron Monke"))
      this.GripMoves(instance1);
    InputDevice deviceAtXrNode1 = InputDevices.GetDeviceAtXRNode((XRNode) 5);
    InputDevice deviceAtXrNode2 = InputDevices.GetDeviceAtXRNode((XRNode) 4);
    bool controllerPrimaryButton;
    (deviceAtXrNode1).TryGetFeatureValue(CommonUsages.primaryButton, out controllerPrimaryButton);
    bool controllerTriggerButton;
    (deviceAtXrNode1).TryGetFeatureValue(CommonUsages.triggerButton, out controllerTriggerButton);
    Vector2 controllerPrimary2Daxis;
    (deviceAtXrNode2).TryGetFeatureValue(CommonUsages.primary2DAxis, out controllerPrimary2Daxis);
    ControllerInputPoller instance3 = ControllerInputPoller.instance;
    if (((Object) instance3 != (Object) null))
    {
      controllerPrimaryButton = instance3.rightControllerPrimaryButton;
      controllerTriggerButton = instance3.rightControllerTriggerButton;
      controllerPrimary2Daxis = instance3.leftControllerPrimary2DAxis;
    }
    Transform transform = ((Component) instance1.headCollider).transform;
    if (this.state.IsOn("Fly") & controllerPrimaryButton || this.state.IsOn("Trigger Fly") & controllerTriggerButton)
      rigidbody1.linearVelocity = (transform.forward * this.state.FlySpeed);
    if (!this.state.IsOn("Joystick Fly") || (double) (controllerPrimary2Daxis).sqrMagnitude <= 0.090000003576278687)
      return;
    Rigidbody rigidbody2 = rigidbody1;
    Vector3 vector3_1 = ((transform.forward * controllerPrimary2Daxis.y) + (transform.right * controllerPrimary2Daxis.x));
    Vector3 vector3_2 = ((vector3_1).normalized * this.state.FlySpeed);
    rigidbody2.linearVelocity = vector3_2;
  }
  public void Tick(bool suppressInput)
  {
    this.boards.Tick();
    GorillaTagger instance = GorillaTagger.Instance;
    if (((Object) instance == (Object) null))
    {
      this.broom.Reset(LocalMods.RightGrip());
    }
    else
    {
      try
      {
        if (this.state.IsOn("CosmeticX (CS)"))
          this.cosmetics.Tick();
        if (this.state.IsOn("Pokruk CameraMod"))
        {
          if (!this.pokruk.Enabled)
          {
            if (((Object) GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera") != (Object) null))
              this.pokruk.Enable();
          }
        }
      }
      catch
      {
        this.state.SetEnabled("CosmeticX (CS)", false);
        this.state.SetEnabled("Pokruk CameraMod", false);
        this.cosmetics.Dispose();
        this.pokruk.Dispose();
        this.broom.Dispose();
        throw;
      }
      if (this.state.IsOn("Platforms") && !suppressInput)
        this.Platforms(instance);
      this.broom.Tick(this.state.IsOn("Grab Broom"), LocalMods.RightGrip(), suppressInput);
      if (this.state.IsOn("Sticky Platforms") && !suppressInput)
        this.StickyPlatforms(instance);
      if (this.state.IsOn("NoClip") || this.broom.Active)
        this.NoClip(this.broom.Active);
      foreach (GameObject platform in this.platforms)
      {
        if (((Object) platform != (Object) null))
          MenuPlatform.Paint(platform, this.state, Time.unscaledTime, this.platformColors);
      }
      if (this.state.IsOn("WASD Fly") && !suppressInput && !this.state.KeyboardOpen && Application.isFocused)
      {
        this.Wasd(instance);
      }
      else
      {
        this.stationaryCaptured = false;
        this.ResetMouseLook();
      }
      if (this.state.IsOn("First Person Camera"))
        this.FirstPerson(instance, suppressInput);
      if (this.state.IsOn("PC Button Click") && !suppressInput)
        this.ClickWorld(instance);
      else
        this.RestoreClick();
      if ((this.state.IsOn("Alien Stuff Remover (No Colliders)") || this.state.IsOn("Alien Stuff Remover (With Colliders)")) && (double) Time.unscaledTime >= (double) this.nextSweep)
        this.SweepAliens();
      if (this.state.IsOn("Mod Nametags"))
        this.Nametags(instance);
      if (this.state.IsOn("Tracers"))
        this.Tracers(instance);
      else
        this.ClearTracers();
      if (this.state.IsOn("Beacons"))
        this.Beacons(instance);
      else
        this.ClearBeacons();
      if (this.state.IsOn("Distance Tags"))
        this.DistanceTags(instance);
      else
        this.ClearDistTags();
      try
      {
        this.detected.Tick();
      }
      catch
      {
      }
    }
  }
  public void OnGunBody(Transform body)
  {
    try
    {
      this.detected.OnGunBody(body);
    }
    catch
    {
    }
  }
  public void OnGunPoint(Vector3 point)
  {
    try
    {
      this.detected.OnGunPoint(point);
    }
    catch
    {
    }
  }
  public static void Teleport(Vector3 position)
  {
    if (!((Object) GTPlayer.Instance != (Object) null))
      return;
    GTPlayer.Instance.TeleportTo(position, ((Component) GTPlayer.Instance).transform.rotation, false, false);
  }
  private static bool RightGrip()
  {
    InputDevice deviceAtXrNode = InputDevices.GetDeviceAtXRNode((XRNode) 5);
    bool flag;
    (deviceAtXrNode).TryGetFeatureValue(CommonUsages.gripButton, out flag);
    ControllerInputPoller instance = ControllerInputPoller.instance;
    return !((Object) instance != (Object) null) ? flag : (double) instance.rightControllerGripFloat > 0.5;
  }
  private void BroomNoClip(bool active)
  {
    if (active || this.state.IsOn("NoClip"))
      this.NoClip(active);
    else
      this.RestoreColliders();
  }
  public void SharedPlatforms(List<Transform> result)
  {
    result.Clear();
    if (((Object) this.leftPlat != (Object) null))
      result.Add(this.leftPlat.transform);
    if (((Object) this.rightPlat != (Object) null))
      result.Add(this.rightPlat.transform);
    foreach (GameObject stickyPlatform in this.stickyPlatforms)
    {
      if (((Object) stickyPlatform != (Object) null))
        result.Add(stickyPlatform.transform);
    }
  }
  private void Platforms(GorillaTagger tagger)
  {
    InputDevice deviceAtXrNode1 = InputDevices.GetDeviceAtXRNode((XRNode) 4);
    bool held1;
    (deviceAtXrNode1).TryGetFeatureValue(CommonUsages.gripButton, out held1);
    InputDevice deviceAtXrNode2 = InputDevices.GetDeviceAtXRNode((XRNode) 5);
    bool held2;
    (deviceAtXrNode2).TryGetFeatureValue(CommonUsages.gripButton, out held2);
    ControllerInputPoller instance = ControllerInputPoller.instance;
    if (((Object) instance != (Object) null))
    {
      held1 = (double) instance.leftControllerGripFloat > 0.5;
      held2 = (double) instance.rightControllerGripFloat > 0.5;
    }
    this.ProcessPlatform(tagger.leftHandTransform, held1, ref this.leftPlat);
    this.ProcessPlatform(tagger.rightHandTransform, held2, ref this.rightPlat);
  }
  private void ProcessPlatform(Transform hand, bool held, ref GameObject slot)
  {
    if (held)
    {
      if (!((Object) slot == (Object) null) || !((Object) hand != (Object) null))
        return;
      slot = this.SpawnPlatform(hand);
    }
    else
    {
      if (!((Object) slot != (Object) null))
        return;
      Object.Destroy((Object) slot);
      slot = (GameObject) null;
    }
  }
  private GameObject SpawnPlatform(Transform hand)
  {
    if (((Object) hand == (Object) null))
      return (GameObject) null;
    GameObject platform = MenuPlatform.Create(this.menuPrefab, (hand.position + (Vector3.down * 0.05f)), hand.rotation);
    MenuPlatform.Paint(platform, this.state, Time.unscaledTime, this.platformColors);
    LocalMods.AddStickyPlates(platform);
    this.platforms.Add(platform);
    if (this.platforms.Count > 72)
    {
      Object.Destroy((Object) this.platforms[0]);
      this.platforms.RemoveAt(0);
    }
    return platform;
  }
  private static void AddStickyPlates(GameObject platform)
  {
    try
    {
      Vector3 vector3_1;
      vector3_1 = new Vector3(MenuPlatform.Size.x / 2f, 0.0f, 0.0f);
      Vector3 vector3_2;
      vector3_2 = new Vector3(0.0f, MenuPlatform.Size.y / 2f, 0.0f);
      Vector3 vector3_3;
      vector3_3 = new Vector3(0.0f, 0.0f, MenuPlatform.Size.z / 2f);
      Vector3[] vector3Array1 = new Vector3[6]
      {
        vector3_2,
        (-vector3_2),
        vector3_1,
        (-vector3_1),
        vector3_3,
        (-vector3_3)
      };
      Vector3[] vector3Array2 = new Vector3[6]
      {
        new Vector3(0.03f, 0.01f, MenuPlatform.Size.z * 0.9f),
        new Vector3(0.03f, 0.01f, MenuPlatform.Size.z * 0.9f),
        new Vector3(0.01f, MenuPlatform.Size.y * 0.9f, MenuPlatform.Size.z * 0.9f),
        new Vector3(0.01f, MenuPlatform.Size.y * 0.9f, MenuPlatform.Size.z * 0.9f),
        new Vector3(0.03f, MenuPlatform.Size.y * 0.9f, 0.01f),
        new Vector3(0.03f, MenuPlatform.Size.y * 0.9f, 0.01f)
      };
      Vector3 lossyScale = platform.transform.lossyScale;
      for (int index = 0; index < 6; ++index)
      {
        GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
        primitive.transform.SetParent(platform.transform, false);
        primitive.transform.localPosition = new Vector3(vector3Array1[index].x / lossyScale.x, vector3Array1[index].y / lossyScale.y, vector3Array1[index].z / lossyScale.z);
        primitive.transform.localRotation = Quaternion.identity;
        primitive.transform.localScale = new Vector3(vector3Array2[index].x / lossyScale.x, vector3Array2[index].y / lossyScale.y, vector3Array2[index].z / lossyScale.z);
        primitive.GetComponent<Renderer>().enabled = false;
      }
    }
    catch
    {
    }
  }
  private void Wasd(GorillaTagger tagger)
  {
    if (((Object) tagger.rigidbody == (Object) null) || ((Object) tagger.headCollider == (Object) null) || Keyboard.current == null)
    {
      this.stationaryCaptured = false;
    }
    else
    {
      Transform transform = this.MouseLook(tagger);
      Vector3 vector3 = Vector3.zero;
      if (LocalMods.KeyHeld((Key) 37))
        vector3 = (vector3 + transform.forward);
      if (LocalMods.KeyHeld((Key) 33))
        vector3 = (vector3 - transform.forward);
      if (LocalMods.KeyHeld((Key) 18))
        vector3 = (vector3 + transform.right);
      if (LocalMods.KeyHeld((Key) 15))
        vector3 = (vector3 - transform.right);
      if (LocalMods.KeyHeld((Key) 1))
        vector3 = (vector3 + Vector3.up);
      if (LocalMods.KeyHeld((Key) 55))
        vector3 = (vector3 - Vector3.up);
      float num = LocalMods.KeyHeld((Key) 51) ? 16f : (LocalMods.KeyHeld((Key) 53) ? 4f : 8f);
      if ((double) (vector3).sqrMagnitude > 0.0)
      {
        tagger.rigidbody.linearVelocity = Vector3.zero;
        this.stationaryPosition = (((Component) tagger.rigidbody).transform.position + (((vector3).normalized * num) * Time.deltaTime));
        tagger.rigidbody.position = this.stationaryPosition;
        ((Component) tagger.rigidbody).transform.position = this.stationaryPosition;
        this.stationaryCaptured = true;
      }
      else
      {
        if (this.state.IsOn("Disable Stationary WASD Fly"))
          return;
        if (!this.stationaryCaptured)
        {
          this.stationaryPosition = ((Component) tagger.rigidbody).transform.position;
          this.stationaryCaptured = true;
        }
        tagger.rigidbody.linearVelocity = Vector3.zero;
        tagger.rigidbody.position = this.stationaryPosition;
        ((Component) tagger.rigidbody).transform.position = this.stationaryPosition;
      }
    }
  }
  private static bool KeyHeld(Key key)
  {
    return Keyboard.current != null && ((ButtonControl) Keyboard.current[key]).isPressed;
  }
  private Transform MouseLook(GorillaTagger tagger)
  {
    Transform transform1 = ((Component) tagger.headCollider).transform;
    Mouse current = Mouse.current;
    GTPlayer instance = GTPlayer.Instance;
    Transform transform2 = (Transform) null;
    if (((Object) instance != (Object) null))
    {
      Transform controllerTransform = instance.GetControllerTransform(false);
      transform2 = ((Object) controllerTransform == (Object) null) ? (Transform) null : controllerTransform.parent;
    }
    if (((Object) transform2 == (Object) null))
      transform2 = transform1;
    if (current == null || !current.rightButton.isPressed || !Application.isFocused || Screen.width <= 0 || Screen.height <= 0)
    {
      this.ResetMouseLook();
      return transform2;
    }
    Vector2 vector2 = ((InputControl<Vector2>) ((Pointer) current).position).ReadValue();
    if (!this.mouseLookCaptured)
    {
      this.mouseLookCaptured = true;
      this.mouseLookStart = vector2;
      Quaternion rotation = transform2.rotation;
      this.mouseLookRotation = (rotation).eulerAngles;
    }
    float num1 = this.mouseLookRotation.x - (float) (((double) vector2.y - (double) this.mouseLookStart.y) / (double) Screen.height * 360.0 * 1.3300000429153442);
    float num2 = this.mouseLookRotation.y + (float) (((double) vector2.x - (double) this.mouseLookStart.x) / (double) Screen.width * 360.0 * 1.3300000429153442);
    if ((double) num1 > 180.0)
      num1 -= 360f;
    transform2.rotation = Quaternion.Euler(Mathf.Clamp(num1, -90f, 90f), num2, this.mouseLookRotation.z);
    return transform2;
  }
  private void ResetMouseLook()
  {
    this.mouseLookCaptured = false;
    this.mouseLookStart = Vector2.zero;
    this.mouseLookRotation = Vector3.zero;
  }
  private bool CaptureCamera()
  {
    if (((Object) this.firstPerson != (Object) null))
      return true;
    GameObject gameObject = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
    if (((Object) gameObject == (Object) null) || ((Object) (this.firstPerson = gameObject.GetComponent<Camera>()) == (Object) null))
      return false;
    this.cameraPosition = ((Component) this.firstPerson).transform.localPosition;
    this.cameraRotation = ((Component) this.firstPerson).transform.localRotation;
    this.cameraFov = this.firstPerson.fieldOfView;
    Transform transform = gameObject.transform.Find("CM vcam1");
    this.virtualCamera = ((Object) transform == (Object) null) ? (Behaviour) null : ((Component) transform).GetComponent("CinemachineVirtualCamera") as Behaviour;
    if (((Object) this.virtualCamera != (Object) null))
    {
      this.cameraWasEnabled = this.virtualCamera.enabled;
      this.virtualCamera.enabled = false;
    }
    return true;
  }
  private void FirstPerson(GorillaTagger tagger, bool suppressInput)
  {
    if (((Object) this.firstPerson == (Object) null) && !this.CaptureCamera() || ((Object) tagger.headCollider == (Object) null))
      return;
    ((Component) this.firstPerson).transform.SetPositionAndRotation(((Component) tagger.headCollider).transform.position, ((Component) tagger.headCollider).transform.rotation);
    Camera firstPerson = this.firstPerson;
    double num1;
    if (!LocalMods.KeyHeld((Key) 17) || suppressInput)
    {
      num1 = 90.0;
    }
    else
    {
      double fieldOfView = (double) this.firstPerson.fieldOfView;
      Mouse current = Mouse.current;
      double num2 = (current != null ? (double) ((InputControl<Vector2>) current.scroll).ReadValue().y : 0.0) * 0.05000000074505806;
      num1 = (double) Mathf.Clamp((float) (fieldOfView - num2), 10f, 90f);
    }
    firstPerson.fieldOfView = (float) num1;
  }
  private void RestoreCamera()
  {
    if (((Object) this.firstPerson != (Object) null))
    {
      ((Component) this.firstPerson).transform.localPosition = this.cameraPosition;
      ((Component) this.firstPerson).transform.localRotation = this.cameraRotation;
      this.firstPerson.fieldOfView = this.cameraFov;
    }
    if (((Object) this.virtualCamera != (Object) null))
      this.virtualCamera.enabled = this.cameraWasEnabled;
    this.firstPerson = (Camera) null;
    this.virtualCamera = (Behaviour) null;
  }
  private void ClickWorld(GorillaTagger tagger)
  {
    GameObject handTriggerCollider = tagger.rightHandTriggerCollider;
    Camera main = Camera.main;
    if (((Object) handTriggerCollider == (Object) null) || ((Object) main == (Object) null) || Mouse.current == null || !Mouse.current.leftButton.isPressed)
    {
      this.RestoreClick();
    }
    else
    {
      RaycastHit raycastHit;
      if (!Physics.Raycast(main.ScreenPointToRay(((Vector2)((InputControl<Vector2>) ((Pointer) Mouse.current).position).ReadValue())), out raycastHit, 100f, 2147483643, (QueryTriggerInteraction) 2))
        return;
      if (!this.originalClickPosition.HasValue)
      {
        this.originalClickPosition = new Vector3?(handTriggerCollider.transform.localPosition);
        this.clickFollower = handTriggerCollider.GetComponent("TransformFollow") as Behaviour;
        if (((Object) this.clickFollower != (Object) null))
        {
          this.clickFollowerEnabled = this.clickFollower.enabled;
          this.clickFollower.enabled = false;
        }
      }
      handTriggerCollider.transform.position = (raycastHit).point;
    }
  }
  private void RestoreClick()
  {
    if (this.originalClickPosition.HasValue && ((Object) GorillaTagger.Instance != (Object) null) && ((Object) GorillaTagger.Instance.rightHandTriggerCollider != (Object) null))
      GorillaTagger.Instance.rightHandTriggerCollider.transform.localPosition = this.originalClickPosition.Value;
    if (((Object) this.clickFollower != (Object) null))
      this.clickFollower.enabled = this.clickFollowerEnabled;
    this.originalClickPosition = new Vector3?();
    this.clickFollower = (Behaviour) null;
  }
  private void SweepAliens()
  {
    this.nextSweep = Time.unscaledTime + 5f;
    for (int index = 0; index < SceneManager.sceneCount; ++index)
    {
      Scene sceneAt = SceneManager.GetSceneAt(index);
      if ((sceneAt).isLoaded)
      {
        foreach (GameObject rootGameObject in (sceneAt).GetRootGameObjects())
        {
          foreach (Transform componentsInChild1 in rootGameObject.GetComponentsInChildren<Transform>(true))
          {
            if (LocalMods.IsAlienTarget(componentsInChild1))
            {
              if (this.state.IsOn("Alien Stuff Remover (No Colliders)"))
              {
                if (!this.hiddenGroups.ContainsKey(((Component) componentsInChild1).gameObject))
                  this.hiddenGroups.Add(((Component) componentsInChild1).gameObject, ((Component) componentsInChild1).gameObject.activeSelf);
                ((Component) componentsInChild1).gameObject.SetActive(false);
              }
              else
              {
                foreach (Renderer componentsInChild2 in ((Component) componentsInChild1).GetComponentsInChildren<Renderer>(true))
                {
                  if (!this.hiddenRenderers.ContainsKey(componentsInChild2))
                    this.hiddenRenderers.Add(componentsInChild2, componentsInChild2.enabled);
                  componentsInChild2.enabled = false;
                }
                foreach (Collider componentsInChild3 in ((Component) componentsInChild1).GetComponentsInChildren<Collider>(true))
                {
                  if (!this.hiddenColliders.ContainsKey(componentsInChild3))
                    this.hiddenColliders.Add(componentsInChild3, componentsInChild3.enabled);
                  componentsInChild3.enabled = false;
                }
                foreach (Light componentsInChild4 in ((Component) componentsInChild1).GetComponentsInChildren<Light>(true))
                {
                  if (!this.hiddenLights.ContainsKey(componentsInChild4))
                    this.hiddenLights.Add(componentsInChild4, ((Behaviour) componentsInChild4).enabled);
                  ((Behaviour) componentsInChild4).enabled = false;
                }
              }
            }
          }
        }
      }
    }
  }
  private static bool IsAlienTarget(Transform node)
  {
    if (((Object) node).name != "Terraformer" && ((Object) node).name != "Terraformers" && ((Object) node).name != "Event_Overview_AlienArrival")
      return false;
    for (Transform parent = node.parent; ((Object) parent != (Object) null); parent = parent.parent)
    {
      if ((((Object) node).name == "Event_Overview_AlienArrival" ? (((Object) parent).name == "City_Pretty" ? 1 : 0) : (((Object) parent).name == "2026_Halloween_Forest" || ((Object) parent).name == "2026_Halloween_Canyon" || ((Object) parent).name == "2026_Halloween_Mountain" || ((Object) parent).name == "2025_Halloween1_Skyjungle" ? 1 : (((Object) parent).name == "2026_Halloween1_Metro" ? 1 : 0))) != 0)
        return true;
    }
    return false;
  }
  private void Nametags(GorillaTagger tagger)
  {
    if (((Object) tagger.headCollider == (Object) null))
      return;
    bool flag = (double) Time.unscaledTime >= (double) this.nextTagRefresh;
    if (flag)
      this.nextTagRefresh = Time.unscaledTime + 0.75f;
    foreach (TextMesh textMesh in this.tags.Values)
    {
      if (((Object) textMesh != (Object) null))
        ((Component) textMesh).gameObject.SetActive(false);
    }
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((Object) activeRig == (Object) null) && !activeRig.isOfflineVRRig && activeRig.head != null && !((Object) activeRig.head.rigTarget == (Object) null) && activeRig.Creator != null)
      {
        TextMesh textMesh;
        if (!this.tags.TryGetValue(activeRig, out textMesh) || ((Object) textMesh == (Object) null))
        {
          textMesh = new GameObject("ItzGameModTag").AddComponent<TextMesh>();
          textMesh.font = this.font;
          textMesh.fontSize = 0;
          textMesh.characterSize = 0.1728f / (float) this.font.fontSize;
          textMesh.anchor = (TextAnchor) 4;
          textMesh.alignment = (TextAlignment) 1;
          textMesh.richText = false;
          ((Component) textMesh).GetComponent<Renderer>().sharedMaterial = this.tagMaterial;
          this.tags[activeRig] = textMesh;
        }
        if (flag || textMesh.text.Length == 0)
        {
          Player playerRef = activeRig.Creator.GetPlayerRef();
          List<string> values = new List<string>();
          if (playerRef != null && playerRef.CustomProperties != null)
          {
            foreach (object key in ((Dictionary<object, object>) playerRef.CustomProperties).Keys)
            {
              string str = key?.ToString();
              if (!string.IsNullOrEmpty(str) && !(str == "didTutorial") && !(str == "255"))
              {
                if (values.Count != 6)
                  values.Add(str.Length > 24 ? str.Substring(0, 24) : str);
                else
                  break;
              }
            }
          }
          textMesh.text = activeRig.Creator.SanitizedNickName + (values.Count > 0 ? "\n" + string.Join(", ", (IEnumerable<string>) values) : "");
        }
        ((Component) textMesh).gameObject.SetActive(true);
        ((Component) textMesh).transform.position = (activeRig.head.rigTarget.position + (Vector3.up * 0.35f));
        ((Component) textMesh).transform.rotation = Quaternion.LookRotation((((Component) textMesh).transform.position - ((Component) tagger.headCollider).transform.position));
      }
    }
  }
  private static void CrawlPostfix(VRRig __instance)
  {
    if (!LocalMods.CrawlEnabled || !__instance.isLocal || ((Object) GorillaTagger.Instance == (Object) null) || ((Object) GorillaTagger.Instance.headCollider == (Object) null))
      return;
    ((Component) __instance).transform.rotation = Quaternion.Euler(87f, ((Component) GorillaTagger.Instance.headCollider).transform.eulerAngles.y, 0.0f);
    float num = Mathf.Abs(((Component) __instance).transform.lossyScale.x);
    __instance.head.MapMine(num, __instance.playerOffsetTransform);
    __instance.leftHand.MapMine(num, __instance.playerOffsetTransform);
    __instance.rightHand.MapMine(num, __instance.playerOffsetTransform);
  }
  private void RestoreSpeed()
  {
    if (((Object) this.speedPlayer != (Object) null))
    {
      this.speedPlayer.maxJumpSpeed = this.originalSpeed;
      this.speedPlayer.jumpMultiplier = this.originalMultiplier;
    }
    this.speedPlayer = (GTPlayer) null;
  }
  private void RestoreAliens()
  {
    foreach (KeyValuePair<GameObject, bool> hiddenGroup in this.hiddenGroups)
    {
      if (((Object) hiddenGroup.Key != (Object) null))
        hiddenGroup.Key.SetActive(hiddenGroup.Value);
    }
    foreach (KeyValuePair<Renderer, bool> hiddenRenderer in this.hiddenRenderers)
    {
      if (((Object) hiddenRenderer.Key != (Object) null))
        hiddenRenderer.Key.enabled = hiddenRenderer.Value;
    }
    foreach (KeyValuePair<Collider, bool> hiddenCollider in this.hiddenColliders)
    {
      if (((Object) hiddenCollider.Key != (Object) null))
        hiddenCollider.Key.enabled = hiddenCollider.Value;
    }
    foreach (KeyValuePair<Light, bool> hiddenLight in this.hiddenLights)
    {
      if (((Object) hiddenLight.Key != (Object) null))
        ((Behaviour) hiddenLight.Key).enabled = hiddenLight.Value;
    }
    this.hiddenGroups.Clear();
    this.hiddenRenderers.Clear();
    this.hiddenColliders.Clear();
    this.hiddenLights.Clear();
  }
  private void ClearPlatforms()
  {
    foreach (GameObject platform in this.platforms)
    {
      if (((Object) platform != (Object) null))
        Object.Destroy((Object) platform);
    }
    this.platforms.Clear();
    this.leftPlat = (GameObject) null;
    this.rightPlat = (GameObject) null;
  }
  private void StickyPlatforms(GorillaTagger tagger)
  {
    InputDevice deviceAtXrNode1 = InputDevices.GetDeviceAtXRNode((XRNode) 4);
    bool flag1;
    (deviceAtXrNode1).TryGetFeatureValue(CommonUsages.gripButton, out flag1);
    InputDevice deviceAtXrNode2 = InputDevices.GetDeviceAtXRNode((XRNode) 5);
    bool flag2;
    (deviceAtXrNode2).TryGetFeatureValue(CommonUsages.gripButton, out flag2);
    ControllerInputPoller instance = ControllerInputPoller.instance;
    if (((Object) instance != (Object) null))
    {
      flag1 = (double) instance.leftControllerGripFloat > 0.5;
      flag2 = (double) instance.rightControllerGripFloat > 0.5;
    }
    if (flag1 && !this.previousStickyLeftGrip)
      this.SpawnSticky(tagger.leftHandTransform);
    if (flag2 && !this.previousStickyRightGrip)
      this.SpawnSticky(tagger.rightHandTransform);
    this.previousStickyLeftGrip = flag1;
    this.previousStickyRightGrip = flag2;
  }
  private void SpawnSticky(Transform hand)
  {
    if (((Object) hand == (Object) null))
      return;
    GameObject platform = MenuPlatform.Create(this.menuPrefab, (hand.position + (Vector3.down * 0.05f)), hand.rotation);
    MenuPlatform.Paint(platform, this.state, Time.unscaledTime, this.platformColors);
    LocalMods.AddStickyPlates(platform);
    this.stickyPlatforms.Add(platform);
    if (this.stickyPlatforms.Count <= 72)
      return;
    Object.Destroy((Object) this.stickyPlatforms[0]);
    this.stickyPlatforms.RemoveAt(0);
  }
  private void ClearStickyPlatforms()
  {
    foreach (GameObject stickyPlatform in this.stickyPlatforms)
    {
      if (((Object) stickyPlatform != (Object) null))
        Object.Destroy((Object) stickyPlatform);
    }
    this.stickyPlatforms.Clear();
  }
  private void NoClip(bool force = false)
  {
    if ((double) Time.unscaledTime >= (double) this.noclipRefresh)
    {
      this.noclipRefresh = Time.unscaledTime + 1f;
      foreach (MeshCollider key in Object.FindObjectsByType<MeshCollider>((FindObjectsSortMode) 0))
      {
        if (!this.noclipColliders.ContainsKey(key))
          this.noclipColliders.Add(key, ((Collider) key).enabled);
      }
    }
    bool flag = false;
    InputDevice deviceAtXrNode = InputDevices.GetDeviceAtXRNode((XRNode) 5);
    (deviceAtXrNode).TryGetFeatureValue(CommonUsages.triggerButton, out flag);
    ControllerInputPoller instance = ControllerInputPoller.instance;
    if (((Object) instance != (Object) null))
      flag = instance.rightControllerTriggerButton || (double) instance.rightControllerIndexFloat > 0.5;
    foreach (KeyValuePair<MeshCollider, bool> noclipCollider in this.noclipColliders)
    {
      if (((Object) noclipCollider.Key != (Object) null))
        ((Collider) noclipCollider.Key).enabled = noclipCollider.Value && !(flag | force);
    }
  }
  private void RestoreColliders()
  {
    foreach (KeyValuePair<MeshCollider, bool> noclipCollider in this.noclipColliders)
    {
      if (((Object) noclipCollider.Key != (Object) null))
        ((Collider) noclipCollider.Key).enabled = noclipCollider.Value;
    }
    this.noclipColliders.Clear();
    this.noclipRefresh = 0.0f;
  }
  private void RestoreIce()
  {
    try
    {
      if (this.iceCaptured)
      {
        if (((Object) GTPlayer.Instance != (Object) null))
          GTPlayer.Instance.slideControl = this.iceOrig;
      }
    }
    catch
    {
    }
    this.iceCaptured = false;
  }
  private void GripMoves(GorillaTagger tagger)
  {
    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    bool flag4 = false;
    InputDevice deviceAtXrNode1 = InputDevices.GetDeviceAtXRNode((XRNode) 5);
    (deviceAtXrNode1).TryGetFeatureValue(CommonUsages.triggerButton, out flag1);
    InputDevice deviceAtXrNode2 = InputDevices.GetDeviceAtXRNode((XRNode) 4);
    (deviceAtXrNode2).TryGetFeatureValue(CommonUsages.triggerButton, out flag2);
    InputDevice deviceAtXrNode3 = InputDevices.GetDeviceAtXRNode((XRNode) 5);
    (deviceAtXrNode3).TryGetFeatureValue(CommonUsages.gripButton, out flag3);
    InputDevice deviceAtXrNode4 = InputDevices.GetDeviceAtXRNode((XRNode) 4);
    (deviceAtXrNode4).TryGetFeatureValue(CommonUsages.gripButton, out flag4);
    ControllerInputPoller instance = ControllerInputPoller.instance;
    if (((Object) instance != (Object) null))
    {
      flag1 = instance.rightControllerTriggerButton || (double) instance.rightControllerIndexFloat > 0.5;
      flag2 = instance.leftControllerTriggerButton || (double) instance.leftControllerIndexFloat > 0.5;
      flag3 = (double) instance.rightControllerGripFloat > 0.5;
      flag4 = (double) instance.leftControllerGripFloat > 0.5;
    }
    if (((Object) tagger.rigidbody == (Object) null))
      return;
    if (this.state.IsOn("Up Down"))
    {
      if (flag1)
      {
        Rigidbody rigidbody = tagger.rigidbody;
        rigidbody.linearVelocity = (rigidbody.linearVelocity + (Vector3.up * 10f * Time.fixedDeltaTime));
      }
      if (flag2)
      {
        Rigidbody rigidbody = tagger.rigidbody;
        rigidbody.linearVelocity = (rigidbody.linearVelocity - (Vector3.up * 10f * Time.fixedDeltaTime));
      }
    }
    if (!this.state.IsOn("Iron Monke"))
      return;
    if (flag3 && ((Object) tagger.rightHandTransform != (Object) null))
      tagger.rigidbody.AddForce((tagger.rightHandTransform.right * 15f), (ForceMode) 5);
    if (!flag4 || !((Object) tagger.leftHandTransform != (Object) null))
      return;
    tagger.rigidbody.AddForce(((-tagger.leftHandTransform.right) * 15f), (ForceMode) 5);
  }
  private void Beacons(GorillaTagger tagger)
  {
    if (((Object) this.beaconMaterial == (Object) null))
    {
      this.beaconMaterial = new Material(Shader.Find("GUI/Text Shader"));
      this.beaconMaterial.color = Color.white;
    }
    foreach (VRRig key in this.beaconLines.Keys.Where<VRRig>((Func<VRRig, bool>) (rig => ((Object) rig == (Object) null))).ToArray<VRRig>())
    {
      if (((Object) this.beaconLines[key] != (Object) null))
        Object.Destroy((Object) ((Component) this.beaconLines[key]).gameObject);
      this.beaconLines.Remove(key);
    }
    foreach (LineRenderer lineRenderer in this.beaconLines.Values)
    {
      if (((Object) lineRenderer != (Object) null))
        ((Component) lineRenderer).gameObject.SetActive(false);
    }
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((Object) activeRig == (Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal)
      {
        LineRenderer lineRenderer;
        if (!this.beaconLines.TryGetValue(activeRig, out lineRenderer) || ((Object) lineRenderer == (Object) null))
        {
          lineRenderer = new GameObject("ItzGameBeacon").AddComponent<LineRenderer>();
          ((Renderer) lineRenderer).sharedMaterial = this.beaconMaterial;
          lineRenderer.startWidth = 0.025f;
          lineRenderer.endWidth = 0.025f;
          lineRenderer.positionCount = 2;
          lineRenderer.useWorldSpace = true;
          this.beaconLines[activeRig] = lineRenderer;
        }
        Color playerColor = activeRig.playerColor;
        playerColor.a = 1f;
        lineRenderer.startColor = playerColor;
        lineRenderer.endColor = playerColor;
        ((Component) lineRenderer).gameObject.SetActive(true);
        lineRenderer.SetPosition(0, (((Component) activeRig).transform.position + new Vector3(0.0f, 9999f, 0.0f)));
        lineRenderer.SetPosition(1, (((Component) activeRig).transform.position - new Vector3(0.0f, 9999f, 0.0f)));
      }
    }
  }
  private void ClearBeacons()
  {
    foreach (LineRenderer lineRenderer in this.beaconLines.Values)
    {
      if (((Object) lineRenderer != (Object) null))
        Object.Destroy((Object) ((Component) lineRenderer).gameObject);
    }
    this.beaconLines.Clear();
  }
  private void DistanceTags(GorillaTagger tagger)
  {
    if (((Object) tagger.headCollider == (Object) null))
      return;
    Vector3 vector3 = ((Object) tagger.bodyCollider != (Object) null) ? ((Component) tagger.bodyCollider).transform.position : ((Component) tagger).transform.position;
    foreach (TextMesh textMesh in this.distTags.Values)
    {
      if (((Object) textMesh != (Object) null))
        ((Component) textMesh).gameObject.SetActive(false);
    }
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((Object) activeRig == (Object) null) && !activeRig.isOfflineVRRig && activeRig.head != null && !((Object) activeRig.head.rigTarget == (Object) null) && activeRig.Creator != null)
      {
        TextMesh textMesh;
        if (!this.distTags.TryGetValue(activeRig, out textMesh) || ((Object) textMesh == (Object) null))
        {
          textMesh = new GameObject("ItzGameDistTag").AddComponent<TextMesh>();
          textMesh.font = this.font;
          textMesh.fontSize = 0;
          textMesh.characterSize = 0.1728f / (float) this.font.fontSize;
          textMesh.anchor = (TextAnchor) 4;
          textMesh.alignment = (TextAlignment) 1;
          textMesh.richText = false;
          ((Component) textMesh).GetComponent<Renderer>().sharedMaterial = this.tagMaterial;
          this.distTags[activeRig] = textMesh;
        }
        float num = Vector3.Distance(vector3, ((Component) activeRig).transform.position);
        textMesh.text = $"{activeRig.Creator.SanitizedNickName}\n{num.ToString("F1")}m";
        ((Component) textMesh).gameObject.SetActive(true);
        ((Component) textMesh).transform.position = (activeRig.head.rigTarget.position + (Vector3.up * 0.35f));
        ((Component) textMesh).transform.rotation = Quaternion.LookRotation((((Component) textMesh).transform.position - ((Component) tagger.headCollider).transform.position));
      }
    }
  }
  private void ClearDistTags()
  {
    foreach (TextMesh textMesh in this.distTags.Values)
    {
      if (((Object) textMesh != (Object) null))
        Object.Destroy((Object) ((Component) textMesh).gameObject);
    }
    this.distTags.Clear();
  }
  private void WallWalk(GorillaTagger tagger)
  {
    bool flag1 = false;
    bool flag2 = false;
    InputDevice deviceAtXrNode1 = InputDevices.GetDeviceAtXRNode((XRNode) 5);
    (deviceAtXrNode1).TryGetFeatureValue(CommonUsages.gripButton, out flag1);
    InputDevice deviceAtXrNode2 = InputDevices.GetDeviceAtXRNode((XRNode) 4);
    (deviceAtXrNode2).TryGetFeatureValue(CommonUsages.gripButton, out flag2);
    ControllerInputPoller instance = ControllerInputPoller.instance;
    if (((Object) instance != (Object) null))
    {
      flag1 = (double) instance.rightControllerGripFloat > 0.5;
      flag2 = (double) instance.leftControllerGripFloat > 0.5;
    }
    if (((Object) tagger.rigidbody == (Object) null))
      return;
    RaycastHit raycastHit1;
    if (flag1 && ((Object) tagger.rightHandTransform != (Object) null) && Physics.Raycast(tagger.rightHandTransform.position, (-tagger.rightHandTransform.up), out raycastHit1) && (double) (raycastHit1).distance < 2.0)
    {
      Rigidbody rigidbody = tagger.rigidbody;
      rigidbody.linearVelocity = (rigidbody.linearVelocity - ((raycastHit1).normal * 15f * Time.fixedDeltaTime));
    }
    RaycastHit raycastHit2;
    if (!flag2 || !((Object) tagger.leftHandTransform != (Object) null) || !Physics.Raycast(tagger.leftHandTransform.position, (-tagger.leftHandTransform.up), out raycastHit2) || (double) (raycastHit2).distance >= 2.0)
      return;
    Rigidbody rigidbody1 = tagger.rigidbody;
    rigidbody1.linearVelocity = (rigidbody1.linearVelocity - ((raycastHit2).normal * 15f * Time.fixedDeltaTime));
  }
  private void ClearTags()
  {
    foreach (TextMesh textMesh in this.tags.Values)
    {
      if (((Object) textMesh != (Object) null))
        Object.Destroy((Object) ((Component) textMesh).gameObject);
    }
    this.tags.Clear();
  }
  private void Tracers(GorillaTagger tagger)
  {
    if (((Object) tagger.rightHandTransform == (Object) null))
      return;
    if (((Object) this.tracerMaterial == (Object) null))
    {
      this.tracerMaterial = new Material(Shader.Find("GUI/Text Shader"));
      this.tracerMaterial.color = Color.white;
    }
    foreach (VRRig key in this.tracers.Keys.Where<VRRig>((Func<VRRig, bool>) (rig => ((Object) rig == (Object) null))).ToArray<VRRig>())
    {
      if (((Object) this.tracers[key] != (Object) null))
        Object.Destroy((Object) ((Component) this.tracers[key]).gameObject);
      this.tracers.Remove(key);
    }
    Vector3 position = tagger.rightHandTransform.position;
    foreach (LineRenderer lineRenderer in this.tracers.Values)
    {
      if (((Object) lineRenderer != (Object) null))
        ((Component) lineRenderer).gameObject.SetActive(false);
    }
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((Object) activeRig == (Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal)
      {
        LineRenderer lineRenderer;
        if (!this.tracers.TryGetValue(activeRig, out lineRenderer) || ((Object) lineRenderer == (Object) null))
        {
          lineRenderer = new GameObject("ItzGameTracer").AddComponent<LineRenderer>();
          ((Renderer) lineRenderer).sharedMaterial = this.tracerMaterial;
          lineRenderer.startWidth = 0.025f;
          lineRenderer.endWidth = 0.025f;
          lineRenderer.positionCount = 2;
          lineRenderer.useWorldSpace = true;
          this.tracers[activeRig] = lineRenderer;
        }
        Color playerColor = activeRig.playerColor;
        playerColor.a = 1f;
        lineRenderer.startColor = playerColor;
        lineRenderer.endColor = playerColor;
        ((Component) lineRenderer).gameObject.SetActive(true);
        lineRenderer.SetPosition(0, position);
        lineRenderer.SetPosition(1, ((Component) activeRig).transform.position);
      }
    }
  }
  private void ClearTracers()
  {
    foreach (LineRenderer lineRenderer in this.tracers.Values)
    {
      if (((Object) lineRenderer != (Object) null))
        Object.Destroy((Object) ((Component) lineRenderer).gameObject);
    }
    this.tracers.Clear();
  }
  public void Dispose()
  {
    this.broom.Dispose();
    this.boards.Dispose();
    LocalMods.CrawlEnabled = false;
    this.harmony.UnpatchSelf();
    this.cosmetics.Dispose();
    this.pokruk.Dispose();
    try
    {
      this.detected.Dispose();
    }
    catch
    {
    }
    this.RestoreSpeed();
    this.RestoreCamera();
    this.RestoreAliens();
    this.RestoreClick();
    this.ClearPlatforms();
    this.ClearStickyPlatforms();
    this.ClearTags();
    this.ClearTracers();
    this.RestoreColliders();
    this.RestoreIce();
    this.ClearBeacons();
    this.ClearDistTags();
    try
    {
      if (((Object) GTPlayer.Instance != (Object) null))
        ((Component) GTPlayer.Instance).transform.localScale = Vector3.one;
    }
    catch
    {
    }
    Object.Destroy((Object) this.tagMaterial);
  }
}
