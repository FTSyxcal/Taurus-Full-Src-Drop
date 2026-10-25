
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class MenuNetworking : IDisposable
{
  public const string Property = "Taurus.Menu.v6";
  private readonly GameObject prefab;
  private readonly Dictionary<int, MenuNetworking.Remote> peers = new Dictionary<int, MenuNetworking.Remote>();
  private readonly List<int> removed = new List<int>();
  private readonly Action<string> warning;
  private readonly AudioClip[] sounds;
  private readonly AudioClip keyboardSound;
  private readonly AudioClip logoSound;
  private Room room;
  private byte[] lastSent;
  private float nextPoll;
  private float nextHeartbeat;
  private float nextWarning;
  private byte press;
  private int pressSerial;
  private readonly MaterialPropertyBlock pointerColor = new MaterialPropertyBlock();
  public MenuNetworking(
    GameObject prefab,
    Action<string> warning,
    AudioClip[] sounds,
    AudioClip keyboardSound,
    AudioClip logoSound)
  {
    this.prefab = prefab;
    this.warning = warning;
    this.sounds = sounds;
    this.keyboardSound = keyboardSound;
    this.logoSound = logoSound;
  }
  public void Press(string name)
  {
    byte num = MenuPacket.PressId(name);
    if (num == (byte) 0)
      return;
    this.press = num;
    ++this.pressSerial;
  }
  public void Tick(
    MenuState state,
    bool visible,
    bool enabled,
    bool pointerVisible,
    Vector3 offset,
    Vector3 rotation,
    float scale,
    string hovered)
  {
    try
    {
      if (!enabled || !PhotonNetwork.InRoom)
      {
        if (this.room == null && this.peers.Count <= 0)
          return;
        this.Clear();
      }
      else
      {
        if (this.room != PhotonNetwork.CurrentRoom)
        {
          this.Clear();
          this.room = PhotonNetwork.CurrentRoom;
          this.lastSent = (byte[]) null;
          this.nextPoll = 0.0f;
        }
        float unscaledTime = Time.unscaledTime;
        if ((double) unscaledTime >= (double) this.nextPoll)
        {
          this.nextPoll = unscaledTime + 0.1f;
          MenuPacket menuPacket = new MenuPacket()
          {
            Visible = visible,
            Category = (byte) state.Category,
            Page = (ushort) state.Page,
            Theme = (byte) Math.Max(0, Array.IndexOf<string>(MenuPacket.Themes, state.Theme)),
            Design = (byte) state.Design,
            RightHanded = state.RightHanded,
            Rounded = state.Rounded,
            FlySpeed = state.FlySpeed,
            VrScale = state.VrScale,
            Scale = float.IsNaN(scale) || float.IsInfinity(scale) ? 1f : Mathf.Clamp(scale, 0.25f, 3f),
            PrivateModded = state.PrivateModdedRoom,
            Master = state.IsMasterClient,
            PrivateInput = state.KeyboardOpen || state.Query.Length > 0 || state.Category == 10,
            Press = this.press,
            PressSerial = this.pressSerial,
            Timestamp = PhotonNetwork.ServerTimestamp
          };
          if (state.Category == 12)
          {
            menuPacket.PrivateInput = true;
            menuPacket.Page = (ushort) 0;
          }
          menuPacket.ColorPickerOpen = state.ColorPickerOpen;
          menuPacket.EditingEnd = state.ColorPickerOpen && state.EditingEnd;
          menuPacket.ClickVolume = state.ClickVolume;
          menuPacket.GradientStart = state.GradientStart;
          menuPacket.GradientEnd = state.GradientEnd;
          menuPacket.GradientPeriod = state.GradientPeriod;
          menuPacket.ClickSound = (byte) state.ClickSound;
          menuPacket.InRoom = state.InRoom;
          menuPacket.Font = (byte) state.FontIndex;
          VRRig localRig = VRRig.LocalRig;
          Transform rigTarget1 = ((Object) localRig == (Object) null) ? (Transform) null : (state.RightHanded ? localRig.rightHand : localRig.leftHand)?.rigTarget;
          GorillaTagger instance = GorillaTagger.Instance;
          Transform hand = ((Object) instance == (Object) null) ? (Transform) null : (state.RightHanded ? instance.rightHandTransform : instance.leftHandTransform);
          if (((Object) rigTarget1 != (Object) null) && ((Object) hand != (Object) null))
          {
            Pose pose = MenuPose.Relative(rigTarget1, MenuPose.Local(hand, offset, rotation, state.RightHanded));
            Vector3 eulerAngles = (pose.rotation).eulerAngles;
            for (int index = 0; index < 3; ++index)
            {
              menuPacket.Placement[index] = Mathf.Clamp((pose.position)[index], -2f, 2f);
              menuPacket.Placement[index + 3] = (eulerAngles)[index];
            }
          }
          else
            menuPacket.Visible = false;
          Transform rigTarget2 = ((Object) localRig == (Object) null) ? (Transform) null : (state.RightHanded ? localRig.leftHand : localRig.rightHand)?.rigTarget;
          Transform controller = ((Object) instance == (Object) null) ? (Transform) null : (state.RightHanded ? instance.leftHandTransform : instance.rightHandTransform);
          if (((Object) rigTarget2 != (Object) null) && ((Object) controller != (Object) null))
          {
            Vector3 vector3 = MenuPose.RelativePointer(rigTarget2, controller);
            for (int index = 0; index < 3; ++index)
              menuPacket.PointerPlacement[index] = Mathf.Clamp((vector3)[index], -2f, 2f);
            menuPacket.PointerVisible = pointerVisible;
          }
          menuPacket.Hover = menuPacket.PrivateInput || menuPacket.Category == (byte) 11 ? (byte) 0 : MenuPacket.PressId(hovered);
          for (int index = 0; index < MenuCatalog.Buttons.Length; ++index)
          {
            if (state.ShowsAvailable(MenuCatalog.Buttons[index]))
              MenuPacket.Set(menuPacket.Available, index);
            if (state.IsOn(index))
              MenuPacket.Set(menuPacket.Enabled, index);
            if (state.IsFavorite(index))
              MenuPacket.Set(menuPacket.Favorites, index);
          }
          byte[] first = menuPacket.Encode();
          if (!MenuPacket.SameDisplay(first, this.lastSent) || (double) unscaledTime >= (double) this.nextHeartbeat)
          {
            if (PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
            {
              [(object) "Taurus.Menu.v6"] = (object) first
            }, (Hashtable) null, (WebFlags) null))
            {
              this.lastSent = first;
              this.nextHeartbeat = unscaledTime + 1f;
            }
          }
          this.ReadPeers();
        }
        foreach (MenuNetworking.Remote peer in this.peers.Values)
          this.Animate(peer, unscaledTime);
      }
    }
    catch (Exception ex)
    {
      if ((double) Time.unscaledTime < (double) this.nextWarning)
        return;
      this.nextWarning = Time.unscaledTime + 5f;
      this.warning("Menu sharing: " + ex.Message);
    }
  }
  private void ReadPeers()
  {
    this.removed.Clear();
    foreach (int key in this.peers.Keys)
    {
      if (!PhotonNetwork.CurrentRoom.Players.ContainsKey(key))
        this.removed.Add(key);
    }
    foreach (int key in this.removed)
    {
      this.peers[key].Dispose();
      this.peers.Remove(key);
    }
    foreach (Player playerListOther in PhotonNetwork.PlayerListOthers)
    {
      object bytes;
      ((Dictionary<object, object>) playerListOther.CustomProperties).TryGetValue((object) "Taurus.Menu.v6", out bytes);
      MenuPacket packet;
      if (!MenuPacket.TryDecode(bytes as byte[], PhotonNetwork.ServerTimestamp, out packet))
      {
        MenuNetworking.Remote remote;
        if (this.peers.TryGetValue(playerListOther.ActorNumber, out remote))
        {
          remote.Dispose();
          this.peers.Remove(playerListOther.ActorNumber);
        }
      }
      else
      {
        MenuNetworking.Remote remote;
        if (!this.peers.TryGetValue(playerListOther.ActorNumber, out remote))
        {
          if (packet.Visible)
          {
            remote = this.CreateRemote();
            this.peers.Add(playerListOther.ActorNumber, remote);
          }
          else
            continue;
        }
        remote.Rig = (VRRig) null;
        foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
        {
          if (((Object) activeRig != (Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal && activeRig.Creator != null && activeRig.Creator.ActorNumber == playerListOther.ActorNumber)
          {
            remote.Rig = activeRig;
            break;
          }
        }
        bool flag1 = !MenuPacket.SameView(remote.Packet, packet);
        bool flag2 = remote.Packet == null;
        remote.Packet = packet;
        if (flag1)
        {
          remote.State.ApplyRemote(packet);
          foreach (Collider componentsInChild in remote.Menu.GetComponentsInChildren<Collider>(true))
            componentsInChild.enabled = true;
          remote.View.Refresh();
        }
        Transform transform1 = packet.Hover == (byte) 0 ? (Transform) null : remote.Menu.transform.Find(MenuPacket.PressName(packet.Hover));
        remote.View.SetHover(!((Object) transform1 != (Object) null) || !((Component) transform1).gameObject.activeSelf ? (BoxCollider) null : ((Component) transform1).GetComponent<BoxCollider>());
        if (flag1 && (packet.PrivateInput || packet.Category == (byte) 11))
        {
          foreach (Transform transform2 in remote.Menu.transform)
          {
            if (((Object) transform2).name.StartsWith("Row") || ((Object) transform2).name.StartsWith("Increment") || ((Object) transform2).name == "Keyboard" || ((Object) transform2).name == "BrowserSurface" || ((Object) transform2).name == "ButtonSearch" || ((Object) transform2).name == "ButtonPrevious" || ((Object) transform2).name == "ButtonNext")
              ((Component) transform2).gameObject.SetActive(false);
          }
          TextMesh component = ((Component) remote.Menu.transform.Find("BrowserWarning")).GetComponent<TextMesh>();
          component.text = packet.Category == (byte) 11 ? "Discord\nPrivate page" : (packet.Category == (byte) 12 ? "Soundboard\nLocal library" : "Keyboard\nPrivate input");
          ((Component) component).gameObject.SetActive(true);
        }
        if (!flag2 && packet.PressSerial != remote.LastPress && packet.Category != (byte) 11)
        {
          if (!packet.PrivateInput)
            remote.View.Pulse(MenuPacket.PressName(packet.Press));
          if (!packet.PrivateInput || packet.Press == (byte) 46)
            this.PlayClick(remote);
        }
        remote.LastPress = packet.PressSerial;
        foreach (Collider componentsInChild in remote.Menu.GetComponentsInChildren<Collider>(true))
          componentsInChild.enabled = false;
      }
    }
  }
  private MenuNetworking.Remote CreateRemote()
  {
    MenuNetworking.Remote remote = new MenuNetworking.Remote()
    {
      Menu = Object.Instantiate<GameObject>(this.prefab)
    };
    ((Object) remote.Menu).name = "TaurusSharedMenu";
    Object.DontDestroyOnLoad((Object) remote.Menu);
    foreach (Component componentsInChild in remote.Menu.GetComponentsInChildren<Transform>(true))
      componentsInChild.gameObject.layer = 2;
    foreach (Behaviour componentsInChild in remote.Menu.GetComponentsInChildren<AudioSource>(true))
      componentsInChild.enabled = false;
    remote.Audio = remote.Menu.AddComponent<AudioSource>();
    remote.Audio.playOnAwake = false;
    remote.Audio.spatialBlend = 1f;
    remote.Audio.dopplerLevel = 0.0f;
    remote.Audio.minDistance = 0.15f;
    remote.Audio.maxDistance = 8f;
    remote.Audio.rolloffMode = (AudioRolloffMode) 1;
    remote.View = new MenuView(remote.Menu.transform, remote.State, false);
    remote.Pointer = GameObject.CreatePrimitive((PrimitiveType) 0);
    ((Object) remote.Pointer).name = "TaurusSharedPointer";
    remote.Pointer.layer = 2;
    remote.Pointer.GetComponent<Collider>().enabled = false;
    Object.Destroy((Object) remote.Pointer.GetComponent<Collider>());
    remote.Pointer.transform.localScale = (Vector3.one * 0.01f);
    Object.DontDestroyOnLoad((Object) remote.Pointer);
    remote.Pointer.GetComponent<Renderer>().sharedMaterial = ((Component) this.prefab.transform.Find("Background")).GetComponent<Renderer>().sharedMaterial;
    remote.Menu.SetActive(false);
    remote.Pointer.SetActive(false);
    return remote;
  }
  private void Animate(MenuNetworking.Remote peer, float time)
  {
    MenuPacket packet = peer.Packet;
    VRMap vrMap1 = ((Object) peer.Rig == (Object) null) ? (VRMap) null : (packet.RightHanded ? peer.Rig.rightHand : peer.Rig.leftHand);
    bool flag = packet.Visible && vrMap1 != null && ((Object) vrMap1.rigTarget != (Object) null) && PhotonNetwork.ServerTimestamp - packet.Timestamp <= 6000;
    peer.Menu.SetActive(flag);
    peer.Pointer.SetActive(flag && packet.PointerVisible && !packet.PrivateInput && packet.Category != (byte) 11);
    if (!flag)
      return;
    Vector3 vector3_1;
    vector3_1 = new Vector3(packet.Placement[0], packet.Placement[1], packet.Placement[2]);
    Vector3 vector3_2;
    vector3_2 = new Vector3(packet.Placement[3], packet.Placement[4], packet.Placement[5]);
    Pose pose = MenuPose.World(vrMap1.rigTarget, new Pose(vector3_1, Quaternion.Euler(vector3_2)));
    peer.Menu.transform.SetPositionAndRotation(pose.position, pose.rotation);
    peer.Menu.transform.localScale = (((Vector3.one * packet.VrScale) * packet.Scale) * MenuStyle.DesignVrScale((int) packet.Design, (int) packet.Category));
    Transform transform = peer.Menu.transform;
    transform.position = (transform.position + peer.Menu.transform.TransformVector(peer.View.DesignSlide(time)));
    VRMap vrMap2 = packet.RightHanded ? peer.Rig.leftHand : peer.Rig.rightHand;
    if (vrMap2 != null && ((Object) vrMap2.rigTarget != (Object) null))
      peer.Pointer.transform.position = vrMap2.rigTarget.TransformPoint(new Vector3(packet.PointerPlacement[0], packet.PointerPlacement[1], packet.PointerPlacement[2]));
    else
      peer.Pointer.SetActive(false);
    this.pointerColor.SetColor("_Color", MenuPalette.Right(peer.State, time));
    peer.Pointer.GetComponent<Renderer>().SetPropertyBlock(this.pointerColor);
    peer.View.Animate(time);
    if (!peer.Audio.isPlaying || (double) time < (double) peer.SoundEndsAt)
      return;
    peer.Audio.Stop();
  }
  private void PlayClick(MenuNetworking.Remote peer)
  {
    MenuPacket packet = peer.Packet;
    string button = MenuPacket.PressName(packet.Press);
    if (button.Length == 0 || (double) packet.ClickVolume <= 0.0)
      return;
    bool flag = packet.Design == (byte) 10 && button == "DesignLogo";
    AudioClip audioClip;
    if (flag)
      audioClip = this.logoSound;
    else if (packet.Press == (byte) 46)
    {
      audioClip = this.keyboardSound;
    }
    else
    {
      if (packet.ClickSound == (byte) 0 && (packet.Design == (byte) 4 || packet.Design == (byte) 5 || packet.Design == (byte) 6 || packet.Design == (byte) 9 || packet.Design == (byte) 11) && ((Object) peer.Rig != (Object) null))
      {
        peer.Rig.PlayHandTapLocal(packet.Design == (byte) 9 ? 106 : (packet.Design == (byte) 5 || packet.Design == (byte) 11 ? 8 : 67), packet.RightHanded, packet.ClickVolume);
        return;
      }
      int index = packet.ClickSound == (byte) 0 ? MenuClickSounds.DefaultIndex((int) packet.Design) : (int) packet.ClickSound;
      audioClip = index == 0 ? ImportedAssetMenu.ClickSound(peer.State, button) ?? this.sounds[0] : this.sounds[index];
    }
    if (((Object) audioClip == (Object) null))
      return;
    peer.Audio.Stop();
    peer.Audio.PlayOneShot(audioClip, Mathf.Min(packet.ClickVolume, 1f));
    peer.SoundEndsAt = Time.unscaledTime + (flag ? audioClip.length : Mathf.Min(audioClip.length, 1f));
  }
  public void Clear()
  {
    try
    {
      if (this.room != null)
      {
        if (PhotonNetwork.LocalPlayer != null)
          PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
          {
            [(object) "Taurus.Menu.v6"] = (object) null
          }, (Hashtable) null, (WebFlags) null);
      }
    }
    catch (Exception ex)
    {
      this.warning("Could not clear menu property: " + ex.Message);
    }
    foreach (MenuNetworking.Remote remote in this.peers.Values)
      remote.Dispose();
    this.peers.Clear();
    this.room = (Room) null;
    this.lastSent = (byte[]) null;
  }
  public void Dispose() => this.Clear();
  private sealed class Remote : IDisposable
  {
    public GameObject Menu;
    public GameObject Pointer;
    public MenuView View;
    public readonly MenuState State = new MenuState();
    public MenuPacket Packet;
    public VRRig Rig;
    public int LastPress;
    public AudioSource Audio;
    public float SoundEndsAt;
    public void Dispose()
    {
      this.View?.Dispose();
      if (((Object) this.Menu != (Object) null))
        Object.Destroy((Object) this.Menu);
      if (!((Object) this.Pointer != (Object) null))
        return;
      Object.Destroy((Object) this.Pointer);
    }
  }
}
