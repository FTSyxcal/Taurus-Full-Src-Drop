
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class WorldEffectsNetworking : IDisposable
{
  public const string Property = "Taurus.Effects.v1";
  public const string PlatformsProperty = "Taurus.Platforms.v1";
  public const string OwnerUserId = "B58D3865EFC8AD15";
  private readonly GameObject prefab;
  private readonly MenuState state;
  private readonly LocalMods features;
  private readonly GunLib gun;
  private readonly Action<string> warning;
  private readonly Dictionary<int, WorldEffectsNetworking.Peer> peers = new Dictionary<int, WorldEffectsNetworking.Peer>();
  private readonly Dictionary<uint, GameObject> templates = new Dictionary<uint, GameObject>();
  private readonly List<Transform> localPlatforms = new List<Transform>();
  private readonly List<WorldEffectsPacket.Shot> pending = new List<WorldEffectsPacket.Shot>();
  private readonly List<int> removed = new List<int>();
  private readonly MaterialPropertyBlock colors = new MaterialPropertyBlock();
  private readonly Font font;
  private readonly Material tagMaterial;
  private Room room;
  private byte[] sent;
  private byte[] sentPlatforms;
  private float nextTick;
  private float heartbeat;
  private float platformHeartbeat;
  private float nextTemplateSearch;
  private float nextWarning;
  private uint serial;
  public static string UserTag(string userId)
  {
    return !string.Equals(userId, "B58D3865EFC8AD15", StringComparison.OrdinalIgnoreCase) ? "[Taurus User]" : "[Taurus Owner (ItzGame)]";
  }
  public WorldEffectsNetworking(
    GameObject prefab,
    MenuState state,
    LocalMods features,
    GunLib gun,
    Action<string> warning)
  {
    this.prefab = prefab;
    this.state = state;
    this.features = features;
    this.gun = gun;
    this.warning = warning;
    TextMesh component = ((Component) prefab.transform.Find("Row0/Label")).GetComponent<TextMesh>();
    this.font = component.font;
    this.tagMaterial = new Material(((Component) component).GetComponent<Renderer>().sharedMaterial);
    this.tagMaterial.mainTexture = this.font.material.mainTexture;
    features.SharedProjectile += new Action<GameObject, Vector3, Vector3, float>(this.OnProjectile);
  }
  private static float[] Position(Vector3 value)
  {
    return new float[3]{ value.x, value.y, value.z };
  }
  private static Vector3 Vector(float[] value, int offset = 0)
  {
    return new Vector3(value[offset], value[offset + 1], value[offset + 2]);
  }
  private void OnProjectile(GameObject source, Vector3 position, Vector3 velocity, float size)
  {
    if (!PhotonNetwork.InRoom || ((Object) source == (Object) null))
      return;
    uint key = WorldEffectsPacket.Kind(((Object) source).name);
    if (!this.templates.ContainsKey(key) && this.templates.Count < 64 )
      this.templates[key] = this.CreateTemplate(source);
    if (this.pending.Count == 6)
      this.pending.RemoveAt(0);
    this.pending.Add(new WorldEffectsPacket.Shot()
    {
      Serial = ++this.serial,
      Kind = key,
      Timestamp = PhotonNetwork.ServerTimestamp,
      Position = WorldEffectsNetworking.Position(position),
      Velocity = WorldEffectsNetworking.Position(velocity),
      Scale = Mathf.Clamp(size, 0.05f, 4f)
    });
  }
  public void Tick()
  {
    try
    {
      if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
      {
        this.Clear();
      }
      else
      {
        if (this.room != PhotonNetwork.CurrentRoom)
        {
          this.Clear();
          this.room = PhotonNetwork.CurrentRoom;
        }
        float unscaledTime = Time.unscaledTime;
        if ((double) unscaledTime >= (double) this.nextTick)
        {
          this.nextTick = unscaledTime + 0.1f;
          this.Publish(unscaledTime);
          this.ReadPeers();
        }
        foreach (WorldEffectsNetworking.Peer peer in this.peers.Values)
          this.Animate(peer);
      }
    }
    catch (Exception ex)
    {
      if ((double) Time.unscaledTime < (double) this.nextWarning)
        return;
      this.nextWarning = Time.unscaledTime + 10f;
      this.warning("World sharing: " + ex.Message);
    }
  }
  private WorldEffectsPacket Packet()
  {
    float unscaledTime = Time.unscaledTime;
    return new WorldEffectsPacket()
    {
      Timestamp = PhotonNetwork.ServerTimestamp,
      Design = (byte) this.state.Design,
      Background = MenuPalette.Pack(MenuPalette.Background(this.state, unscaledTime)),
      Left = MenuPalette.Pack(MenuPalette.Left(this.state, unscaledTime)),
      Right = MenuPalette.Pack(MenuPalette.Right(this.state, unscaledTime))
    };
  }
  private void Publish(float time)
  {
    this.features.SharedPlatforms(this.localPlatforms);
    WorldEffectsPacket worldEffectsPacket1 = this.Packet();
    if (this.gun.IsVisible)
    {
      Vector3 visualOrigin = this.gun.VisualOrigin;
      Vector3 targetPoint = this.gun.TargetPoint;
      worldEffectsPacket1.Gun = new float[6]
      {
        visualOrigin.x,
        visualOrigin.y,
        visualOrigin.z,
        targetPoint.x,
        targetPoint.y,
        targetPoint.z
      };
      worldEffectsPacket1.Locked = ((Object) this.gun.LockedBody != (Object) null);
      worldEffectsPacket1.Pressed = this.gun.IsPressed;
    }
    worldEffectsPacket1.Shots = this.pending.ToArray();
    if (worldEffectsPacket1.Gun == null && worldEffectsPacket1.Shots.Length == 0 && this.localPlatforms.Count == 0)
      worldEffectsPacket1.Background = worldEffectsPacket1.Left = worldEffectsPacket1.Right = 0U;
    byte[] second1 = worldEffectsPacket1.Encode();
    if ((double) time >= (double) this.heartbeat || !WorldEffectsPacket.Same(this.sent, second1))
    {
      if (PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
      {
        [(object) "Taurus.Effects.v1"] = (object) second1
      }, (Hashtable) null, (WebFlags) null))
      {
        this.sent = second1;
        this.heartbeat = time + 2f;
        this.pending.Clear();
      }
    }
    WorldEffectsPacket worldEffectsPacket2 = new WorldEffectsPacket()
    {
      Timestamp = worldEffectsPacket1.Timestamp,
      Platforms = new float[Mathf.Min(this.localPlatforms.Count, 74)][]
    };
    for (int index = 0; index < worldEffectsPacket2.Platforms.Length; ++index)
    {
      Transform localPlatform = this.localPlatforms[index];
      Vector3 position = localPlatform.position;
      Quaternion rotation = localPlatform.rotation;
      worldEffectsPacket2.Platforms[index] = new float[7]
      {
        position.x,
        position.y,
        position.z,
        rotation.x,
        rotation.y,
        rotation.z,
        rotation.w
      };
    }
    byte[] second2 = worldEffectsPacket2.Encode();
    if ((double) time < (double) this.platformHeartbeat && WorldEffectsPacket.Same(this.sentPlatforms, second2))
      return;
    if (!PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable()
    {
      [(object) "Taurus.Platforms.v1"] = (object) second2
    }, (Hashtable) null, (WebFlags) null))
      return;
    this.sentPlatforms = second2;
    this.platformHeartbeat = time + 2f;
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
      object bytes1;
      ((Dictionary<object, object>) playerListOther.CustomProperties).TryGetValue((object) "Taurus.Effects.v1", out bytes1);
      WorldEffectsPacket packet1;
      bool flag = WorldEffectsPacket.TryDecode(bytes1 as byte[], PhotonNetwork.ServerTimestamp, out packet1);
      if (!flag)
      {
        object bytes2;
        ((Dictionary<object, object>) playerListOther.CustomProperties).TryGetValue((object) "Taurus.Menu.v6", out bytes2);
        flag = MenuPacket.TryDecode(bytes2 as byte[], PhotonNetwork.ServerTimestamp, out MenuPacket _);
      }
      if (!flag)
      {
        WorldEffectsNetworking.Peer peer;
        if (this.peers.TryGetValue(playerListOther.ActorNumber, out peer))
        {
          peer.Dispose();
          this.peers.Remove(playerListOther.ActorNumber);
        }
      }
      else
      {
        WorldEffectsNetworking.Peer peer;
        if (!this.peers.TryGetValue(playerListOther.ActorNumber, out peer))
        {
          if (this.peers.Count < 20)
          {
            peer = new WorldEffectsNetworking.Peer()
            {
              Player = playerListOther,
              Gun = this.gun.CreateRemoteVisual(),
              Root = new GameObject("TaurusSharedEffects")
            };
            Object.DontDestroyOnLoad((Object) peer.Root);
            peer.Tag = new GameObject("TaurusUserTag").AddComponent<TextMesh>();
            peer.Tag.font = this.font;
            peer.Tag.fontSize = 0;
            peer.Tag.characterSize = 0.115200005f / (float) this.font.fontSize;
            ((Component) peer.Tag).transform.SetParent(peer.Root.transform, false);
            peer.Tag.anchor = (TextAnchor) 4;
            peer.Tag.alignment = (TextAlignment) 1;
            peer.Tag.richText = false;
            ((Component) peer.Tag).GetComponent<Renderer>().sharedMaterial = this.tagMaterial;
            this.peers.Add(playerListOther.ActorNumber, peer);
          }
          else
            continue;
        }
        peer.Rig = (VRRig) null;
        foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
        {
          if (((Object) activeRig != (Object) null) && !activeRig.isOfflineVRRig && activeRig.Creator != null && activeRig.Creator.ActorNumber == playerListOther.ActorNumber)
          {
            peer.Rig = activeRig;
            break;
          }
        }
        string str = WorldEffectsNetworking.UserTag(peer.Rig?.Creator?.UserId ?? playerListOther.UserId);
        if (peer.Tag.text != str)
          peer.Tag.text = str;
        if (packet1 != null && (peer.Packet == null || peer.Packet.Timestamp != packet1.Timestamp))
        {
          peer.Packet = packet1;
          peer.Gun.SetDesign((int) packet1.Design);
          peer.Gun.SetColors(MenuPalette.Unpack(packet1.Left), MenuPalette.Unpack(packet1.Right));
          foreach (WorldEffectsPacket.Shot shot in packet1.Shots)
          {
            if ((!peer.ReceivedShot || (int) shot.Serial - (int) peer.LastShot > 0) && PhotonNetwork.ServerTimestamp - shot.Timestamp < 2000)
            {
              peer.LastShot = shot.Serial;
              peer.ReceivedShot = true;
              this.Spawn(peer, shot);
            }
          }
          foreach (WorldEffectsNetworking.Platform platform in peer.Platforms)
            this.Paint(platform, packet1);
        }
        else if (packet1 == null)
        {
          peer.Packet = (WorldEffectsPacket) null;
          peer.Gun.Hide();
        }
        object bytes3;
        ((Dictionary<object, object>) playerListOther.CustomProperties).TryGetValue((object) "Taurus.Platforms.v1", out bytes3);
        WorldEffectsPacket packet2;
        if (WorldEffectsPacket.TryDecode(bytes3 as byte[], PhotonNetwork.ServerTimestamp, out packet2) && packet2.Platforms != null)
        {
          if (packet2.Timestamp != peer.PlatformTimestamp)
          {
            peer.PlatformTimestamp = packet2.Timestamp;
            this.SetPlatforms(peer, packet2.Platforms);
          }
        }
        else
          this.SetPlatforms(peer, Array.Empty<float[]>());
      }
    }
  }
  private void SetPlatforms(WorldEffectsNetworking.Peer peer, float[][] poses)
  {
    while (peer.Platforms.Count > poses.Length)
    {
      int index = peer.Platforms.Count - 1;
      Object.Destroy((Object) peer.Platforms[index].Root);
      peer.Platforms.RemoveAt(index);
    }
    while (peer.Platforms.Count < poses.Length)
    {
      GameObject gameObject = MenuPlatform.Create(this.prefab, Vector3.zero, Quaternion.identity);
      ((Object) gameObject).name = "TaurusSharedPlatform";
      gameObject.transform.SetParent(peer.Root.transform, true);
      foreach (Collider componentsInChild in gameObject.GetComponentsInChildren<Collider>())
      {
        componentsInChild.enabled = false;
        Object.Destroy((Object) componentsInChild);
      }
      WorldEffectsNetworking.Platform platform = new WorldEffectsNetworking.Platform()
      {
        Root = gameObject,
        Body = ((Component) gameObject.transform.Find("Background")).GetComponent<Renderer>(),
        First = ((Component) gameObject.transform.Find("Outline0")).GetComponent<Renderer>(),
        Second = ((Component) gameObject.transform.Find("Outline1")).GetComponent<Renderer>()
      };
      peer.Platforms.Add(platform);
      if (peer.Packet != null)
        this.Paint(platform, peer.Packet);
    }
    for (int index = 0; index < poses.Length; ++index)
    {
      float[] pose = poses[index];
      peer.Platforms[index].Root.transform.SetPositionAndRotation(WorldEffectsNetworking.Vector(pose), new Quaternion(pose[3], pose[4], pose[5], pose[6]));
    }
  }
  private void Paint(WorldEffectsNetworking.Platform platform, WorldEffectsPacket packet)
  {
    this.colors.Clear();
    this.colors.SetColor("_Color", MenuPalette.Unpack(packet.Background));
    platform.Body.SetPropertyBlock(this.colors);
    this.colors.Clear();
    this.colors.SetColor("_LeftColor", MenuPalette.Unpack(packet.Left));
    this.colors.SetColor("_RightColor", MenuPalette.Unpack(packet.Right));
    this.colors.SetFloat("_Speed", 0.0f);
    platform.First.SetPropertyBlock(this.colors);
    platform.Second.SetPropertyBlock(this.colors);
  }
  private GameObject CreateTemplate(GameObject source)
  {
    GameObject template = new GameObject("TaurusProjectileVisual");
    template.SetActive(false);
    Object.DontDestroyOnLoad((Object) template);
    int num = 0;
    foreach (MeshFilter meshFilter in ((Object) source == (Object) null) ? Array.Empty<MeshFilter>() : source.GetComponentsInChildren<MeshFilter>(true))
    {
      MeshRenderer meshRenderer;
      if (!((Object) meshFilter.sharedMesh == (Object) null) && ((Component) meshFilter).TryGetComponent<MeshRenderer>(out meshRenderer) && num < 12 && ((Renderer) meshRenderer).enabled)
      {
        bool flag = true;
        for (Transform transform = ((Component) meshFilter).transform; ((Object) transform != (Object) source.transform); transform = transform.parent)
        {
          if (!((Component) transform).gameObject.activeSelf)
          {
            flag = false;
            break;
          }
        }
        if (flag)
        {
          ++num;
          GameObject gameObject = new GameObject("Mesh", new Type[2]
          {
            typeof (MeshFilter),
            typeof (MeshRenderer)
          });
          gameObject.transform.SetParent(template.transform, false);
          gameObject.transform.localPosition = source.transform.InverseTransformPoint(((Component) meshFilter).transform.position);
          gameObject.transform.localRotation = (Quaternion.Inverse(source.transform.rotation) * ((Component) meshFilter).transform.rotation);
          Vector3 lossyScale1 = source.transform.lossyScale;
          Vector3 lossyScale2 = ((Component) meshFilter).transform.lossyScale;
          gameObject.transform.localScale = new Vector3(lossyScale2.x / Mathf.Max(0.0001f, Mathf.Abs(lossyScale1.x)), lossyScale2.y / Mathf.Max(0.0001f, Mathf.Abs(lossyScale1.y)), lossyScale2.z / Mathf.Max(0.0001f, Mathf.Abs(lossyScale1.z)));
          gameObject.GetComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;
          ((Renderer) gameObject.GetComponent<MeshRenderer>()).sharedMaterials = ((Renderer) meshRenderer).sharedMaterials;
          ((Renderer) meshRenderer).GetPropertyBlock(this.colors);
          ((Renderer) gameObject.GetComponent<MeshRenderer>()).SetPropertyBlock(this.colors);
        }
      }
    }
    if (template.transform.childCount == 0)
    {
      GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 0);
      primitive.transform.SetParent(template.transform, false);
      primitive.transform.localScale = (Vector3.one * 0.12f);
      primitive.GetComponent<Collider>().enabled = false;
      Object.Destroy((Object) primitive.GetComponent<Collider>());
      primitive.GetComponent<Renderer>().sharedMaterial = ((Component) this.prefab.transform.Find("Background")).GetComponent<Renderer>().sharedMaterial;
    }
    return template;
  }
  private void Spawn(WorldEffectsNetworking.Peer peer, WorldEffectsPacket.Shot shot)
  {
    GameObject template;
    if (!this.templates.TryGetValue(shot.Kind, out template) && (double) Time.unscaledTime >= (double) this.nextTemplateSearch)
    {
      this.nextTemplateSearch = Time.unscaledTime + 2f;
      foreach (SlingshotProjectile slingshotProjectile in Resources.FindObjectsOfTypeAll<SlingshotProjectile>())
      {
        uint key = WorldEffectsPacket.Kind(((Object) slingshotProjectile).name);
        if (!this.templates.ContainsKey(key) && this.templates.Count < 64 )
          this.templates[key] = this.CreateTemplate(((Component) slingshotProjectile).gameObject);
      }
      this.templates.TryGetValue(shot.Kind, out template);
    }
    if (((Object) template == (Object) null) && !this.templates.TryGetValue(0U, out template))
    {
      template = this.CreateTemplate((GameObject) null);
      this.templates[0U] = template;
    }
    WorldEffectsNetworking.Projectile projectile1 = (WorldEffectsNetworking.Projectile) null;
    foreach (WorldEffectsNetworking.Projectile projectile2 in peer.Projectiles)
    {
      if (!projectile2.Root.activeSelf && (int) projectile2.Shot.Kind == (int) shot.Kind)
      {
        projectile1 = projectile2;
        break;
      }
    }
    if (projectile1 == null)
    {
      if (peer.Projectiles.Count >= 32 )
      {
        WorldEffectsNetworking.Projectile projectile3 = peer.Projectiles[0];
        peer.Projectiles.RemoveAt(0);
        Object.Destroy((Object) projectile3.Root);
      }
      projectile1 = new WorldEffectsNetworking.Projectile()
      {
        Root = Object.Instantiate<GameObject>(template)
      };
      projectile1.Root.transform.SetParent(peer.Root.transform, false);
      peer.Projectiles.Add(projectile1);
    }
    projectile1.Shot = shot;
    projectile1.Root.transform.localScale = (Vector3.one * shot.Scale);
    projectile1.Root.SetActive(true);
  }
  private void Animate(WorldEffectsNetworking.Peer peer)
  {
    bool flag = ((Object) peer.Rig != (Object) null) && peer.Rig.head != null && ((Object) peer.Rig.head.rigTarget != (Object) null);
    ((Component) peer.Tag).gameObject.SetActive(flag);
    if (flag)
    {
      ((Component) peer.Tag).transform.position = (peer.Rig.head.rigTarget.position + (Vector3.up * 0.42f));
      Transform transform = ((Component) GorillaTagger.Instance?.headCollider)?.transform;
      if (((Object) transform != (Object) null))
        ((Component) peer.Tag).transform.rotation = Quaternion.LookRotation((((Component) peer.Tag).transform.position - transform.position));
    }
    if (flag && peer.Packet?.Gun != null)
    {
      float num = peer.GunReady ? 1f - Mathf.Exp((float) (-(double) Time.unscaledDeltaTime * 25.0)) : 1f;
      peer.GunStart = Vector3.Lerp(peer.GunStart, WorldEffectsNetworking.Vector(peer.Packet.Gun), num);
      peer.GunEnd = Vector3.Lerp(peer.GunEnd, WorldEffectsNetworking.Vector(peer.Packet.Gun, 3), num);
      peer.GunReady = true;
      peer.Gun.ShowRemote(peer.GunStart, peer.GunEnd, peer.Packet.Locked);
    }
    else
    {
      peer.Gun.Hide();
      peer.GunReady = false;
    }
    foreach (WorldEffectsNetworking.Projectile projectile in peer.Projectiles)
    {
      float num = (float) (PhotonNetwork.ServerTimestamp - projectile.Shot.Timestamp) * (1f / 1000f);
      projectile.Root.SetActive(flag && (double) num >= 0.0 && (double) num < 2.0);
      if (projectile.Root.activeSelf)
        projectile.Root.transform.position = (WorldEffectsNetworking.Vector(projectile.Shot.Position) + (WorldEffectsNetworking.Vector(projectile.Shot.Velocity) * num));
    }
  }
  public void Clear()
  {
    if (this.room != null)
    {
      try
      {
        Player localPlayer = PhotonNetwork.LocalPlayer;
        if (localPlayer != null)
          localPlayer.SetCustomProperties(new Hashtable()
          {
            [(object) "Taurus.Effects.v1"] = (object) null,
            [(object) "Taurus.Platforms.v1"] = (object) null
          }, (Hashtable) null, (WebFlags) null);
      }
      catch
      {
      }
    }
    foreach (WorldEffectsNetworking.Peer peer in this.peers.Values)
      peer.Dispose();
    this.peers.Clear();
    this.pending.Clear();
    this.room = (Room) null;
    this.sent = (byte[]) null;
    this.sentPlatforms = (byte[]) null;
  }
  public void Dispose()
  {
    this.features.SharedProjectile -= new Action<GameObject, Vector3, Vector3, float>(this.OnProjectile);
    this.Clear();
    foreach (Object @object in this.templates.Values)
      Object.Destroy(@object);
    this.templates.Clear();
    Object.Destroy((Object) this.tagMaterial);
  }
  private sealed class Platform
  {
    public GameObject Root;
    public Renderer Body;
    public Renderer First;
    public Renderer Second;
  }
  private sealed class Projectile
  {
    public GameObject Root;
    public WorldEffectsPacket.Shot Shot;
  }
  private sealed class Peer : IDisposable
  {
    public Player Player;
    public GameObject Root;
    public VRRig Rig;
    public TextMesh Tag;
    public GunLib Gun;
    public WorldEffectsPacket Packet;
    public int PlatformTimestamp;
    public readonly List<WorldEffectsNetworking.Platform> Platforms = new List<WorldEffectsNetworking.Platform>();
    public readonly List<WorldEffectsNetworking.Projectile> Projectiles = new List<WorldEffectsNetworking.Projectile>();
    public uint LastShot;
    public bool ReceivedShot;
    public Vector3 GunStart;
    public Vector3 GunEnd;
    public bool GunReady;
    public void Dispose()
    {
      this.Gun?.Dispose();
      if (!((Object) this.Root != (Object) null))
        return;
      Object.Destroy((Object) this.Root);
    }
  }
}
