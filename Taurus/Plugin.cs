
using BepInEx;
using BepInEx.Configuration;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;
using Valve.Newtonsoft.Json;
using Object = UnityEngine.Object;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;
namespace Taurus;
[BepInPlugin("local.gtag.menutemplate", "Taurus", "1.4.0")]
public sealed class Plugin : BaseUnityPlugin
{
  public const string Version = "1.4.0";
  private ConfigEntry<Vector3> offset;
  private ConfigEntry<Vector3> rotation;
  private ConfigEntry<float> scale;
  private ConfigEntry<bool> shareMenus;
  private MenuNetworking sharing;
  private WorldEffectsNetworking effectsSharing;
  private AssetBundle bundle;
  private GameObject menu;
  private GameObject pointer;
  private BoxCollider[] buttons;
  private BoxCollider touching;
  private AudioSource clickSource;
  private AudioSource keyboardAudio;
  private AudioClip keyboardClick;
  private AudioClip[] clickSounds;
  private AudioSource activeClick;
  private float clickEndsAt;
  private readonly MenuState state = new MenuState();
  private MenuView view;
  private float openedAt;
  private bool vrVisible;
  private bool vrClosing;
  private float vrClosedAt;
  private float vrClosingScale;
  private AudioSource transitionSound;
  private AudioSource kyraLogoAudio;
  private AudioClip kyraLogoClip;
  private DesktopMenu desktop;
  private GunLib gun;
  private LocalMods features;
  private string savePath;
  private bool creatingLobby;
  private Soundboard soundboard;
  private GradientPicker gradientPicker;
  private bool pcFireArmed;
  private bool pcGunCursor;
  private bool pcPreviousCursor;
  private readonly MaterialPropertyBlock pointerColors = new MaterialPropertyBlock();
  private readonly List<GunLib.BodyTarget> targets = new List<GunLib.BodyTarget>();
  private TelemetryWebhook telemetry;
  private DiscordPresence presence;
  private RexonKeyboard rexonKeyboard;
  private RemoteFeaturePolicy remotePolicy;
  private int hitchCount;
  private float hitchLogAt;
  private float heartbeatAt;
  private string EnabledNames()
  {
    try
    {
      List<string> stringList = new List<string>();
      for (int index = 0; index < MenuCatalog.Buttons.Length && stringList.Count < 12; ++index)
      {
        if (this.state.IsOn(index))
          stringList.Add(MenuCatalog.Buttons[index].Name);
      }
      return string.Join(",", stringList.ToArray());
    }
    catch
    {
      return "?";
    }
  }
  private void Awake()
  {
    this.offset = this.Config.Bind<Vector3>("Placement", "Offset", Vector3.zero, "Offset from the left hand in metres.");
    this.rotation = this.Config.Bind<Vector3>("Placement", "Rotation", Vector3.zero, "Rotation relative to the left hand in degrees.");
    this.scale = this.Config.Bind<float>("Placement", "Scale", 1f, "Menu size multiplier, clamped to 0.25 through 3.");
    this.shareMenus = this.Config.Bind<bool>("Networking", "ShareMenus", true, "Share menu pages, enabled mods and button presses with other Taurus users in the same Photon room. Discord and typed input stay private.");
    this.savePath = Path.Combine(MenuDataPaths.DirectoryFor(Paths.GameRootPath), "settings.json");
    this.telemetry = new TelemetryWebhook((MonoBehaviour) this);
    try
    {
      MenuDataPaths.Migrate(Paths.GameRootPath);
      using (Stream manifestResourceStream = typeof (Plugin).Assembly.GetManifestResourceStream("ItzGameMenu.menutemplate.bundle"))
      {
        if (manifestResourceStream == null)
          throw new InvalidDataException("Embedded menu bundle is missing.");
        using (MemoryStream destination = new MemoryStream())
        {
          manifestResourceStream.CopyTo((Stream) destination);
          this.bundle = AssetBundle.LoadFromMemory(destination.ToArray());
        }
      }
      if (((Object) this.bundle == (Object) null))
        throw new InvalidDataException("Could not load the embedded menu bundle.");
      this.clickSounds = Array.ConvertAll<string, AudioClip>(MenuClickSounds.Paths, (Converter<string, AudioClip>) (path => this.bundle.LoadAsset<AudioClip>(path)));
      if (Array.Exists<AudioClip>(this.clickSounds, (Predicate<AudioClip>) (clip => ((Object) clip == (Object) null))))
        throw new InvalidDataException("Embedded click sound is missing.");
      this.kyraLogoClip = this.bundle.LoadAsset<AudioClip>("Assets/Audio/KyraLogo.mp3");
      if (((Object) this.kyraLogoClip == (Object) null))
        throw new InvalidDataException("Kyra logo sound is missing.");
      this.kyraLogoAudio = ((Component) this).gameObject.AddComponent<AudioSource>();
      this.kyraLogoAudio.playOnAwake = false;
      this.kyraLogoAudio.spatialBlend = 0.0f;
      GameObject gameObject = this.bundle.LoadAsset<GameObject>("Assets/Menu/MenuTemplate.prefab");
      this.menu = !((Object) gameObject == (Object) null) ? Object.Instantiate<GameObject>(gameObject) : throw new InvalidDataException("MenuTemplate prefab is missing.");
      this.keyboardAudio = ((Component) this).gameObject.AddComponent<AudioSource>();
      this.keyboardAudio.playOnAwake = false;
      this.keyboardAudio.spatialBlend = 0.0f;
      this.keyboardClick = ImportedAssetMenu.Load("menu-fonts").LoadAsset<AudioClip>("assets/audio/keyboard_click.ogg");
      if (((Object) this.keyboardClick == (Object) null))
        throw new InvalidDataException("Embedded keyboard sound is missing.");
      ((Object) this.menu).name = "TaurusMenu";
      Object.DontDestroyOnLoad((Object) this.menu);
      this.view = new MenuView(this.menu.transform, this.state);
      this.buttons = this.menu.GetComponentsInChildren<BoxCollider>(true);
      foreach (Component componentsInChild in this.menu.GetComponentsInChildren<Transform>(true))
        componentsInChild.gameObject.layer = 2;
      this.transitionSound = ((Component) this).gameObject.AddComponent<AudioSource>();
      this.transitionSound.playOnAwake = false;
      this.transitionSound.spatialBlend = 0.0f;
      this.transitionSound.clip = this.menu.GetComponents<AudioSource>()[1].clip;
      this.desktop = new DesktopMenu(gameObject, this.state, new Action<string, AudioSource>(this.Press), new Action(this.PlayTransitionSound));
      this.gradientPicker = new GradientPicker(this.state, (Action) (() =>
      {
        this.view.Refresh();
        this.desktop.View.Refresh();
      }), new Action(this.SaveSettings));
      this.sharing = new MenuNetworking(gameObject, (Action<string>) (message => this.Logger.LogWarning((object) message)), this.clickSounds, this.keyboardClick, this.kyraLogoClip);
      this.desktop.BrowserInput = (Func<Ray, Event, bool>) ((ray, input) => this.state.ColorPickerOpen && this.gradientPicker.Mouse(this.desktop.View.Root, ray, input));
      this.gun = new GunLib(this.bundle.LoadAsset<Material>("Assets/Menu/GunLine.mat"), this.bundle.LoadAsset<Material>("Assets/Menu/GunBall.mat"));
      this.gun.Triggered += (Action<Transform>) (body => this.Logger.LogInfo((object) ("Gun trigger: " + ((Object) body).name)));
      this.gun.PointTriggered += (Action<Vector3>) (point =>
      {
        if (!this.state.IsOn("Teleport Gun"))
          return;
        LocalMods.Teleport(((point + (this.gun.TargetNormal * 0.3f)) + Vector3.up));
      });
      this.gun.Triggered += (Action<Transform>) (body => this.features?.OnGunBody(body));
      this.gun.PointTriggered += (Action<Vector3>) (point => this.features?.OnGunPoint(point));
      this.features = new LocalMods(this.state, gameObject, ((Component) this.menu.transform.Find("Title")).GetComponent<TextMesh>().font)
      {
        Notify = new Action<string, bool>(this.Notify)
      };
      this.effectsSharing = new WorldEffectsNetworking(gameObject, this.state, this.features, this.gun, (Action<string>) (message => this.Logger.LogWarning((object) message)));
      RigPatchKit.Apply();
      this.state.RegisterHandler("Gun pointer", (Action<MenuEntry>) (entry => this.gun.Hide()));
      this.state.RegisterHandler("Teleport Gun", (Action<MenuEntry>) (entry => this.gun.Hide()));
      this.state.RegisterHandler("Create Public Lobby", (Action<MenuEntry>) (entry => this.CreatePublicLobby()));
      this.soundboard = new Soundboard((MonoBehaviour) this, this.state, Path.GetDirectoryName(this.savePath), (Action) (() =>
      {
        this.view.Refresh();
        this.desktop.View.Refresh();
      }), new Action<string, bool>(this.Notify));
      try
      {
        if (File.Exists(this.savePath))
          this.state.Restore(MenuSave.Load(this.savePath));
        else
          this.SaveSettings();
      }
      catch (Exception ex)
      {
        this.Logger.LogWarning((object) $"Could not restore {this.savePath}: {ex.Message}");
      }
      string path1 = Path.Combine(Path.GetDirectoryName(this.savePath), "feature-status.json");
      try
      {
        if (File.Exists(path1))
        {
          foreach (KeyValuePair<string, string> keyValuePair in JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path1)) ?? new Dictionary<string, string>())
            this.state.SetTestStatus(keyValuePair.Key, keyValuePair.Value);
        }
      }
      catch (Exception ex)
      {
        this.Logger.LogWarning((object) ("Could not load feature statuses: " + ex.Message));
      }
      foreach (string name in DetectedPorts.Names)
      {
        int index = MenuCatalog.Find(name);
        if (index >= 0 && MenuState.IsFeature(MenuCatalog.Buttons[index]) && this.state.HasCode(MenuCatalog.Buttons[index]))
          this.state.SetTestStatus(name, "W");
      }
      this.remotePolicy = new RemoteFeaturePolicy(this.Config, this.state, (Action<string>) (message =>
      {
        this.Logger.LogInfo((object) message);
        this.Notify(message);
      }));
      LocalMods.CrawlEnabled = this.state.IsOn("Spider Body");
      try
      {
        this.view.Refresh();
        this.desktop.View.Refresh();
      }
      catch (Exception ex)
      {
        this.Logger.LogError((object) ("Saved design failed to load; opening Taurus instead: " + ex?.ToString()));
        MenuSaveData data = this.state.Capture();
        data.design = "Taurus";
        this.state.Restore(data);
        this.view.Refresh();
        this.desktop.View.Refresh();
        this.SaveSettings();
      }
      this.clickSource = this.menu.GetComponent<AudioSource>();
      if (((Object) this.clickSource == (Object) null) || ((Object) this.clickSource.clip == (Object) null))
        throw new InvalidDataException("Menu click sound is missing.");
      this.pointer = GameObject.CreatePrimitive((PrimitiveType) 0);
      ((Object) this.pointer).name = "MenuPointer";
      this.pointer.layer = 2;
      Object.Destroy((Object) this.pointer.GetComponent<Collider>());
      this.pointer.transform.localScale = (Vector3.one * 0.01f);
      this.pointer.GetComponent<Renderer>().sharedMaterial = this.bundle.LoadAsset<Material>("Assets/Menu/Pointer.mat");
      Object.DontDestroyOnLoad((Object) this.pointer);
      this.Hide(false);
      try
      {
        this.presence = new DiscordPresence(this.Logger);
      }
      catch (Exception ex)
      {
        this.Logger.LogWarning((object) ("Discord RPC unavailable: " + ex.Message));
      }
    }
    catch (Exception ex)
    {
      this.Logger.LogError((object) ex);
      ((Behaviour) this).enabled = false;
      this.Cleanup();
    }
  }
  private void LateUpdate()
  {
    try
    {
      float num = Time.unscaledDeltaTime * 1000f;
      if ((double) num > 100.0)
      {
        ++this.hitchCount;
        if (this.hitchCount <= 5 || (double) Time.unscaledTime >= (double) this.hitchLogAt)
        {
          this.hitchLogAt = Time.unscaledTime + 10f;
          this.Logger.LogWarning((object) $"Hitch: {num.ToString("F0")}ms frame. hitches={this.hitchCount.ToString()} enabled=[{this.EnabledNames()}] room={(!((Object) NetworkSystem.Instance !=  (Object) null) || !NetworkSystem.Instance.InRoom ? "none" : NetworkSystem.Instance.RoomName)}");
        }
      }
      if ((double) Time.unscaledTime >= (double) this.heartbeatAt)
      {
        this.heartbeatAt = Time.unscaledTime + 30f;
        this.Logger.LogInfo((object) $"Heartbeat: hitches(>100ms)={this.hitchCount.ToString()} enabled=[{this.EnabledNames()}]");
        this.hitchCount = 0;
      }
    }
    catch
    {
    }
    if (((Object) this.activeClick != (Object) null) && (double) Time.unscaledTime >= (double) this.clickEndsAt)
    {
      this.activeClick.Stop();
      this.activeClick = (AudioSource) null;
    }
    GorillaTagger tagger = GorillaTagger.Instance;
    NetworkSystem instance1 = NetworkSystem.Instance;
    this.presence?.Tick(instance1);
    RemoteFeaturePolicy remotePolicy = this.remotePolicy;
    if ((remotePolicy != null ? (remotePolicy.Tick() ? 1 : 0) : 0) != 0)
    {
      this.SaveSettings();
      this.view.Refresh();
      this.desktop.View.Refresh();
    }
    bool inRoom = ((Object) instance1 != (Object) null) && instance1.InRoom;
    this.telemetry?.Tick(true, inRoom, inRoom ? instance1.RoomName : (string) null);
    string str = (string) null;
    try
    {
      str = instance1.GameModeString;
    }
    catch
    {
    }
    try
    {
      object obj = (object) null;
      if (PhotonNetwork.CurrentRoom != null)
      {
        if (((RoomInfo) PhotonNetwork.CurrentRoom).CustomProperties != null)
        {
          if (((Dictionary<object, object>) ((RoomInfo) PhotonNetwork.CurrentRoom).CustomProperties).TryGetValue((object) "gameMode", out obj))
          {
            if (obj != null)
              str = $"{str};{obj.ToString()}";
          }
        }
      }
    }
    catch
    {
    }
    try
    {
      string roomName = instance1.RoomName;
      if (!string.IsNullOrEmpty(roomName))
        str = $"{str};{roomName}";
    }
    catch
    {
    }
    bool flag1 = instance1?.CurrentRoom == null || instance1.CurrentRoom.isPublic;
    if (this.state.UpdateRoom(inRoom, false, inRoom ? str : (string) null, inRoom && instance1.IsMasterClient))
    {
      try
      {
        this.Logger.LogInfo((object) $"Room changed: inRoom={inRoom.ToString()} public={flag1.ToString()} mode={str} antiban={this.state.PrivateModdedRoom.ToString()}");
      }
      catch
      {
      }
      this.SaveSettings();
      this.view.Refresh();
      this.desktop.View.Refresh();
      if (this.state.Notice.Length > 0)
      {
        this.Logger.LogWarning((object) this.state.Notice);
        this.Notify(this.state.Notice, true);
      }
    }
    this.view.SetFavoriteMode(Plugin.FavoriteModifier());
    this.desktop.View.SetFavoriteMode(Plugin.FavoriteModifier());
    this.desktop.Tick();
    if (this.desktop.IsClosing)
      this.gradientPicker?.Release();
    this.soundboard?.Tick();
    try
    {
      this.features?.Tick(this.desktop.IsOpen || this.desktop.IsClosing);
    }
    catch (Exception ex)
    {
      this.Logger.LogError((object) ex);
      this.SaveSettings();
      this.Notify("Local feature failed", true);
      this.view.Refresh();
      this.desktop.View.Refresh();
    }
    this.UpdateGun(tagger);
    this.effectsSharing?.Tick();
    ControllerInputPoller instance2 = ControllerInputPoller.instance;
    bool flag2 = ((Object) instance2 != (Object) null) && (this.state.RightHanded ? instance2.rightControllerSecondaryButton : instance2.leftControllerSecondaryButton);
    if (((Object) instance2 == (Object) null))
    {
      InputDevice deviceAtXrNode = InputDevices.GetDeviceAtXRNode(this.state.RightHanded ? (XRNode) 5 : (XRNode) 4);
      (deviceAtXrNode).TryGetFeatureValue(CommonUsages.secondaryButton, out flag2);
    }
    bool flag3 = this.state.Design == 9 && this.state.KeyboardOpen && this.state.Category != 11 && !this.desktop.IsOpen;
    if (flag3 && ((Object) tagger != (Object) null) && ((Object) tagger.bodyCollider != (Object) null) && ((Object) tagger.leftHandTransform != (Object) null) && ((Object) tagger.rightHandTransform != (Object) null))
    {
      if (this.rexonKeyboard == null)
      {
        this.rexonKeyboard = new RexonKeyboard(this.state, ((Component) this.menu.transform.Find("Background")).GetComponent<Renderer>().sharedMaterial, ((Component) this.menu.transform.Find("XyferFont")).GetComponent<TMP_Text>().font, (Action) (() =>
        {
          this.view.Refresh();
          this.desktop.View.Refresh();
        }), (Action<bool>) (isLeft =>
        {
          tagger.StartVibration(isLeft, 0.05f, 0.02f);
          this.PlayClick(this.clickSource, "Key_Rexon");
        }));
        this.rexonKeyboard.Spawn(((Component) tagger.bodyCollider).transform, tagger.leftHandTransform, tagger.rightHandTransform);
      }
      this.rexonKeyboard.RefreshDisplay();
      flag2 = true;
    }
    else if (this.rexonKeyboard != null)
    {
      this.rexonKeyboard.Dispose();
      this.rexonKeyboard = (RexonKeyboard) null;
    }
    this.sharing?.Tick(this.state, this.desktop.IsOpen || flag2 && ((Object) tagger != (Object) null), this.shareMenus.Value, flag2 && !this.desktop.IsOpen && ((Object) tagger != (Object) null), this.offset.Value, this.rotation.Value, this.scale.Value, this.desktop.IsOpen ? this.desktop.View.HoveredButton : this.view.HoveredButton);
    if (this.desktop.IsOpen || ((Object) tagger == (Object) null) || ((Object) tagger.leftHandTransform == (Object) null) || ((Object) tagger.rightHandTransform == (Object) null) || !flag2)
    {
      this.Hide();
    }
    else
    {
      Transform hand = this.state.RightHanded ? tagger.rightHandTransform : tagger.leftHandTransform;
      if (!this.vrVisible)
      {
        this.openedAt = Time.unscaledTime;
        this.vrVisible = true;
        this.vrClosing = false;
        this.PlayTransitionSound();
      }
      Vector3 offset = this.offset.Value;
      Vector3 rotation = this.rotation.Value;
      int num = this.state.RightHanded ? 1 : 0;
      Pose pose = MenuPose.Local(hand, offset, rotation, num != 0);
      if (flag3 && ((Object) tagger.headCollider != (Object) null))
      {
        Transform transform = ((Component) tagger.headCollider).transform;
        Vector3 forward = transform.forward;
        forward.y = 0.0f;
        (forward).Normalize();
        pose = new Pose(((transform.position + (forward * 0.45f)) - (Vector3.up * 0.05f)), Quaternion.LookRotation(Vector3.up, (-Vector3.Cross(Vector3.up, forward))));
      }
      this.menu.transform.SetPositionAndRotation(pose.position, pose.rotation);
      float f = this.scale.Value;
      float elapsed = Time.unscaledTime - this.openedAt;
      this.menu.transform.localScale = ((((Vector3.one * (float.IsNaN(f) || float.IsInfinity(f) ? 1f : Mathf.Clamp(f, 0.25f, 3f))) * this.state.VrScale) * MenuStyle.DesignVrScale(this.state.Design, this.state.Category)) * MenuStyle.OpenScale(elapsed));
      Transform transform1 = this.menu.transform;
      transform1.position = (transform1.position + this.menu.transform.TransformVector(this.view.DesignSlide(Time.unscaledTime)));
      this.pointer.transform.position = MenuPose.PointerPosition(this.state.RightHanded ? tagger.leftHandTransform : tagger.rightHandTransform);
      this.menu.SetActive(true);
      this.pointer.SetActive(true);
      this.pointerColors.SetColor("_Color", MenuPalette.Right(this.state, Time.unscaledTime));
      this.pointer.GetComponent<Renderer>().SetPropertyBlock(this.pointerColors);
      this.view.Animate(Time.unscaledTime);
      Physics.SyncTransforms();
      if ((double) elapsed < 0.31999999284744263)
        return;
      if (this.gradientPicker.Touch(this.menu.transform, this.pointer.transform.position))
      {
        this.touching = (BoxCollider) null;
        this.view.SetHover((BoxCollider) null);
      }
      else
      {
        BoxCollider button1 = (BoxCollider) null;
        foreach (BoxCollider button2 in this.buttons)
        {
          if (((Collider) button2).enabled && ((Component) button2).gameObject.activeInHierarchy)
          {
            Vector3 vector3 = (((Collider) button2).ClosestPoint(this.pointer.transform.position) - this.pointer.transform.position);
            if ((double) (vector3).sqrMagnitude <= 2.4999999368446879E-05)
            {
              button1 = button2;
              break;
            }
          }
        }
        if (((Object) button1 == (Object) this.touching))
          return;
        this.touching = button1;
        this.view.SetHover(button1);
        this.pointer.transform.localScale = (Vector3.one * (((Object) button1 == (Object) null) ? 0.01f : 0.012f));
        if (((Object) button1 == (Object) null))
          return;
        this.Press(((Object) button1).name, this.clickSource);
        InputDevice deviceAtXrNode = InputDevices.GetDeviceAtXRNode(this.state.RightHanded ? (XRNode) 4 : (XRNode) 5);
        (deviceAtXrNode).SendHapticImpulse(0U, 0.25f, 0.05f);
      }
    }
  }
  private void FixedUpdate()
  {
    this.features?.FixedTick(this.desktop != null && this.desktop.IsOpen);
  }
  private void OnGUI()
  {
    if (Event.current.type == (EventType) 4 && Event.current.keyCode == (KeyCode) 113 && this.desktop != null && !this.desktop.IsOpen)
      this.HideGun();
    this.desktop?.OnGUI();
  }
  private void Press(string name, AudioSource audio)
  {
    try
    {
      string message = (string) null;
      bool error = false;
      this.PlayClick(audio, name);
      this.view.Pulse(name);
      this.desktop.View.Pulse(name);
      int row;
      if (Plugin.FavoriteModifier() && this.FavoriteRow(name, out row) && this.state.ToggleFavorite(this.state.EntryAt(row)))
      {
        this.SaveSettings();
        message = this.state.Notice;
      }
      else
      {
        switch (name)
        {
          case "ButtonEnabled":
            this.state.SelectCategory(1);
            break;
          case "ButtonRoomStatus":
            this.state.SelectCategory(4);
            break;
          case "DesignLogo":
            if (this.state.Design != 10)
            {
              this.state.SelectCategory(22);
              break;
            }
            break;
          case "DesignSettings":
            this.state.SelectCategory(3);
            break;
          default:
            if (name.StartsWith("Key_"))
            {
              this.state.Type(name.Substring(4));
              break;
            }
            if (name.StartsWith("IncrementMinus") || name.StartsWith("IncrementPlus"))
            {
              bool flag = name.StartsWith("IncrementMinus");
              int result;
              if (int.TryParse(name.Substring(flag ? 14 : 13), out result))
              {
                MenuEntry menuEntry = this.state.Adjust(result, flag ? -1 : 1);
                if (menuEntry != null)
                {
                  this.soundboard?.ApplySettings();
                  this.SaveSettings();
                  if (menuEntry.Name.StartsWith("Click sound"))
                  {
                    this.PlayClick(audio);
                    break;
                  }
                  break;
                }
                break;
              }
              break;
            }
            int result1;
            if (name.StartsWith("Row") && int.TryParse(name.Substring(3), out result1))
            {
              if (this.state.SoundNameAt(result1) != null)
              {
                Soundboard soundboard = this.soundboard;
                if (soundboard != null)
                {
                  soundboard.Press(result1);
                  break;
                }
                break;
              }
              int index = this.state.EntryAt(result1);
              MenuEntry menuEntry = this.state.Activate(index);
              if (menuEntry != null)
              {
                this.SaveSettings();
                this.soundboard?.ApplySettings();
                message = this.state.Notice.Length > 0 ? this.state.Notice : (menuEntry.Toggle ? menuEntry.Name + (this.state.IsOn(menuEntry.Name) ? " enabled" : " disabled") : menuEntry.Name + " applied");
                if (menuEntry.Name == "VR size +" || menuEntry.Name == "VR size -" || menuEntry.Name == "Reset VR size")
                  message = $"VR size {Mathf.RoundToInt(this.state.VrScale * 100f).ToString()}%";
                if (menuEntry.Name == "Create Public Lobby" || menuEntry.Name == "Developer: ItzGame")
                  message = (string) null;
                if (this.state.ColorPickerOpen)
                  message = (string) null;
                if (menuEntry.Name == "Type Code" || menuEntry.Name == "Set Name" || menuEntry.Name == "Join Room")
                {
                  message = "Type it, then Done";
                  break;
                }
                break;
              }
              if (index >= 0 && this.state.Notice.Length > 0)
              {
                this.Logger.LogInfo((object) this.state.Notice);
                message = this.state.Notice;
                error = true;
                break;
              }
              break;
            }
            switch (name)
            {
              case "PastCategoryPrevious":
                this.state.MovePastCategories(-1);
                break;
              case "PastCategoryNext":
                this.state.MovePastCategories(1);
                break;
              default:
                int result2;
                if (name.StartsWith("PastCategory", StringComparison.Ordinal) && int.TryParse(name.Substring(12), out result2))
                {
                  int category = this.state.PastCategoryAt(result2);
                  if (category >= 0)
                  {
                    this.state.SelectCategory(category);
                    break;
                  }
                  break;
                }
                switch (name)
                {
                  case "ButtonSearch":
                    if (this.state.Design >= 5 && this.state.KeyboardOpen)
                    {
                      this.state.Type("Done");
                      break;
                    }
                    this.state.OpenSearch();
                    break;
                  case "ButtonPrevious":
                    this.state.MovePage(-1);
                    break;
                  case "ButtonNext":
                    this.state.MovePage(1);
                    break;
                  case "ButtonHome":
                    if (this.state.ColorPickerOpen)
                    {
                      this.gradientPicker.Release();
                      this.state.AdvanceColorPicker();
                      this.SaveSettings();
                      break;
                    }
                    this.state.Home();
                    break;
                  case "ButtonDisconnect":
                    if (this.state.Design == 7 && !this.state.InRoom)
                    {
                      LobbyActions.JoinRandomPublic();
                      break;
                    }
                    this.Disconnect();
                    break;
                }
                break;
            }
            break;
        }
      }
      this.view.Refresh();
      this.desktop.View.Refresh();
      this.sharing?.Press(name);
      if (message == null)
        return;
      this.Notify(message, error);
    }
    catch (Exception ex)
    {
      this.Logger.LogError((object) ex);
      this.view.Refresh();
      this.desktop.View.Refresh();
      this.Notify("Action failed", true);
      this.SaveSettings();
    }
  }
  private void PlayClick(AudioSource audio, string button = "")
  {
    if (this.state.Design == 10 && button == "DesignLogo")
    {
      this.kyraLogoAudio.Stop();
      this.kyraLogoAudio.PlayOneShot(this.kyraLogoClip, this.state.ClickVolume);
    }
    else
    {
      if (((Object) this.activeClick != (Object) null))
        this.activeClick.Stop();
      if (button.StartsWith("Key_", StringComparison.Ordinal) && ((Object) this.keyboardClick != (Object) null))
      {
        this.keyboardAudio.PlayOneShot(this.keyboardClick, this.state.ClickVolume);
        this.activeClick = this.keyboardAudio;
        this.clickEndsAt = Time.unscaledTime + Mathf.Min(this.keyboardClick.length, 1f);
      }
      else if ((this.state.Design == 4 || this.state.Design == 5 || this.state.Design == 6 || this.state.Design == 9 || this.state.Design == 11) && this.state.ClickSound == 0 && ((Object) VRRig.LocalRig != (Object) null))
      {
        VRRig.LocalRig.PlayHandTapLocal(this.state.Design == 9 ? (button.StartsWith("Key_", StringComparison.Ordinal) ? 66 : 106) : (this.state.Design == 5 || this.state.Design == 11 ? (button.StartsWith("Key_", StringComparison.Ordinal) ? 66 : 8) : 67), this.state.RightHanded, this.state.ClickVolume);
      }
      else
      {
        int index = MenuClickSounds.DefaultIndex(this.state.Design);
        AudioClip audioClip = this.state.ClickSound == 0 ? (index != 0 ? this.clickSounds[index] : ImportedAssetMenu.ClickSound(this.state, button) ?? this.clickSounds[0]) : this.clickSounds[this.state.ClickSound];
        audio.PlayOneShot(audioClip, this.state.ClickVolume);
        this.activeClick = audio;
        this.clickEndsAt = Time.unscaledTime + Mathf.Min(audioClip.length, 1f);
      }
    }
  }
  private void Notify(string message, bool error = false)
  {
    this.view?.ShowNotification(message, error);
    this.desktop?.View.ShowNotification(message, error);
  }
  private static bool FavoriteModifier()
  {
    ControllerInputPoller instance = ControllerInputPoller.instance;
    bool flag = ((Object) instance != (Object) null) && (double) instance.leftControllerGripFloat > 0.5;
    if (((Object) instance == (Object) null))
    {
      InputDevice deviceAtXrNode = InputDevices.GetDeviceAtXRNode((XRNode) 4);
      (deviceAtXrNode).TryGetFeatureValue(CommonUsages.gripButton, out flag);
    }
    if (flag)
      return true;
    Keyboard current = Keyboard.current;
    return current != null && ((ButtonControl) current.leftShiftKey).isPressed;
  }
  private bool FavoriteRow(string name, out int row)
  {
    return int.TryParse(name.StartsWith("Row") ? name.Substring(3) : (name.StartsWith("IncrementMinus") ? name.Substring(14) : (name.StartsWith("IncrementPlus") ? name.Substring(13) : "")), out row) && row >= 0 && row < this.state.VisibleRows;
  }
  private async Task Disconnect()
  {
    try
    {
      if (!((Object) NetworkSystem.Instance != (Object) null) || !NetworkSystem.Instance.InRoom)
        return;
      await NetworkSystem.Instance.ReturnToSinglePlayer();
    }
    catch (Exception ex)
    {
      this.Logger.LogError((object) ex);
    }
  }
  private async Task CreatePublicLobby()
  {
    if (this.creatingLobby)
      return;
    this.creatingLobby = true;
    this.Notify("Creating public lobby");
    try
    {
      this.Notify("Public lobby " + await LobbyActions.CreatePublic());
    }
    catch (Exception ex)
    {
      this.Logger.LogError((object) ex);
      this.Notify("Public lobby failed", true);
    }
    finally
    {
      this.creatingLobby = false;
    }
  }
  private void SaveSettings()
  {
    if (this.savePath == null)
      return;
    try
    {
      MenuSave.Save(this.savePath, this.state.Capture());
    }
    catch (Exception ex)
    {
      this.Logger.LogWarning((object) $"Could not save {this.savePath}: {ex.Message}");
    }
  }
  private void UpdateGun(GorillaTagger tagger)
  {
    if (!Application.isFocused)
    {
      this.HideGun();
    }
    else
    {
      this.gun.SetDesign(this.state.Design);
      this.gun.SetColors(MenuPalette.Left(this.state, Time.unscaledTime), MenuPalette.Right(this.state, Time.unscaledTime));
      if (!this.state.HasActiveGun || ((Object) tagger == (Object) null))
        this.HideGun();
      else if (this.desktop != null && (this.desktop.IsOpen || this.desktop.IsClosing))
      {
        this.HideGun();
      }
      else
      {
        InputDevice deviceAtXrNode = InputDevices.GetDeviceAtXRNode((XRNode) 5);
        this.targets.Clear();
        foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
        {
          if (((Object) activeRig != (Object) null) && !activeRig.isOfflineVRRig && ((Object) activeRig.bodyTransform != (Object) null))
            this.targets.Add(new GunLib.BodyTarget(activeRig.bodyTransform, 0.3f * Mathf.Abs(((Component) activeRig).transform.lossyScale.x), ((Component) activeRig).transform));
        }
        if (Mouse.current != null && Mouse.current.rightButton.isPressed || !XRSettings.isDeviceActive || !(deviceAtXrNode).isValid)
        {
          Mouse current = Mouse.current;
          if (current == null || !current.rightButton.isPressed)
          {
            this.HideGun();
          }
          else
          {
            Camera camera = this.gun.DesktopCamera();
            Ray ray;
            if (current == null || !GunLib.DesktopRay(camera, ((InputControl<Vector2>) ((Pointer) current).position).ReadValue(), Cursor.lockState == (CursorLockMode) 1, out ray))
            {
              this.HideGun();
            }
            else
            {
              this.gun.UseCamera(camera);
              if (!this.pcGunCursor)
              {
                this.pcPreviousCursor = Cursor.visible;
                this.pcGunCursor = true;
              }
              Cursor.visible = Cursor.lockState != (CursorLockMode) 1;
              if (!current.leftButton.isPressed)
                this.pcFireArmed = true;
              Vector3 worldPoint = camera.ViewportToWorldPoint(new Vector3(0.6f, 0.35f, Mathf.Max(0.35f, camera.nearClipPlane + 0.05f)));
              this.gun.Update((ray).origin, (ray).direction, true, this.pcFireArmed && current.leftButton.isPressed, (IReadOnlyList<GunLib.BodyTarget>) this.targets, ((Object) tagger.offlineVRRig == (Object) null) ? (Transform) null : ((Component) tagger.offlineVRRig).transform, new Vector3?(worldPoint), ((Component) GTPlayer.Instance)?.transform);
            }
          }
        }
        else
        {
          this.ReleaseGunCursor();
          this.gun.UseCamera((Camera) null);
          if (((Object) tagger.rightHandTransform == (Object) null))
          {
            this.gun.Hide();
          }
          else
          {
            bool flag1;
            (deviceAtXrNode).TryGetFeatureValue(CommonUsages.gripButton, out flag1);
            bool flag2;
            (deviceAtXrNode).TryGetFeatureValue(CommonUsages.triggerButton, out flag2);
            ControllerInputPoller instance = ControllerInputPoller.instance;
            float num1;
            (deviceAtXrNode).TryGetFeatureValue(CommonUsages.grip, out num1);
            float num2;
            (deviceAtXrNode).TryGetFeatureValue(CommonUsages.trigger, out num2);
            bool aiming = flag1 | (double) num1 > 0.5;
            bool trigger = flag2 | (double) num2 > 0.5;
            if (((Object) instance != (Object) null))
            {
              aiming |= (double) instance.rightControllerGripFloat > 0.5;
              trigger = ((trigger ? 1 : 0) | (instance.rightControllerTriggerButton ? 1 : ((double) instance.rightControllerIndexFloat > 0.5 ? 1 : 0))) != 0;
            }
            if (!aiming)
              this.gun.Hide();
            else
              this.gun.Update(tagger.rightHandTransform, aiming, trigger, (IReadOnlyList<GunLib.BodyTarget>) this.targets, ((Object) tagger.offlineVRRig == (Object) null) ? (Transform) null : ((Component) tagger.offlineVRRig).transform, ((Component) GTPlayer.Instance)?.transform);
          }
        }
      }
    }
  }
  private void ReleaseGunCursor()
  {
    if (!this.pcGunCursor)
      return;
    Cursor.visible = this.pcPreviousCursor;
    this.pcGunCursor = false;
  }
  private void HideGun()
  {
    this.gun?.Hide();
    this.pcFireArmed = false;
    this.ReleaseGunCursor();
  }
  private void PlayTransitionSound()
  {
    if (!((Object) this.transitionSound != (Object) null) || !((Object) this.transitionSound.clip != (Object) null))
      return;
    this.transitionSound.PlayOneShot(this.transitionSound.clip, this.state.WhooshVolume);
  }
  private void Hide(bool animate = true)
  {
    if (this.desktop == null || !this.desktop.IsOpen)
      this.gradientPicker?.Release();
    this.touching = (BoxCollider) null;
    if (this.view != null)
      this.view.SetHover((BoxCollider) null);
    if (((Object) this.menu != (Object) null))
    {
      if (this.vrVisible & animate)
      {
        this.vrClosedAt = Time.unscaledTime;
        this.vrClosingScale = this.menu.transform.localScale.x;
        this.vrClosing = true;
        this.PlayTransitionSound();
      }
      this.vrVisible = false;
      float elapsed = Time.unscaledTime - this.vrClosedAt;
      if (!animate || (double) elapsed >= 0.20000000298023224)
        this.vrClosing = false;
      if (this.vrClosing)
      {
        this.menu.transform.localScale = ((Vector3.one * this.vrClosingScale) * MenuStyle.CloseScale(elapsed));
        this.view.Animate(Time.unscaledTime);
      }
      else
        this.menu.SetActive(false);
    }
    if (!((Object) this.pointer != (Object) null))
      return;
    this.pointer.SetActive(false);
    this.pointer.transform.localScale = (Vector3.one * 0.01f);
  }
  private void OnDisable()
  {
    this.effectsSharing?.Dispose();
    this.effectsSharing = (WorldEffectsNetworking) null;
    this.rexonKeyboard?.Dispose();
    this.rexonKeyboard = (RexonKeyboard) null;
    if (((Object) this.activeClick != (Object) null))
    {
      this.activeClick.Stop();
      this.activeClick = (AudioSource) null;
    }
    this.kyraLogoAudio?.Stop();
    this.HideGun();
    if (((Object) this.menu != (Object) null))
      this.SaveSettings();
    this.Hide(false);
    this.gun?.Hide();
    this.sharing?.Clear();
    this.soundboard?.Stop();
    if (this.desktop != null && this.desktop.IsOpen)
      this.desktop.Toggle();
    this.features?.Dispose();
    this.features = (LocalMods) null;
  }
  private void OnEnable()
  {
    if (this.features != null || !((Object) this.menu != (Object) null) || !((Object) this.bundle != (Object) null))
      return;
    this.features = new LocalMods(this.state, this.bundle.LoadAsset<GameObject>("Assets/Menu/MenuTemplate.prefab"), ((Component) this.menu.transform.Find("Title")).GetComponent<TextMesh>().font)
    {
      Notify = new Action<string, bool>(this.Notify)
    };
    this.effectsSharing = new WorldEffectsNetworking(this.bundle.LoadAsset<GameObject>("Assets/Menu/MenuTemplate.prefab"), this.state, this.features, this.gun, (Action<string>) (message => this.Logger.LogWarning((object) message)));
  }
  private void OnDestroy() => this.Cleanup();
  private void Cleanup()
  {
    this.effectsSharing?.Dispose();
    this.effectsSharing = (WorldEffectsNetworking) null;
    this.rexonKeyboard?.Dispose();
    this.rexonKeyboard = (RexonKeyboard) null;
    this.presence?.Dispose();
    this.presence = (DiscordPresence) null;
    this.remotePolicy?.Dispose();
    this.remotePolicy = (RemoteFeaturePolicy) null;
    this.HideGun();
    this.soundboard?.Dispose();
    this.soundboard = (Soundboard) null;
    this.sharing?.Dispose();
    this.sharing = (MenuNetworking) null;
    this.desktop?.Dispose();
    this.gun?.Dispose();
    this.features?.Dispose();
    this.features = (LocalMods) null;
    this.view?.Dispose();
    this.desktop = (DesktopMenu) null;
    this.gun = (GunLib) null;
    if (((Object) this.menu != (Object) null))
      Object.Destroy((Object) this.menu);
    if (((Object) this.pointer != (Object) null))
      Object.Destroy((Object) this.pointer);
    if (!((Object) this.bundle != (Object) null))
      return;
    this.bundle.Unload(false);
  }
}
