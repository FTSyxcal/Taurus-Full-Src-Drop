
using BepInEx;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class DetectedPorts
{
  public static readonly string[] Names = new string[187]
  {
    "Antiban Join",
    "Stump Kick Antiban",
    "Type Code",
    "Join Room",
    "Set Name",
    "Anti Mod Checker",
    "Stump Kick All",
    "Freeze Gun",
    "Freeze All",
    "Crash Gun",
    "Crash All",
    "Set Master",
    "Ban Gun",
    "Ban All",
    "Ban Self",
    "Lag Gun",
    "Lag All",
    "Lag Aura",
    "Lag On Touch",
    "Destroy Player Gun",
    "Destroy All Players",
    "Destroy Self",
    "Quarantine Gun",
    "Quarantine All",
    "Quarantine Aura",
    "Quarantine On Touch",
    "Leaderboard Quarantine",
    "Mute Gun",
    "Mute All",
    "Mute Aura",
    "Mute On Touch",
    "Leaderboard Mute",
    "Ghost Gun",
    "Ghost All",
    "Ghost Aura",
    "Ghost On Touch",
    "Leaderboard Ghost",
    "Unghost Gun",
    "Unghost All",
    "Unghost Aura",
    "Unghost On Touch",
    "Spam Ghost Gun",
    "Spam Ghost All",
    "Spam Ghost Aura",
    "Spam Ghost On Touch",
    "Crash Leaderboard",
    "Isolate Gun",
    "Isolate All",
    "Isolate Aura",
    "Isolate On Touch",
    "Isolate Others",
    "Change Name Gun",
    "Change Name All",
    "Change Name Aura",
    "Change Name On Touch",
    "Bypass Mod Checkers Gun",
    "Bypass Mod Checkers All",
    "Bypass Mod Checkers Aura",
    "Bypass Mod Checkers On Touch",
    "Break Mod Checkers Gun",
    "Break Mod Checkers All",
    "Break Mod Checkers Aura",
    "Break Mod Checkers On Touch",
    "Gamemode Include Gun",
    "Gamemode Include All",
    "Gamemode Include Aura",
    "Gamemode Include On Touch",
    "Gamemode Exclude Gun",
    "Gamemode Exclude All",
    "Gamemode Exclude Aura",
    "Gamemode Exclude On Touch",
    "Break Network Triggers",
    "Kick Network Triggers",
    "Spaz Gamemode",
    "Break Gamemode",
    "Change Gamemode to None",
    "Change Gamemode to Count",
    "Change Gamemode to Casual",
    "Change Gamemode to Infection",
    "Change Gamemode to Competitive Infection",
    "Change Gamemode to Super Infection",
    "Change Gamemode to Super Casual",
    "Change Gamemode to Hunt",
    "Change Gamemode to Paintbrawl",
    "Change Gamemode to Ambush",
    "Change Gamemode to Ghost Tag",
    "Change Gamemode to Guardian",
    "Change Gamemode to Freeze Tag",
    "Change Gamemode to Prop Hunt",
    "Change Gamemode to Custom",
    "Switch to Modded Gamemode",
    "Spawn Blue Lucy (M)",
    "Spawn Red Lucy (M)",
    "Insta Spawn Blue Lucy (M)",
    "Insta Spawn Red Lucy (M)",
    "Despawn Lucy (M)",
    "Normal Lucy (M)",
    "Slow Lucy (M)",
    "Fast Lucy (M)",
    "Freeze Lucy (M)",
    "Lucy Float Self (M)",
    "Lucy Float Gun (M)",
    "Lucy Float Touch (M)",
    "Lucy Float On Touch (M)",
    "Lucy Grab (M)",
    "Lucy Move Gun (M)",
    "Lucy Orbit (M)",
    "Lucy Target Self (M)",
    "Lucy Target Gun (M)",
    "Lucy Target Spaz (M)",
    "GodMode Self (M)",
    "GodMode Gun (M)",
    "GodMode All (M)",
    "UnGodMode Self (M)",
    "UnGodMode Gun (M)",
    "UnGodMode All (M)",
    "Break Map Lighting (M)",
    "Ghost Monkey",
    "Invisible Monkey",
    "Become Daisy09",
    "Play Daisy09 Song",
    "Tag Self",
    "Tag Gun",
    "Tag All",
    "Tag Aura",
    "Head Twirl X",
    "Head Twirl Y",
    "Head Twirl Z",
    "Body Twirl",
    "Ghost Monke",
    "Invis Monke",
    "Voice Hijack",
    "Keep Voice Hijack Target",
    "Hear Self",
    "Voice Hijack Gun",
    "Grab Fling Gun",
    "Grab Fling All",
    "Close Room",
    "Open Room",
    "Bypass Auto Mute",
    "Kick Gun",
    "Kick All",
    "Anti Report",
    "Anti Report Visualizer",
    "Flush RPC",
    "Unlimited RPC",
    "Become Lucy",
    "Become PBBV",
    "Snowball Spammer",
    "Growing Snowball Spammer",
    "Water Balloon Spammer",
    "Lava Rock Spammer",
    "Gift Bucket Spammer",
    "Science Candy Spammer",
    "Fish Food Spammer",
    "Apple Spammer",
    "Trick or Treat Spammer",
    "Voting Rock Spammer",
    "Book Spammer",
    "Coin Spammer",
    "Egg Spammer",
    "Ice Cream Spammer",
    "Hot Dog Spammer",
    "Fireworks Spammer",
    "Papers Spammer",
    "Ice Cream Scoop Spammer",
    "Chips Spammer",
    "Salsa Spammer",
    "Apple Pie Spammer",
    "Mashed Potato Spammer",
    "Berry Pie Spammer",
    "Layer Dip Spammer",
    "Pumpkin Pie Spammer",
    "Stuffing Spammer",
    "Corn Spammer",
    "Turkey Leg Spammer",
    "Football Spammer",
    "Popcorn Ball Spammer",
    "Cracked Plate Spammer",
    "Bonfire Spammer",
    "Walnut Spammer",
    "Hot Cocoa Spammer",
    "Pillow Spammer",
    "Cake Pieces Spammer",
    "Balloon Animal Spammer",
    "Random Projectile Spammer",
    "Slingshot Spammer"
  };
  private readonly MenuState state;
  private readonly Dictionary<VRRig, int> viewIdArchive = new Dictionary<VRRig, int>();
  private readonly Queue<float> sendTimes = new Queue<float>();
  private readonly HashSet<string> fakeModKeys;
  private int sendCap = 480;
  private string customName = "Detectable";
  private string antibanCode = "";
  private bool joining;
  private string captureMode;
  private float captureArmedAt;
  private bool isolateOthers;
  private bool moddedGamemode;
  private bool orbitLucy;
  private float orbitAngle;
  private float customPropertyDelay;
  private float destroyGunDelay;
  private float spazGamemodeDelay;
  private float antiDelay;
  private float lagDebounce;
  private readonly Dictionary<int, float> freezeUntil = new Dictionary<int, float>();
  private readonly Dictionary<int, float> rpcNext = new Dictionary<int, float>();
  private readonly Dictionary<int, float> destroyNext = new Dictionary<int, float>();
  private Vector3 invisReturnPos;
  private bool invisHasPos;
  private Vector3 nullReturnPos;
  private bool nullHasPos;
  private const float TwirlSpeed = 8f;
  private GameObject daisyGO;
  private AudioSource daisySource;
  private AudioClip daisyClip;
  private bool daisyLoading;
  private Recorder daisyRec;
  private AudioClip daisyMic;
  private AudioClip daisyPrevClip;
  private Recorder.InputSourceType daisyPrevSource;
  private bool daisyPrevLoop;
  private bool daisyPrevDetect;
  private NetPlayer hijackTarget;
  private bool hijackKeep;
  private float hijackAutoDelay;
  private float hijackReassertDelay;
  private float hijackClaimTime;
  private float hijackNoSlotAt;
  private bool hijackReassert1;
  private bool hijackReassert2;
  private readonly Dictionary<int, float> hijackFirstSeen = new Dictionary<int, float>();
  private readonly List<VRRig> flingQueue = new List<VRRig>();
  private int flingIdx;
  private float flingNextAt;
  private GameObject antiReportSphere;
  private Material antiReportMaterial;
  private static MethodInfo runViewUpdate;
  private float tagAuraDelay;
  private float muteMaintenanceAt;
  private float prevLoudness;
  private float silenceStartedAt = -1f;
  private bool recordingRestarted;
  private bool tagBusy;
  private float tagReportAt;
  private static readonly Dictionary<string, string> projectileSpammers = new Dictionary<string, string>()
  {
    {
      "Snowball Spammer",
      "SnowballRightAnchor"
    },
    {
      "Growing Snowball Spammer",
      "GrowingSnowballRightAnchor"
    },
    {
      "Water Balloon Spammer",
      "WaterBalloonRightAnchor"
    },
    {
      "Lava Rock Spammer",
      "LavaRockAnchor"
    },
    {
      "Gift Bucket Spammer",
      "BucketGiftFunctionalAnchor_Right"
    },
    {
      "Science Candy Spammer",
      "ScienceCandyRightAnchor"
    },
    {
      "Fish Food Spammer",
      "FishFoodRightAnchor"
    },
    {
      "Apple Spammer",
      "AppleRightAnchor"
    },
    {
      "Trick or Treat Spammer",
      "TrickTreatFunctionalAnchorRIGHT Variant"
    },
    {
      "Voting Rock Spammer",
      "VotingRockAnchor_RIGHT"
    },
    {
      "Book Spammer",
      "BookRightAnchor"
    },
    {
      "Coin Spammer",
      "CoinRightAnchor"
    },
    {
      "Egg Spammer",
      "EggRightHand_Anchor Variant"
    },
    {
      "Ice Cream Spammer",
      "IceCreamRightAnchor"
    },
    {
      "Hot Dog Spammer",
      "HotDogRightAnchor"
    },
    {
      "Fireworks Spammer",
      "Fireworks_Anchor Variant_Right Hand"
    },
    {
      "Papers Spammer",
      "Papers_Anchor Variant_Right Hand"
    },
    {
      "Ice Cream Scoop Spammer",
      "IceCreamScoopRightAnchor"
    },
    {
      "Chips Spammer",
      "ChipsRightAnchor"
    },
    {
      "Salsa Spammer",
      "SalsaRightAnchor"
    },
    {
      "Apple Pie Spammer",
      "ApplePieRightAnchor"
    },
    {
      "Mashed Potato Spammer",
      "GrowingMashedPotatoRightAnchor"
    },
    {
      "Berry Pie Spammer",
      "BerryPieRightAnchor"
    },
    {
      "Layer Dip Spammer",
      "LayerDipRightAnchor"
    },
    {
      "Pumpkin Pie Spammer",
      "PumpkinPieRightAnchor"
    },
    {
      "Stuffing Spammer",
      "GrowingStuffingRightAnchor"
    },
    {
      "Corn Spammer",
      "CornRightAnchor"
    },
    {
      "Turkey Leg Spammer",
      "TurkeyLegRightAnchor"
    },
    {
      "Football Spammer",
      "GoalpostFootball_Anchor_RightHand"
    },
    {
      "Popcorn Ball Spammer",
      "PopcornBall_Anchor_Right"
    },
    {
      "Cracked Plate Spammer",
      "CrackedPlate_Lump_Projectile_Anchor_RIGHT"
    },
    {
      "Bonfire Spammer",
      "PortableBonfire_Sticks_Anchor_RightHand"
    },
    {
      "Walnut Spammer",
      "Walnut_Anchor_Right"
    },
    {
      "Hot Cocoa Spammer",
      "HotCocoaCup_Anchor_RIGHT"
    },
    {
      "Pillow Spammer",
      "PillowProjectile_Anchor_RIGHT"
    },
    {
      "Cake Pieces Spammer",
      "CakePieces_Anchor_RIGHT"
    },
    {
      "Balloon Animal Spammer",
      "BalloonAnimalProjectileAnchor_RIGHT"
    },
    {
      "Slingshot Spammer",
      "SlingshotProjectile"
    }
  };
  private readonly Dictionary<string, SnowballThrowable> projectileCache = new Dictionary<string, SnowballThrowable>();
  private int projIncrement;
  private readonly System.Random projRandom = new System.Random();
  private readonly HashSet<string> projMissed = new HashSet<string>();
  private readonly Dictionary<string, float> projRetryAt = new Dictionary<string, float>();
  private static FieldInfo weaponPrefabField;
  private static Type projectileTrackerType;
  private static MethodInfo addIncrementMethod;
  private readonly Dictionary<string, float> projCsNext = new Dictionary<string, float>();
  public DetectedPorts(MenuState state)
  {
    this.state = state;
    this.fakeModKeys = new HashSet<string>((IEnumerable<string>) new string[52]
    {
      "Poison",
      "ShibaGT",
      "MonkeMod",
      "Symex",
      "Azalea",
      "KIDMOD",
      "StupidTemplate",
      "Violet",
      "Frostbite",
      "Nexus",
      "ObsidianMC",
      "genesis",
      "elux",
      "VioletFreeUser",
      "Hidden Menu",
      "void",
      "cronos",
      "ORBIT",
      "Violet On Top",
      "ElixirMenu",
      "Elixir",
      "VioletPaidUser",
      "EmoteWheel",
      "MistUser",
      "Untitled",
      "void_menu_open",
      "dark",
      "oblivionuser",
      "eyerock reborn",
      "asteroidlite",
      "cokecosmetics",
      "ØƦƁƖƬ",
      "Atlas",
      "Euphoric",
      "CurrentEmote",
      "Explicit",
      "envision build transform flourish",
      "https://chqser.lol/",
      "y u lookin in here wei",
      "explicit",
      "Track Track Track Sahur",
      "R3_MENU",
      "ZlothYDances",
      "1y u lookin in here weirdo",
      "tsbibiscool",
      "WalkSimulator",
      "AceUtilsPad",
      "Chud menu",
      "Lexi's WalkSimulator",
      "67ur actually so weird for making a mod checker LMAO get a life kid9",
      "seralyth networkmenu",
      "DTASLOI"
    });
  }
  public void Changed(MenuEntry entry)
  {
    if (entry == null || this.RunAction(entry.Name))
      return;
    bool flag = this.state.IsOn(entry.Name);
    string name = entry.Name;
    if (name == null)
      return;
    switch (name.Length)
    {
      case 9:
        if (!(name == "Hear Self"))
          return;
        try
        {
          Recorder talkRecorder = DetectedPorts.GetTalkRecorder();
          if (!((Object) talkRecorder != (Object) null))
            return;
          talkRecorder.DebugEchoMode = flag;
          return;
        }
        catch
        {
          return;
        }
      case 10:
        if (!(name == "Lucy Orbit"))
          return;
        this.orbitLucy = flag && DetectedPorts.IsMaster();
        return;
      case 11:
        switch (name[0])
        {
          case 'A':
            if (!(name == "Anti Report"))
              return;
            if (flag)
              return;
            try
            {
              foreach (GorillaPlayerScoreboardLine playerScoreboardLine in Object.FindObjectsByType<GorillaPlayerScoreboardLine>((FindObjectsInactive) 0, (FindObjectsSortMode) 0))
              {
                if (((Object) playerScoreboardLine != (Object) null))
                  ((Behaviour) playerScoreboardLine).enabled = true;
              }
            }
            catch
            {
            }
            this.ClearAntiReportSphere();
            return;
          case 'G':
            if (!(name == "Ghost Monke"))
              return;
            if (flag)
              return;
            try
            {
              if (!((Object) VRRig.LocalRig != (Object) null))
                return;
              ((Behaviour) VRRig.LocalRig).enabled = true;
              return;
            }
            catch
            {
              return;
            }
          case 'I':
            if (!(name == "Invis Monke"))
              return;
            if (flag)
            {
              try
              {
                this.nullReturnPos = ((Component) VRRig.LocalRig).transform.position;
                this.nullHasPos = true;
                return;
              }
              catch
              {
                return;
              }
            }
            else
            {
              try
              {
                if (((Object) VRRig.LocalRig != (Object) null))
                {
                  ((Behaviour) VRRig.LocalRig).enabled = true;
                  if (this.nullHasPos)
                    ((Component) VRRig.LocalRig).transform.position = this.nullReturnPos;
                }
              }
              catch
              {
              }
              this.nullHasPos = false;
              return;
            }
          default:
            return;
        }
      case 12:
        switch (name[0])
        {
          case 'G':
            if (!(name == "Ghost Monkey"))
              return;
            if (flag)
              return;
            try
            {
              if (!((Object) VRRig.LocalRig != (Object) null))
                return;
              ((Behaviour) VRRig.LocalRig).enabled = true;
              return;
            }
            catch
            {
              return;
            }
          case 'V':
            if (!(name == "Voice Hijack") || flag)
              return;
            this.DisableTalkThrough();
            return;
          default:
            return;
        }
      case 13:
        if (!(name == "Unlimited RPC"))
          return;
        this.UnlimitedRPCs(flag);
        return;
      case 14:
        if (!(name == "Break Gamemode"))
          return;
        this.BreakGamemodeOn(flag);
        return;
      case 15:
        if (!(name == "Change Name All") || !flag)
          return;
        this.ChangeNameAll();
        return;
      case 16 :
        switch (name[0])
        {
          case 'I':
            if (!(name == "Invisible Monkey"))
              return;
            if (flag)
            {
              try
              {
                this.invisReturnPos = ((Component) VRRig.LocalRig).transform.position;
                this.invisHasPos = true;
                return;
              }
              catch
              {
                return;
              }
            }
            else
            {
              try
              {
                if (((Object) VRRig.LocalRig != (Object) null))
                {
                  ((Behaviour) VRRig.LocalRig).enabled = true;
                  if (this.invisHasPos)
                    ((Component) VRRig.LocalRig).transform.position = this.invisReturnPos;
                }
              }
              catch
              {
              }
              this.invisHasPos = false;
              return;
            }
          case 'L':
            if (!(name == "Leaderboard Mute"))
              return;
            break;
          default:
            return;
        }
        break;
      case 17:
        switch (name[0])
        {
          case 'C':
            if (!(name == "Crash Leaderboard"))
              return;
            break;
          case 'L':
            if (!(name == "Leaderboard Ghost"))
              return;
            break;
          case 'P':
            if (!(name == "Play Daisy09 Song"))
              return;
            if (flag)
            {
              this.DaisyStart();
              return;
            }
            this.DaisyStop();
            return;
          default:
            return;
        }
        break;
      case 18:
        return;
      case 19:
        return;
      case 20:
        return;
      case 21:
        return;
      case 22:
        switch (name[0])
        {
          case 'A':
            if (!(name == "Anti Report Visualizer") || flag)
              return;
            this.ClearAntiReportSphere();
            return;
          case 'L':
            if (!(name == "Leaderboard Quarantine"))
              return;
            break;
          default:
            return;
        }
        break;
      case 23:
        return;
      case 24:
        if (!(name == "Keep Voice Hijack Target"))
          return;
        this.hijackKeep = flag;
        return;
      default:
        return;
    }
    if (flag)
      return;
    this.RevertLeaderboard();
  }
  private bool RunAction(string name)
  {
    if (name != null)
    {
      switch (name.Length)
      {
        case 7:
          switch (name[0])
          {
            case 'B':
              if (name == "Ban All")
              {
                this.BanAll();
                return true;
              }
              break;
            case 'L':
              if (name == "Lag All")
              {
                this.LagAll();
                return true;
              }
              break;
            case 'T':
              if (name == "Tag All")
              {
                this.TagAll();
                return true;
              }
              break;
          }
          break;
        case 8:
          switch (name[0])
          {
            case 'B':
              if (name == "Ban Self")
              {
                Debug.Log((object) "Ban Self is disabled in this version.");
                return true;
              }
              break;
            case 'K':
              if (name == "Kick All")
              {
                this.KickAll();
                return true;
              }
              break;
            case 'S':
              if (name == "Set Name")
              {
                this.ArmCapture(nameof (name));
                return true;
              }
              break;
            case 'T':
              if (name == "Tag Self")
              {
                this.TagSelf();
                return true;
              }
              break;
          }
          break;
        case 9:
          switch (name[0])
          {
            case 'C':
              if (name == "Crash All")
              {
                this.CrashAllOnce();
                return true;
              }
              break;
            case 'F':
              if (name == "Flush RPC")
              {
                this.FlushRPCs();
                return true;
              }
              break;
            case 'G':
              if (name == "Ghost All")
              {
                this.GhostAll();
                return true;
              }
              break;
            case 'J':
              if (name == "Join Room")
              {
                this.ArmCapture("room");
                return true;
              }
              break;
            case 'O':
              if (name == "Open Room")
              {
                this.SetRoomLocked(false);
                return true;
              }
              break;
            case 'T':
              if (name == "Type Code")
              {
                this.ArmCapture("code");
                return true;
              }
              break;
          }
          break;
        case 10:
          switch (name[0])
          {
            case 'C':
              if (name == "Close Room")
              {
                this.SetRoomLocked(true);
                return true;
              }
              break;
            case 'S':
              if (name == "Set Master")
              {
                this.SetMasterSelf();
                return true;
              }
              break;
          }
          break;
        case 11:
          switch (name[0])
          {
            case 'B':
              switch (name)
              {
                case "Become Lucy":
                  this.BecomeGhost("Lucy", 1f, 0.0f, 0.0f);
                  return true;
                case "Become PBBV":
                  this.BecomeGhost("PBBV", 0.8888889f, 0.444444448f, 0.444444448f);
                  return true;
              }
              break;
            case 'I':
              if (name == "Isolate All")
              {
                this.IsolateAll();
                return true;
              }
              break;
            case 'U':
              if (name == "Unghost All")
              {
                this.UnghostAll();
                return true;
              }
              break;
          }
          break;
        case 12:
          switch (name[0])
          {
            case 'A':
              if (name == "Antiban Join")
              {
                this.JoinCurrentCode();
                return true;
              }
              break;
            case 'D':
              if (name == "Destroy Self")
              {
                this.DestroySelf();
                return true;
              }
              break;
          }
          break;
        case 13:
          switch (name[0])
          {
            case 'F':
              if (name == "Fast Lucy (M)")
              {
                this.Lucy((object) 11f);
                return true;
              }
              break;
            case 'S':
              if (name == "Slow Lucy (M)")
              {
                this.Lucy((object) 1f);
                return true;
              }
              break;
          }
          break;
        case 14:
          switch (name[1])
          {
            case 'e':
              if (name == "Become Daisy09")
              {
                this.BecomeGhost("Daisy09", 1f, 0.6666667f, 1f);
                return true;
              }
              break;
            case 'p':
              if (name == "Spam Ghost All")
              {
                this.SpamGhostAll();
                return true;
              }
              break;
            case 't':
              if (name == "Stump Kick All")
              {
                this.StumpKickAll();
                return true;
              }
              break;
            case 'u':
              if (name == "Quarantine All")
              {
                this.QuarantineAll();
                return true;
              }
              break;
          }
          break;
        case 15:
          switch (name[0])
          {
            case 'F':
              if (name == "Freeze Lucy (M)")
              {
                this.Lucy((object) 0.0f);
                return true;
              }
              break;
            case 'G':
              if (name == "GodMode All (M)")
              {
                if (DetectedPorts.RequireMaster())
                {
                  foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
                  {
                    try
                    {
                      this.ChangeTeam(activeRig, 999);
                    }
                    catch
                    {
                    }
                  }
                }
                return true;
              }
              break;
            case 'N':
              if (name == "Normal Lucy (M)")
              {
                this.Lucy((object) 2f);
                return true;
              }
              break;
          }
          break;
        case 16 :
          switch (name[0])
          {
            case 'D':
              if (name == "Despawn Lucy (M)")
              {
                this.Lucy((object) new bool[1]);
                return true;
              }
              break;
            case 'G':
              if (name == "GodMode Self (M)")
              {
                VRRig rig = DetectedPorts.OwnRig();
                if (((Object) rig != (Object) null))
                  this.ChangeTeam(rig, 999);
                return true;
              }
              break;
          }
          break;
        case 17:
          if (name == "UnGodMode All (M)")
          {
            if (DetectedPorts.RequireMaster())
            {
              foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
              {
                try
                {
                  this.ChangeTeam(activeRig, 3);
                }
                catch
                {
                }
              }
            }
            return true;
          }
          break;
        case 18:
          switch (name[1])
          {
            case 'n':
              if (name == "UnGodMode Self (M)")
              {
                VRRig rig = DetectedPorts.OwnRig();
                if (((Object) rig != (Object) null))
                  this.ChangeTeam(rig, 3);
                return true;
              }
              break;
            case 'p':
              if (name == "Spawn Red Lucy (M)")
              {
                this.Lucy((object) new bool[2]{ true, true });
                return true;
              }
              break;
            case 't':
              if (name == "Stump Kick Antiban")
              {
                this.StumpKickAntiban();
                return true;
              }
              break;
          }
          break;
        case 19:
          switch (name[0])
          {
            case 'D':
              if (name == "Destroy All Players")
              {
                this.DestroyAllPlayers();
                return true;
              }
              break;
            case 'L':
              if (name == "Lucy Float Self (M)")
              {
                VRRig o = DetectedPorts.OwnRig();
                if (((Object) o != (Object) null))
                  this.Lucy((object) o);
                return true;
              }
              break;
            case 'S':
              if (name == "Spawn Blue Lucy (M)")
              {
                this.Lucy((object) new bool[2]
                {
                  true,
                  false
                });
                return true;
              }
              break;
          }
          break;
        case 20:
          switch (name[9])
          {
            case 'E':
              if (name == "Gamemode Exclude All")
              {
                this.ExcludeAll();
                return true;
              }
              break;
            case 'I':
              if (name == "Gamemode Include All")
              {
                this.IncludeAll();
                return true;
              }
              break;
            case 'e':
              switch (name)
              {
                case "Lucy Target Self (M)":
                  this.TargetSelf();
                  return true;
                case "Lucy Target Spaz (M)":
                  this.TargetSpaz();
                  return true;
              }
              break;
          }
          break;
        case 21:
          if (name == "Kick Network Triggers")
          {
            this.KickNetworkTriggers();
            return true;
          }
          break;
        case 22:
          switch (name[7])
          {
            case 'a':
              if (name == "Break Map Lighting (M)")
              {
                this.BreakMapLighting();
                return true;
              }
              break;
            case 'e':
              if (name == "Break Network Triggers")
              {
                this.BreakNetworkTriggers();
                return true;
              }
              break;
            case 'o':
              if (name == "Break Mod Checkers All")
              {
                this.BreakAll();
                return true;
              }
              break;
          }
          break;
        case 23:
          switch (name[19])
          {
            case ' ':
              if (name == "Bypass Mod Checkers All")
              {
                this.BypassAll();
                return true;
              }
              break;
            case 'H':
              if (name == "Change Gamemode to Hunt")
              {
                this.ChangeGamemode("HuntDown");
                return true;
              }
              break;
            case 'N':
              if (name == "Change Gamemode to None")
              {
                this.ChangeGamemode("None");
                return true;
              }
              break;
          }
          break;
        case 24:
          switch (name[0])
          {
            case 'C':
              if (name == "Change Gamemode to Count")
              {
                this.ChangeGamemode("Count");
                return true;
              }
              break;
            case 'I':
              if (name == "Insta Spawn Red Lucy (M)")
              {
                this.Lucy((object) new bool[3]
                {
                  true,
                  true,
                  true
                });
                return true;
              }
              break;
          }
          break;
        case 25:
          switch (name[20])
          {
            case 'a':
              if (name == "Change Gamemode to Casual")
              {
                this.ChangeGamemode("Casual");
                return true;
              }
              break;
            case 'm':
              if (name == "Change Gamemode to Ambush")
              {
                this.ChangeGamemode("Ambush");
                return true;
              }
              break;
            case 'u':
              if (name == "Change Gamemode to Custom")
              {
                this.ChangeGamemode("Custom");
                return true;
              }
              break;
            case 'y':
              if (name == "Insta Spawn Blue Lucy (M)")
              {
                this.Lucy((object) new bool[3]
                {
                  true,
                  false,
                  true
                });
                return true;
              }
              break;
          }
          break;
        case 27:
          if (name == "Change Gamemode to Guardian")
          {
            this.ChangeGamemode("Guardian");
            return true;
          }
          break;
        case 28:
          switch (name[19])
          {
            case 'G':
              if (name == "Change Gamemode to Ghost Tag")
              {
                this.ChangeGamemode("Ghost");
                return true;
              }
              break;
            case 'I':
              if (name == "Change Gamemode to Infection")
              {
                this.ChangeGamemode("Infection");
                return true;
              }
              break;
            case 'P':
              if (name == "Change Gamemode to Prop Hunt")
              {
                this.ChangeGamemode("PropHunt");
                return true;
              }
              break;
          }
          break;
        case 29:
          switch (name[19])
          {
            case 'F':
              if (name == "Change Gamemode to Freeze Tag")
              {
                this.ChangeGamemode("FreezeTag");
                return true;
              }
              break;
            case 'P':
              if (name == "Change Gamemode to Paintbrawl")
              {
                this.ChangeGamemode("Paintbrawl");
                return true;
              }
              break;
          }
          break;
        case 31 :
          if (name == "Change Gamemode to Super Casual")
          {
            this.ChangeGamemode("SuperCasual");
            return true;
          }
          break;
        case 34:
          if (name == "Change Gamemode to Super Infection")
          {
            this.ChangeGamemode("SuperInfect");
            return true;
          }
          break;
        case 40:
          if (name == "Change Gamemode to Competitive Infection")
          {
            this.ChangeGamemode("InfectionCompetitive");
            return true;
          }
          break;
      }
    }
    return false;
  }
  public void Tick()
  {
    this.PollCapture();
    this.isolateOthers = this.state.IsOn("Isolate Others");
    this.moddedGamemode = this.state.IsOn("Switch to Modded Gamemode");
    if (this.state.IsOn("Freeze All"))
    {
      foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
        this.FreezeRig(activeRig);
    }
    else if (this.freezeUntil.Count > 0)
    {
      float realtimeSinceStartup = Time.realtimeSinceStartup;
      List<int> intList = (List<int>) null;
      foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
        float num;
        if (netPlayer != null && this.freezeUntil.TryGetValue(netPlayer.ActorNumber, out num))
        {
          if ((double) realtimeSinceStartup < (double) num)
          {
            this.FreezeRig(activeRig);
          }
          else
          {
            if (intList == null)
              intList = new List<int>();
            intList.Add(netPlayer.ActorNumber);
          }
        }
      }
      if (intList != null)
      {
        foreach (int key in intList)
          this.freezeUntil.Remove(key);
      }
    }
    if (this.state.IsOn("Lag Aura"))
      this.LagAura();
    if (this.state.IsOn("Lag On Touch"))
      this.LagOnTouch();
    if (this.state.IsOn("Mute All"))
      this.MuteAll();
    if (this.state.IsOn("Mute Aura"))
      this.MuteAura();
    if (this.state.IsOn("Mute On Touch"))
      this.MuteOnTouch();
    if (this.state.IsOn("Leaderboard Mute"))
      this.LeaderboardMute();
    if (this.state.IsOn("Quarantine Aura"))
      this.QuarantineAura();
    if (this.state.IsOn("Quarantine On Touch"))
      this.QuarantineOnTouch();
    if (this.state.IsOn("Leaderboard Quarantine"))
      this.LeaderboardQuarantine();
    if (this.state.IsOn("Ghost Aura"))
      this.GhostAura();
    if (this.state.IsOn("Ghost On Touch"))
      this.GhostOnTouch();
    if (this.state.IsOn("Leaderboard Ghost"))
      this.LeaderboardGhost();
    if (this.state.IsOn("Crash Leaderboard"))
      this.LeaderboardCrash();
    if (this.state.IsOn("Unghost Aura"))
      this.UnghostAura();
    if (this.state.IsOn("Unghost On Touch"))
      this.UnghostOnTouch();
    if (this.state.IsOn("Spam Ghost Aura"))
      this.SpamGhostAura();
    if (this.state.IsOn("Spam Ghost On Touch"))
      this.SpamGhostOnTouch();
    if (this.state.IsOn("Isolate Aura"))
      this.IsolateAura();
    if (this.state.IsOn("Isolate On Touch"))
      this.IsolateOnTouch();
    if (this.state.IsOn("Change Name Aura"))
      this.ChangeNameAura();
    if (this.state.IsOn("Change Name On Touch"))
      this.ChangeNameOnTouch();
    if (this.state.IsOn("Bypass Mod Checkers Aura"))
      this.BypassAura();
    if (this.state.IsOn("Bypass Mod Checkers On Touch"))
      this.BypassOnTouch();
    if (this.state.IsOn("Break Mod Checkers Aura"))
      this.BreakAura();
    if (this.state.IsOn("Break Mod Checkers On Touch"))
      this.BreakOnTouch();
    if (this.state.IsOn("Gamemode Include Aura"))
      this.IncludeAura();
    if (this.state.IsOn("Gamemode Include On Touch"))
      this.IncludeOnTouch();
    if (this.state.IsOn("Gamemode Exclude Aura"))
      this.ExcludeAura();
    if (this.state.IsOn("Gamemode Exclude On Touch"))
      this.ExcludeOnTouch();
    if (this.state.IsOn("Spaz Gamemode"))
      this.SpazGamemode();
    if (this.state.IsOn("Anti Mod Checker") && (double) Time.time > (double) this.antiDelay)
    {
      this.antiDelay = Time.time + 1f;
      try
      {
        this.BypassPlayer(PhotonNetwork.LocalPlayer);
      }
      catch
      {
      }
    }
    if (this.state.IsOn("Lucy Float Touch (M)"))
      this.FloatTouch();
    if (this.state.IsOn("Lucy Float On Touch (M)"))
      this.FloatOnTouch();
    if (this.state.IsOn("Lucy Grab (M)"))
      this.GrabLucy();
    if (this.orbitLucy)
      this.OrbitTick();
    try
    {
      RigPatchKit.LucyFreeze = this.state.IsOn("Lucy Grab (M)") || this.state.IsOn("Lucy Move Gun (M)") || this.state.IsOn("Lucy Orbit (M)");
    }
    catch
    {
    }
    if (this.state.IsOn("Ghost Monkey"))
    {
      try
      {
        if (((Object) VRRig.LocalRig != (Object) null))
          ((Behaviour) VRRig.LocalRig).enabled = false;
      }
      catch
      {
      }
    }
    if (this.state.IsOn("Invisible Monkey"))
    {
      try
      {
        VRRig localRig = VRRig.LocalRig;
        if (((Object) localRig != (Object) null))
        {
          ((Behaviour) localRig).enabled = false;
          ((Component) localRig).transform.position = new Vector3(0.0f, -100f, 0.0f);
        }
      }
      catch
      {
      }
    }
    if (this.state.IsOn("Tag Aura"))
      this.TagAura();
    if (this.state.IsOn("Grab Fling All"))
    {
      if (this.flingQueue.Count == 0)
      {
        foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
          this.flingQueue.Add(activeRig);
        this.flingIdx = 0;
        this.flingNextAt = 0.0f;
      }
      if (this.flingIdx < this.flingQueue.Count && (double) Time.time >= (double) this.flingNextAt)
      {
        VRRig fling = this.flingQueue[this.flingIdx++];
        this.flingNextAt = Time.time + 1f;
        if (((Object) fling != (Object) null) && !DetectedPorts.IsLocal(fling))
          this.GrabFling(fling);
      }
    }
    else if (this.flingQueue.Count > 0)
    {
      this.flingQueue.Clear();
      this.flingIdx = 0;
    }
    this.DaisyWatchdog();
    this.TwirlTick();
    if (this.state.IsOn("Voice Hijack"))
      this.VoiceHijackTick();
    else if (this.hijackTarget != null)
      this.UpdateTalkThrough();
    if (this.state.IsOn("Bypass Auto Mute"))
      this.BypassAutoMuteTick();
    if (this.state.IsOn("Anti Report"))
      this.AntiReportTick();
    else
      this.ClearAntiReportSphere();
    if (this.state.IsOn("Unlimited RPC"))
      this.UnlimitedRPCs(true);
    this.ProjectileSpamTick();
  }
  public void OnGunBody(Transform body)
  {
    if (((Object) body == (Object) null))
      return;
    VRRig vrRig = (VRRig) null;
    try
    {
      foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
      {
        if (!((Object) activeRig == (Object) null) && ((Object) activeRig.bodyTransform == (Object) body))
        {
          vrRig = activeRig;
          break;
        }
      }
      if (((Object) vrRig == (Object) null))
      {
        foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
        {
          if (((Object) activeRig != (Object) null) && ((Object) ((Component) activeRig).transform == (Object) body))
          {
            vrRig = activeRig;
            break;
          }
        }
      }
    }
    catch
    {
    }
    if (((Object) vrRig == (Object) null) || DetectedPorts.IsLocal(vrRig))
      return;
    if (this.state.IsOn("Freeze Gun"))
    {
      try
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(vrRig);
        if (netPlayer != null)
          this.freezeUntil[netPlayer.ActorNumber] = Time.realtimeSinceStartup + 5f;
      }
      catch
      {
      }
      this.FreezeRig(vrRig);
    }
    if (this.state.IsOn("Crash Gun") && this.state.PrivateModdedRoom && !DetectedPorts.IsLocal(vrRig))
    {
      DetectedPorts.RPCProtection();
      try
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(vrRig);
        if (netPlayer != null)
          this.freezeUntil[netPlayer.ActorNumber] = Time.realtimeSinceStartup + 5f;
      }
      catch
      {
      }
      this.FreezeRig(vrRig, 300);
      try
      {
        PhotonView view = DetectedPorts.GetView(vrRig);
        if (((Object) view == (Object) null))
        {
          Debug.Log((object) "Crash: no view for target.");
          return;
        }
        NetPlayer owner = DetectedPorts.GetNetPlayer(vrRig);
        Debug.Log((object) $"Crash: gun fired on {(owner != null ? owner.NickName : "?")} view {view.ViewID.ToString()}.");
        int[] array = ((IEnumerable<Player>) PhotonNetwork.PlayerList).Where<Player>((Func<Player, bool>) (p => owner == null || p.ActorNumber != owner.ActorNumber)).Select<Player, int>((Func<Player, int>) (p => p.ActorNumber)).ToArray<int>();
        if (array.Length == 0)
          return;
        ExitGames.Client.Photon.Hashtable hashtable1 = new ExitGames.Client.Photon.Hashtable();
        hashtable1.Add((byte) 0, (object) view.ViewID);
        ExitGames.Client.Photon.Hashtable hashtable2 = hashtable1;
        LoadBalancingClient networkingClient = PhotonNetwork.NetworkingClient;
        ExitGames.Client.Photon.Hashtable hashtable3 = hashtable2;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.TargetActors = array;
        SendOptions sendReliable = SendOptions.SendReliable;
        networkingClient.OpRaiseEvent((byte) 204, (object) hashtable3, raiseEventOptions, sendReliable);
        this.FlushAndReplay();
        DetectedPorts.AntiRPCKick();
      }
      catch
      {
      }
    }
    if (this.state.IsOn("Ban Gun"))
      this.BanPlayer(DetectedPorts.GetPhotonPlayer(vrRig));
    if (this.state.IsOn("Lag Gun"))
      this.PoisonLag(DetectedPorts.GetNetPlayer(vrRig));
    if (this.state.IsOn("Destroy Player Gun") && (double) Time.time > (double) this.destroyGunDelay)
    {
      this.destroyGunDelay = Time.time + 1f;
      try
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(vrRig);
        if (netPlayer != null)
        {
          if (!netPlayer.IsLocal)
            PhotonNetwork.OpRemoveCompleteCacheOfPlayer(netPlayer.ActorNumber);
        }
      }
      catch
      {
      }
    }
    if (this.state.IsOn("Quarantine Gun"))
      this.QuarantineRig(vrRig);
    if (this.state.IsOn("Ghost Gun"))
      this.GhostRig(vrRig);
    if (this.state.IsOn("Unghost Gun"))
      this.UnghostRig(vrRig);
    if (this.state.IsOn("Spam Ghost Gun"))
    {
      this.GhostRig(vrRig);
      this.UnghostRig(vrRig);
    }
    if (this.state.IsOn("Isolate Gun"))
      this.IsolateRig(vrRig);
    if (this.state.IsOn("Mute Gun"))
      this.PoisonLag(DetectedPorts.GetNetPlayer(vrRig));
    if (this.state.IsOn("Kick Gun") && !DetectedPorts.IsLocal(vrRig))
      this.KickGun(vrRig);
    if (this.state.IsOn("Voice Hijack Gun"))
    {
      try
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(vrRig);
        if (netPlayer == null || netPlayer == NetworkSystem.Instance.LocalPlayer)
          return;
        this.hijackKeep = true;
        if (this.TrySetTalkThrough(netPlayer))
        {
          Debug.Log((object) $"Voice Hijack Gun: speaking through {netPlayer.NickName}.");
          try
          {
            this.DaisyStart();
          }
          catch
          {
          }
        }
        else
          Debug.Log((object) "Voice Hijack Gun: target voice slot busy.");
      }
      catch
      {
      }
    }
    if (this.state.IsOn("Change Name Gun") && this.PropDelay())
      this.ChangeNamePlayer(DetectedPorts.GetPhotonPlayer(vrRig), this.customName);
    if (this.state.IsOn("Bypass Mod Checkers Gun") && this.PropDelay())
      this.BypassPlayer(DetectedPorts.GetPhotonPlayer(vrRig));
    if (this.state.IsOn("Break Mod Checkers Gun") && this.PropDelay())
      this.BreakPlayer(DetectedPorts.GetPhotonPlayer(vrRig));
    if (this.state.IsOn("Gamemode Include Gun") && this.PropDelay())
      this.SetTutorial(DetectedPorts.GetPhotonPlayer(vrRig), true);
    if (this.state.IsOn("Gamemode Exclude Gun") && this.PropDelay())
      this.SetTutorial(DetectedPorts.GetPhotonPlayer(vrRig), false);
    if (this.state.IsOn("Lucy Float Gun (M)"))
      this.Lucy((object) vrRig);
    if (this.state.IsOn("Lucy Target Gun (M)"))
    {
      NetPlayer o = (NetPlayer) null;
      try
      {
        o = DetectedPorts.GetNetPlayer(vrRig);
      }
      catch
      {
      }
      if (o != null)
        this.Lucy((object) o);
    }
    if (this.state.IsOn("GodMode Gun (M)"))
      this.ChangeTeam(vrRig, 999);
    if (this.state.IsOn("UnGodMode Gun (M)"))
      this.ChangeTeam(vrRig, 3);
    if (this.state.IsOn("Tag Gun"))
    {
      try
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(vrRig);
        if (netPlayer == null)
          return;
        if (PhotonNetwork.IsMasterClient)
          DetectedPorts.InfectExe(netPlayer);
        else if (DetectedPorts.ManagerIsTagged(NetworkSystem.Instance.LocalPlayer))
          GameMode.ReportTag(netPlayer);
        else
          this.XyferTag(vrRig);
      }
      catch
      {
      }
    }
    if (!this.state.IsOn("Grab Fling Gun"))
      return;
    try
    {
      if (DetectedPorts.IsLocal(vrRig))
        return;
      this.GrabFling(vrRig);
    }
    catch
    {
    }
  }
  public void OnGunPoint(Vector3 point)
  {
    if (!this.state.IsOn("Lucy Move Gun (M)"))
      return;
    try
    {
      this.Lucy((object) (point + new Vector3(0.0f, 0.5f, 0.0f)));
    }
    catch
    {
    }
  }
  public void Dispose()
  {
    try
    {
      this.RevertLeaderboard();
    }
    catch
    {
    }
    try
    {
      if (((Object) VRRig.LocalRig != (Object) null))
        ((Behaviour) VRRig.LocalRig).enabled = true;
    }
    catch
    {
    }
    try
    {
      this.DaisyStop();
    }
    catch
    {
    }
    try
    {
      this.DisableTalkThrough();
    }
    catch
    {
    }
    try
    {
      Recorder talkRecorder = DetectedPorts.GetTalkRecorder();
      if (((Object) talkRecorder != (Object) null))
        talkRecorder.DebugEchoMode = false;
    }
    catch
    {
    }
    try
    {
      RigPatchKit.LucyFreeze = false;
    }
    catch
    {
    }
    try
    {
      this.ClearAntiReportSphere();
    }
    catch
    {
    }
    try
    {
      this.UnlimitedRPCs(false);
    }
    catch
    {
    }
  }
  private void TwirlTick()
  {
    try
    {
      VRRig localRig = VRRig.LocalRig;
      if (((Object) localRig == (Object) null))
        return;
      if (this.state.IsOn("Head Twirl X") || this.state.IsOn("Head Twirl Y") || this.state.IsOn("Head Twirl Z"))
      {
        try
        {
          if (((Behaviour) localRig).enabled)
          {
            if (this.state.IsOn("Head Twirl X"))
              localRig.head.trackingRotationOffset.x += 8f;
            if (this.state.IsOn("Head Twirl Y"))
              localRig.head.trackingRotationOffset.y += 8f;
            if (this.state.IsOn("Head Twirl Z"))
              localRig.head.trackingRotationOffset.z += 8f;
          }
          else if (localRig.head != null)
          {
            if (((Object) localRig.head.rigTarget != (Object) null))
            {
              Quaternion rotation = ((Component) localRig.head.rigTarget).transform.rotation;
              Vector3 eulerAngles = (rotation).eulerAngles;
              if (this.state.IsOn("Head Twirl X"))
                eulerAngles.x += 8f;
              if (this.state.IsOn("Head Twirl Y"))
                eulerAngles.y += 8f;
              if (this.state.IsOn("Head Twirl Z"))
                eulerAngles.z += 8f;
              ((Component) localRig.head.rigTarget).transform.rotation = Quaternion.Euler(eulerAngles);
            }
          }
        }
        catch
        {
        }
      }
      if (this.state.IsOn("Body Twirl"))
      {
        try
        {
          ((Component) localRig).transform.Rotate(Vector3.up, 8f);
        }
        catch
        {
        }
      }
      if (this.state.IsOn("Ghost Monke"))
      {
        try
        {
          ((Behaviour) localRig).enabled = false;
        }
        catch
        {
        }
      }
      if (!this.state.IsOn("Invis Monke"))
        return;
      try
      {
        ((Behaviour) localRig).enabled = false;
        ((Component) localRig).transform.position = new Vector3(0.0f, -100f, 0.0f);
      }
      catch
      {
      }
    }
    catch
    {
    }
  }
  public void BecomeGhost(string ghostName, float r, float g, float b)
  {
    try
    {
      try
      {
        PlayerPrefs.SetFloat("redValue", r);
        PlayerPrefs.SetFloat("greenValue", g);
        PlayerPrefs.SetFloat("blueValue", b);
        PlayerPrefs.Save();
      }
      catch
      {
      }
      try
      {
        GorillaTagger.Instance.UpdateColor(r, g, b);
      }
      catch
      {
      }
      try
      {
        GorillaComputer.instance.currentName = ghostName;
        GorillaComputer.instance.SetLocalNameTagText(ghostName);
        GorillaComputer.instance.savedName = ghostName;
        PlayerPrefs.SetString("playerName", ghostName);
        PlayerPrefs.Save();
        PhotonNetwork.LocalPlayer.NickName = ghostName;
      }
      catch
      {
      }
      Debug.Log((object) $"Became {ghostName}.");
    }
    catch
    {
    }
  }
  private static string DaisyPath()
  {
    try
    {
      return Path.Combine(Paths.GameRootPath, "Taurus", "Sounds", "Daisy09.mp3");
    }
    catch
    {
      return (string) null;
    }
  }
  public void DaisyStart()
  {
    try
    {
      if (((Object) this.daisyClip != (Object) null) || this.daisyLoading)
        return;
      string path = DetectedPorts.DaisyPath();
      if (string.IsNullOrEmpty(path) || !File.Exists(path))
      {
        Debug.Log((object) "Daisy09.mp3 missing from Taurus/Sounds.");
      }
      else
      {
        if (((Object) this.daisyGO == (Object) null))
        {
          this.daisyGO = new GameObject("TaurusDaisySong");
          Object.DontDestroyOnLoad((Object) this.daisyGO);
          this.daisySource = this.daisyGO.AddComponent<AudioSource>();
          this.daisySource.playOnAwake = false;
          this.daisySource.spatialBlend = 0.0f;
        }
        this.daisyLoading = true;
        ((MonoBehaviour) GorillaTagger.Instance).StartCoroutine(this.DaisyLoad(path));
      }
    }
    catch
    {
      this.daisyLoading = false;
    }
  }
  private IEnumerator DaisyLoad(string path)
  {
    UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file://" + path, (AudioType) 13);
    yield return (object) req.SendWebRequest();
    this.daisyLoading = false;
    try
    {
      if (!this.state.IsOn("Play Daisy09 Song"))
        req.Dispose();
      else if (req.result != (UnityWebRequest.Result) 1)
      {
        Debug.Log((object) "Daisy09 song failed to decode.");
        req.Dispose();
      }
      else
      {
        AudioClip content = DownloadHandlerAudioClip.GetContent(req);
        req.Dispose();
        if (!((Object) content == (Object) null) && (double) content.length > 0.0)
        {
          this.daisyClip = content;
          this.RouteDaisyMic();
          this.daisySource.clip = this.daisyClip;
          this.daisySource.loop = true;
          try
          {
            this.daisySource.volume = this.state.SoundVolume;
          }
          catch
          {
          }
          this.daisySource.Play();
        }
      }
    }
    catch
    {
      try
      {
        req.Dispose();
      }
      catch
      {
      }
    }
  }
  private void RouteDaisyMic()
  {
    try
    {
      if (((Object) this.daisyClip == (Object) null))
        return;
      Recorder myRecorder = ((Object) GorillaTagger.Instance != (Object) null) ? GorillaTagger.Instance.myRecorder : (Recorder) null;
      if (((Object) myRecorder == (Object) null) || !myRecorder.IsInitialized || !myRecorder.IsRecording || !myRecorder.TransmitEnabled || myRecorder.SourceType != null)
      {
        Debug.Log((object) "Daisy09 song: local only (mic unavailable).");
      }
      else
      {
        float[] numArray = new float[this.daisyClip.samples * this.daisyClip.channels];
        if (!this.daisyClip.GetData(numArray, 0))
          return;
        float num = 0.65f;
        try
        {
          num = this.state.MicVolume;
        }
        catch
        {
        }
        for (int index = 0; index < numArray.Length; ++index)
          numArray[index] *= num;
        this.daisyMic = AudioClip.Create("Taurus daisy song", this.daisyClip.samples, this.daisyClip.channels, this.daisyClip.frequency, false);
        if (!this.daisyMic.SetData(numArray, 0))
        {
          Object.Destroy((Object) this.daisyMic);
          this.daisyMic = (AudioClip) null;
        }
        else
        {
          this.daisyPrevSource = myRecorder.SourceType;
          this.daisyPrevClip = myRecorder.AudioClip;
          this.daisyPrevLoop = myRecorder.LoopAudioClip;
          this.daisyPrevDetect = myRecorder.VoiceDetection;
          this.daisyRec = myRecorder;
          this.daisyRec.AudioClip = this.daisyMic;
          this.daisyRec.LoopAudioClip = true;
          this.daisyRec.VoiceDetection = false;
          this.daisyRec.SourceType = (Recorder.InputSourceType) 1;
          this.daisyRec.RestartRecording(true);
          Debug.Log((object) "Daisy09 song: local + mic.");
        }
      }
    }
    catch (Exception ex)
    {
      this.DaisyRestoreMic();
      Debug.LogWarning((object) ("Daisy09 mic failed: " + ex.Message));
    }
  }
  private void DaisyRestoreMic()
  {
    try
    {
      if (!((Object) this.daisyRec != (Object) null) || !((Object) this.daisyRec.AudioClip == (Object) this.daisyMic))
        return;
      this.daisyRec.SourceType = this.daisyPrevSource;
      this.daisyRec.AudioClip = this.daisyPrevClip;
      this.daisyRec.LoopAudioClip = this.daisyPrevLoop;
      this.daisyRec.VoiceDetection = this.daisyPrevDetect;
      this.daisyRec.RestartRecording(true);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ("Daisy09 mic restore failed: " + ex.Message));
    }
    finally
    {
      this.daisyRec = (Recorder) null;
      if (((Object) this.daisyMic != (Object) null))
        Object.Destroy((Object) this.daisyMic);
      this.daisyMic = (AudioClip) null;
    }
  }
  public void DaisyStop()
  {
    try
    {
      if (((Object) this.daisySource != (Object) null))
        this.daisySource.Stop();
    }
    catch
    {
    }
    try
    {
      this.DaisyRestoreMic();
    }
    catch
    {
    }
    try
    {
      if (((Object) this.daisyClip != (Object) null))
        Object.Destroy((Object) this.daisyClip);
    }
    catch
    {
    }
    this.daisyClip = (AudioClip) null;
    this.daisyLoading = false;
  }
  private void DaisyWatchdog()
  {
    try
    {
      if (!this.state.IsOn("Play Daisy09 Song"))
        return;
      if (((Object) this.daisyClip != (Object) null) && ((Object) this.daisySource != (Object) null) && !this.daisySource.isPlaying)
        this.daisySource.Play();
      if (!((Object) this.daisyRec != (Object) null) || !((Object) NetworkSystem.Instance == (Object) null) && NetworkSystem.Instance.InRoom)
        return;
      this.DaisyRestoreMic();
    }
    catch
    {
    }
  }
  public void VoiceHijackTick()
  {
    try
    {
      if ((double) Time.time < (double) this.hijackAutoDelay)
        return;
      this.hijackAutoDelay = Time.time + 0.1f;
      if (this.hijackTarget != null)
      {
        this.UpdateTalkThrough();
      }
      else
      {
        List<NetPlayer> netPlayerList = this.TalkThroughCandidates();
        if (netPlayerList.Count == 0)
          return;
        bool flag = this.hijackTarget != null;
        for (int index = netPlayerList.Count - 1; index >= 0; --index)
        {
          NetPlayer player = netPlayerList[index];
          if (!this.IsVoiceSlotClaimable(player))
          {
            this.hijackFirstSeen.Remove(player.ActorNumber);
          }
          else
          {
            if (!this.hijackFirstSeen.ContainsKey(player.ActorNumber))
              this.hijackFirstSeen[player.ActorNumber] = Time.time;
            if (this.TrySetTalkThrough(player))
            {
              flag = true;
              break;
            }
          }
        }
        if (flag || (double) Time.time < (double) this.hijackNoSlotAt)
          return;
        this.hijackNoSlotAt = Time.time + 10f;
        Debug.Log((object) $"Voice Hijack: {netPlayerList.Count.ToString()} candidates, no claimable voice slot.");
      }
    }
    catch
    {
    }
  }
  public void UpdateTalkThrough()
  {
    try
    {
      if (this.hijackTarget == null)
        return;
      Recorder talkRecorder = DetectedPorts.GetTalkRecorder();
      if (((Object) talkRecorder == (Object) null))
        return;
      if (((Object) NetworkSystem.Instance == (Object) null) || NetworkSystem.Instance.AllNetPlayers == null || !((IEnumerable<NetPlayer>) NetworkSystem.Instance.AllNetPlayers).Contains<NetPlayer>(this.hijackTarget))
      {
        this.DisableTalkThrough();
      }
      else
      {
        int viewId1;
        int num1 = this.TryGetTargetViewId(this.hijackTarget, out viewId1) ? 1 : 0;
        if (!(talkRecorder.UserData is int num2))
          num2 = -1;
        int num3 = num2;
        if (num1 != 0 && num3 == viewId1)
        {
          this.RunScheduledReasserts(viewId1);
        }
        else
        {
          if ((double) Time.time < (double) this.hijackReassertDelay)
            return;
          this.hijackReassertDelay = Time.time + 2f;
          VRRig rig = DetectedPorts.RigFromPlayer(this.hijackTarget);
          if (((Object) rig == (Object) null))
            return;
          int viewId2 = DetectedPorts.RigVoiceId(rig);
          if (viewId2 == 0)
            return;
          this.ApplyRecorderUserData(viewId2);
        }
      }
    }
    catch
    {
    }
  }
  public void DisableTalkThrough()
  {
    try
    {
      this.hijackTarget = (NetPlayer) null;
      if (((Object) GorillaTagger.Instance != (Object) null) && ((Object) GorillaTagger.Instance.myVRRig != (Object) null))
      {
        PhotonView component = ((Component) GorillaTagger.Instance.myVRRig).GetComponent<PhotonView>();
        if (((Object) component != (Object) null))
          this.ApplyRecorderUserData(component.ViewID);
      }
      Recorder talkRecorder = DetectedPorts.GetTalkRecorder();
      if (!((Object) talkRecorder != (Object) null))
        return;
      talkRecorder.VoiceDetection = true;
    }
    catch
    {
    }
  }
  private static PhotonVoiceView RigVoiceView(VRRig rig)
  {
    try
    {
      if (((Object) rig == (Object) null))
        return (PhotonVoiceView) null;
      try
      {
        if (Traverse.Create((object) rig).Field("netView").GetValue() is NetworkView networkView)
        {
          if (((Object) networkView != (Object) null))
          {
            PhotonVoiceView component = ((Component) networkView).GetComponent<PhotonVoiceView>();
            if (((Object) component != (Object) null))
              return component;
          }
        }
      }
      catch
      {
      }
      try
      {
        PhotonVoiceView component = ((Component) rig).GetComponent<PhotonVoiceView>();
        if (((Object) component != (Object) null))
          return component;
      }
      catch
      {
      }
      try
      {
        return ((Component) rig).GetComponentInChildren<PhotonVoiceView>();
      }
      catch
      {
      }
    }
    catch
    {
    }
    return (PhotonVoiceView) null;
  }
  private static int RigVoiceId(VRRig rig)
  {
    try
    {
      PhotonVoiceView photonVoiceView = DetectedPorts.RigVoiceView(rig);
      if (((Object) photonVoiceView != (Object) null))
      {
        try
        {
          PhotonView component = ((Component) photonVoiceView).GetComponent<PhotonView>();
          if (((Object) component != (Object) null))
            return component.ViewID;
        }
        catch
        {
        }
      }
      PhotonView view = DetectedPorts.GetView(rig);
      if (((Object) view != (Object) null))
        return view.ViewID;
    }
    catch
    {
    }
    return 0;
  }
  private bool TrySetTalkThrough(NetPlayer player)
  {
    try
    {
      if (player == null || player == NetworkSystem.Instance.LocalPlayer || ((Object) DetectedPorts.GetTalkRecorder() == (Object) null) || !this.IsVoiceSlotClaimable(player))
        return false;
      VRRig rig = DetectedPorts.RigFromPlayer(player);
      if (((Object) rig == (Object) null) || rig.isLocal)
        return false;
      int viewId = DetectedPorts.RigVoiceId(rig);
      if (viewId == 0)
        return false;
      this.hijackTarget = player;
      this.hijackClaimTime = Time.time;
      this.hijackReassert1 = false;
      this.hijackReassert2 = false;
      this.ApplyRecorderUserData(viewId);
      this.EnsureMainMicTransmitting();
      Debug.Log((object) $"Voice Hijack: speaking through {player.NickName} (view {viewId.ToString()}).");
      return true;
    }
    catch
    {
      return false;
    }
  }
  private bool IsVoiceSlotClaimable(NetPlayer player)
  {
    try
    {
      if (player == null)
        return false;
      VRRig rig = DetectedPorts.RigFromPlayer(player);
      if (((Object) rig == (Object) null))
        return false;
      PhotonVoiceView photonVoiceView = DetectedPorts.RigVoiceView(rig);
      if (((Object) photonVoiceView == (Object) null))
        return false;
      Speaker speakerInUse = photonVoiceView.SpeakerInUse;
      return ((Object) speakerInUse == (Object) null) || !speakerInUse.IsLinked;
    }
    catch
    {
      return false;
    }
  }
  private void ApplyRecorderUserData(int viewId)
  {
    try
    {
      Recorder talkRecorder = DetectedPorts.GetTalkRecorder();
      if (((Object) talkRecorder == (Object) null))
        return;
      talkRecorder.UserData = (object) viewId;
      try
      {
        talkRecorder.RestartRecording(true);
      }
      catch
      {
      }
    }
    catch
    {
    }
  }
  private void EnsureMainMicTransmitting()
  {
    try
    {
      Recorder talkRecorder = DetectedPorts.GetTalkRecorder();
      if (((Object) talkRecorder == (Object) null))
        return;
      if (PhotonNetwork.InRoom)
        talkRecorder.IsRecording = true;
      talkRecorder.TransmitEnabled = true;
      talkRecorder.VoiceDetection = false;
    }
    catch
    {
    }
  }
  private void RunScheduledReasserts(int targetViewId)
  {
    try
    {
      float num = Time.time - this.hijackClaimTime;
      if (!this.hijackReassert1 && (double) num >= 2.5)
      {
        this.hijackReassert1 = true;
        this.ApplyRecorderUserData(targetViewId);
      }
      else
      {
        if (this.hijackReassert2 || (double) num < 6.0)
          return;
        this.hijackReassert2 = true;
        this.ApplyRecorderUserData(targetViewId);
      }
    }
    catch
    {
    }
  }
  private bool TryGetTargetViewId(NetPlayer player, out int viewId)
  {
    viewId = 0;
    try
    {
      VRRig rig = DetectedPorts.RigFromPlayer(player);
      if (((Object) rig == (Object) null))
        return false;
      viewId = DetectedPorts.RigVoiceId(rig);
      return viewId != 0;
    }
    catch
    {
      return false;
    }
  }
  private List<NetPlayer> TalkThroughCandidates()
  {
    List<NetPlayer> netPlayerList = new List<NetPlayer>();
    try
    {
      if (((Object) NetworkSystem.Instance == (Object) null) || NetworkSystem.Instance.AllNetPlayers == null)
        return netPlayerList;
      foreach (NetPlayer allNetPlayer in NetworkSystem.Instance.AllNetPlayers)
      {
        if (allNetPlayer != null && allNetPlayer != NetworkSystem.Instance.LocalPlayer)
          netPlayerList.Add(allNetPlayer);
      }
      netPlayerList.Sort((Comparison<NetPlayer>) ((a, b) => a.ActorNumber.CompareTo(b.ActorNumber)));
    }
    catch
    {
    }
    return netPlayerList;
  }
  private static Recorder GetTalkRecorder()
  {
    try
    {
      if (((Object) GorillaTagger.Instance != (Object) null) && ((Object) GorillaTagger.Instance.myRecorder != (Object) null))
        return GorillaTagger.Instance.myRecorder;
      if (((Object) NetworkSystem.Instance != (Object) null))
        return NetworkSystem.Instance.LocalRecorder;
    }
    catch
    {
    }
    return (Recorder) null;
  }
  private static VRRig RigFromPlayer(NetPlayer player)
  {
    try
    {
      if (player == null)
        return (VRRig) null;
      foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
      {
        if (!((Object) activeRig == (Object) null))
        {
          try
          {
            if (DetectedPorts.GetNetPlayer(activeRig) == player)
              return activeRig;
          }
          catch
          {
          }
        }
      }
    }
    catch
    {
    }
    return (VRRig) null;
  }
  private static bool InfectExe(NetPlayer player)
  {
    try
    {
      if (player == null || ((Object) GorillaGameManager.instance == (Object) null))
        return false;
      GorillaTagManager instance = GorillaGameManager.instance as GorillaTagManager;
      if (((Object) instance == (Object) null))
        return false;
      MethodInfo method = typeof (GorillaTagManager).GetMethod("ChangeCurrentIt", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
      if (method == (MethodInfo) null)
        return false;
      method.Invoke((object) instance, new object[2]
      {
        (object) player,
        (object) true
      });
      return true;
    }
    catch
    {
      return false;
    }
  }
  private void LaunchRig(VRRig rig, Vector3 velocity)
  {
    try
    {
      if (((Object) rig == (Object) null))
        return;
      NetPlayer netPlayer = DetectedPorts.GetNetPlayer(rig);
      if (netPlayer == null)
        return;
      object obj = typeof (GameMode).GetProperty("ActiveNetworkHandler", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).GetValue((object) null);
      obj?.GetType().GetMethod("SendRPC").Invoke(obj, new object[3]
      {
        (object) "GuardianLaunchPlayer",
        (object) netPlayer,
        (object) velocity
      });
    }
    catch
    {
    }
  }
  private void GrabFling(VRRig target)
  {
    try
    {
      if (((Object) target == (Object) null) || target.isOfflineVRRig)
        return;
      Debug.Log((object) $"Grab Fling: launching {((Object) target).name}.");
      this.LaunchRig(target, (Vector3.up * 30f));
      DetectedPorts.AntiRPCKick();
    }
    catch
    {
    }
  }
  public void AntiReportTick()
  {
    try
    {
      Transform transform = ((Object) GorillaTagger.Instance != (Object) null) ? ((Component) GorillaTagger.Instance.headCollider).transform : (Transform) null;
      if (((Object) transform == (Object) null))
        return;
      float num = 15f;
      try
      {
        num = this.state.AntiReportDistance;
      }
      catch
      {
      }
      foreach (GorillaPlayerScoreboardLine playerScoreboardLine in Object.FindObjectsByType<GorillaPlayerScoreboardLine>((FindObjectsInactive) 0, (FindObjectsSortMode) 0))
      {
        if (!((Object) playerScoreboardLine == (Object) null) && playerScoreboardLine.linePlayer != null && !playerScoreboardLine.linePlayer.IsLocal)
        {
          VRRig vrRig = (VRRig) null;
          foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
          {
            if (((Object) activeRig != (Object) null) && activeRig.Creator != null && activeRig.Creator.Equals((object) playerScoreboardLine.linePlayer))
            {
              vrRig = activeRig;
              break;
            }
          }
          if (!((Object) vrRig == (Object) null))
          {
            if ((double) Vector3.Distance(((Component) vrRig).transform.position, transform.position) < (double) num)
              ((Behaviour) playerScoreboardLine).enabled = false;
            else if (!((Behaviour) playerScoreboardLine).enabled)
              ((Behaviour) playerScoreboardLine).enabled = true;
          }
        }
      }
      if (this.state.IsOn("Anti Report Visualizer"))
      {
        if (((Object) this.antiReportSphere == (Object) null))
        {
          this.antiReportSphere = GameObject.CreatePrimitive((PrimitiveType) 0);
          Object.Destroy((Object) this.antiReportSphere.GetComponent<Collider>());
          this.antiReportMaterial = new Material(Shader.Find("Standard"));
          this.antiReportMaterial.SetFloat("_Mode", 3f);
          this.antiReportMaterial.SetInt("_SrcBlend", 5);
          this.antiReportMaterial.SetInt("_DstBlend", 10);
          this.antiReportMaterial.SetInt("_ZWrite", 0);
          this.antiReportMaterial.DisableKeyword("_ALPHATEST_ON");
          this.antiReportMaterial.EnableKeyword("_ALPHABLEND_ON");
          this.antiReportMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
          this.antiReportMaterial.renderQueue = 3000;
          this.antiReportMaterial.SetInt("_Cull", 0);
          this.antiReportSphere.GetComponent<Renderer>().sharedMaterial = this.antiReportMaterial;
        }
        Color color = Color.red;
        try
        {
          color = MenuPalette.Right(this.state, Time.unscaledTime);
        }
        catch
        {
        }
        color.a = 0.15f;
        this.antiReportMaterial.color = color;
        this.antiReportSphere.transform.position = transform.position;
        this.antiReportSphere.transform.localScale = ((Vector3.one * num) * 2f);
        if (this.antiReportSphere.activeSelf)
          return;
        this.antiReportSphere.SetActive(true);
      }
      else
        this.ClearAntiReportSphere();
    }
    catch
    {
    }
  }
  public void ClearAntiReportSphere()
  {
    try
    {
      if (((Object) this.antiReportSphere != (Object) null))
        Object.Destroy((Object) this.antiReportSphere);
    }
    catch
    {
    }
    this.antiReportSphere = (GameObject) null;
    if (((Object) this.antiReportMaterial != (Object) null))
      Object.Destroy((Object) this.antiReportMaterial);
    this.antiReportMaterial = (Material) null;
  }
  public void FlushRPCs()
  {
    try
    {
      if (((Object) MonkeAgent.instance != (Object) null))
      {
        MonkeAgent.instance.rpcCallLimit = 99999999;
        MonkeAgent.instance.rpcErrorMax = 99999999;
      }
      PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
      PhotonNetwork.RemoveRPCsInGroup(99999999);
      PhotonNetwork.RemoveBufferedRPCs(99999999, (string) null, (int[]) null);
    }
    catch
    {
    }
  }
  public void UnlimitedRPCs(bool on)
  {
    try
    {
      if (((Object) MonkeAgent.instance == (Object) null))
        return;
      if (on)
      {
        MonkeAgent.instance.rpcCallLimit = 999999999;
        MonkeAgent.instance.rpcErrorMax = 999999999;
      }
      else
      {
        MonkeAgent.instance.rpcCallLimit = 50;
        MonkeAgent.instance.rpcErrorMax = 10;
      }
    }
    catch
    {
    }
  }
  private static void ForceSerialize()
  {
    try
    {
      if (DetectedPorts.runViewUpdate == (MethodInfo) null)
      {
        DetectedPorts.runViewUpdate = typeof (PhotonNetwork).GetMethod("RunViewUpdate", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        Debug.Log((object) ("Tag: RunViewUpdate " + (DetectedPorts.runViewUpdate != (MethodInfo) null ? "resolved." : "MISSING.")));
      }
      if (!(DetectedPorts.runViewUpdate != (MethodInfo) null))
        return;
      DetectedPorts.runViewUpdate.Invoke((object) null, (object[]) null);
    }
    catch (Exception ex)
    {
      Debug.Log((object) ("Tag: ForceSerialize failed: " + ex.Message));
    }
  }
  public static void AntiRPCKick()
  {
    try
    {
      if (!PhotonNetwork.InRoom)
        return;
      MonkeAgent instance = MonkeAgent.instance;
      if (((Object) instance != (Object) null))
      {
        instance.rpcErrorMax = int.MaxValue;
        instance.rpcCallLimit = int.MaxValue;
        instance.logErrorMax = int.MaxValue;
      }
      PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
      PhotonNetwork.QuickResends = int.MaxValue;
      PhotonNetwork.RemoveBufferedRPCs(int.MaxValue, (string) null, (int[]) null);
      PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
      PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
    }
    catch
    {
    }
  }
  public void BypassAutoMuteTick()
  {
    try
    {
      if (!PhotonNetwork.InRoom || (double) Time.time < (double) this.muteMaintenanceAt + 0.10000000149011612)
        return;
      this.muteMaintenanceAt = Time.time;
      GorillaTagger.moderationMutedTime = -1f;
      GorillaComputer instance = GorillaComputer.instance;
      if (((Object) instance != (Object) null) && instance.autoMuteType != "OFF")
      {
        instance.autoMuteType = "OFF";
        PlayerPrefs.SetInt("autoMute", 0);
        PlayerPrefs.Save();
      }
      Recorder myRecorder = GorillaTagger.Instance.myRecorder;
      if (((Object) myRecorder == (Object) null) || myRecorder.SourceType != null)
        return;
      GorillaSpeakerLoudness component = ((Component) VRRig.LocalRig).GetComponent<GorillaSpeakerLoudness>();
      if (((Object) component == (Object) null))
        return;
      float loudness = component.Loudness;
      if ((double) loudness != 0.0)
      {
        this.silenceStartedAt = -1f;
        this.recordingRestarted = false;
        this.prevLoudness = loudness;
      }
      else
      {
        if ((double) this.prevLoudness != 0.0)
        {
          this.silenceStartedAt = Time.time;
          this.recordingRestarted = false;
        }
        if ((double) this.silenceStartedAt > 0.0 && !this.recordingRestarted && (double) Time.time - (double) this.silenceStartedAt >= 0.25)
        {
          myRecorder.RestartRecording(true);
          this.recordingRestarted = true;
        }
        this.prevLoudness = loudness;
      }
    }
    catch
    {
    }
  }
  private async void XyferTag(VRRig target)
  {
    if (this.tagBusy)
      return;
    this.tagBusy = true;
    try
    {
      int num = await this.XyferTagAsync(target) ? 1 : 0;
    }
    finally
    {
      this.tagBusy = false;
    }
  }
  private async Task<bool> XyferTagAsync(VRRig target)
  {
    try
    {
      if (((Object) target == (Object) null) || target.isOfflineVRRig || !PhotonNetwork.InRoom)
      {
        Debug.Log((object) "Tag: no target/room.");
        return false;
      }
      NetPlayer netPlayer = DetectedPorts.GetNetPlayer(target);
      if (netPlayer == null)
      {
        Debug.Log((object) "Tag: no NetPlayer.");
        return false;
      }
      if (DetectedPorts.ManagerIsTagged(netPlayer))
      {
        Debug.Log((object) "Tag: already tagged.");
        return false;
      }
      if (NetworkSystem.Instance.IsMasterClient)
      {
        try
        {
          GorillaGameManager.instance.ReportTag(netPlayer, NetworkSystem.Instance.LocalPlayer);
          Debug.Log((object) "Tag: master report sent.");
        }
        catch (Exception ex)
        {
          Debug.Log((object) ("Tag: master report failed: " + ex.Message));
        }
        return true;
      }
      if ((double) Time.time < (double) this.tagReportAt)
        return false;
      RigPatchKit.SpoofPosition = new Vector3?((((Component) target).transform.position - new Vector3(0.0f, 3f, 0.0f)));
      try
      {
        DetectedPorts.ForceSerialize();
        PhotonNetwork.SendAllOutgoingCommands();
        this.tagReportAt = Time.time + 0.1f;
        try
        {
          GameMode.ReportTag(netPlayer);
          Debug.Log((object) "Tag: spoof report sent.");
        }
        catch (Exception ex)
        {
          Debug.Log((object) ("Tag: report failed: " + ex.Message));
          return false;
        }
      }
      finally
      {
        RigPatchKit.SpoofPosition = new Vector3?();
      }
      PhotonNetwork.SendAllOutgoingCommands();
      DetectedPorts.AntiRPCKick();
      return true;
    }
    catch (Exception ex)
    {
      Debug.Log((object) ("Tag: failed: " + ex.Message));
      return false;
    }
  }
  public void TagSelf()
  {
    try
    {
      if (!PhotonNetwork.InRoom)
        return;
      NetPlayer localPlayer = NetworkSystem.Instance.LocalPlayer;
      if (localPlayer == null)
        return;
      if (PhotonNetwork.IsMasterClient)
      {
        DetectedPorts.InfectExe(localPlayer);
      }
      else
      {
        VRRig target = (VRRig) null;
        foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
        {
          if (!DetectedPorts.IsLocal(activeRig))
          {
            NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
            if (netPlayer != null && DetectedPorts.ManagerIsTagged(netPlayer))
            {
              target = activeRig;
              break;
            }
          }
        }
        if (((Object) target == (Object) null))
        {
          Debug.Log((object) "Tag Self: no tagged player nearby.");
        }
        else
        {
          this.XyferTag(target);
          try
          {
            GameMode.ReportTag(localPlayer);
          }
          catch
          {
          }
        }
      }
    }
    catch
    {
    }
  }
  public async void TagAll()
  {
  }
  public void TagAura()
  {
    try
    {
      if (!PhotonNetwork.InRoom || (double) Time.time < (double) this.tagAuraDelay)
        return;
      this.tagAuraDelay = Time.time + 0.5f;
      foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
      {
        if (!DetectedPorts.IsLocal(activeRig))
        {
          NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
          if (netPlayer != null && !DetectedPorts.ManagerIsTagged(netPlayer) && (double) DetectedPorts.RigDistance(activeRig) <= 4.0)
            this.XyferTag(activeRig);
        }
      }
    }
    catch
    {
    }
  }
  private static bool ManagerIsTagged(NetPlayer player)
  {
    try
    {
      if (player != null)
      {
        if (((Object) GorillaGameManager.instance != (Object) null))
          return GorillaGameManager.instance.LocalIsTagged(player);
      }
    }
    catch
    {
    }
    return false;
  }
  public static VRRig[] ActiveRigs()
  {
    try
    {
      return VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (r => ((Object) r != (Object) null) && !r.isOfflineVRRig && !r.isLocal)).ToArray<VRRig>();
    }
    catch
    {
      return new VRRig[0];
    }
  }
  public static bool IsLocal(VRRig rig)
  {
    if (((Object) rig == (Object) null))
      return true;
    try
    {
      if (rig.isLocal)
        return true;
    }
    catch
    {
    }
    try
    {
      if (rig.isOfflineVRRig)
        return true;
    }
    catch
    {
    }
    try
    {
      if (((Object) rig == (Object) VRRig.LocalRig))
        return true;
    }
    catch
    {
    }
    return false;
  }
  public static PhotonView GetView(VRRig rig)
  {
    try
    {
      if (((Object) rig == (Object) null))
        return (PhotonView) null;
      try
      {
        PhotonView component = ((Component) rig).GetComponent<PhotonView>();
        if (((Object) component != (Object) null))
          return component;
      }
      catch
      {
      }
      try
      {
        if (Traverse.Create((object) rig).Field("netView").GetValue() is NetworkView networkView)
        {
          if (((Object) networkView != (Object) null))
          {
            if (((Object) networkView.GetView != (Object) null))
              return networkView.GetView;
          }
        }
      }
      catch
      {
      }
      try
      {
        if (Traverse.Create((object) rig).Field("photonView").GetValue() is PhotonView view)
        {
          if (((Object) view != (Object) null))
            return view;
        }
      }
      catch
      {
      }
    }
    catch
    {
    }
    return (PhotonView) null;
  }
  public static Player GetPhotonPlayer(VRRig rig)
  {
    try
    {
      PhotonView view = DetectedPorts.GetView(rig);
      if (((Object) view != (Object) null))
        return view.Owner;
    }
    catch
    {
    }
    return (Player) null;
  }
  public static NetPlayer GetNetPlayer(VRRig rig)
  {
    try
    {
      if (((Object) rig != (Object) null))
      {
        if (rig.Creator != null)
          return rig.Creator;
      }
    }
    catch
    {
    }
    try
    {
      if (((Object) rig != (Object) null))
      {
        if (rig.OwningNetPlayer != null)
          return rig.OwningNetPlayer;
      }
    }
    catch
    {
    }
    try
    {
      Player photonPlayer = DetectedPorts.GetPhotonPlayer(rig);
      if (photonPlayer != null)
      {
        foreach (NetPlayer playerListOther in NetworkSystem.Instance.PlayerListOthers)
        {
          if (playerListOther.ActorNumber == photonPlayer.ActorNumber)
            return playerListOther;
        }
      }
    }
    catch
    {
    }
    return (NetPlayer) null;
  }
  public static float RigDistance(VRRig rig)
  {
    try
    {
      return Vector3.Distance(((Component) GorillaTagger.Instance.bodyCollider).transform.position, ((Component) rig).transform.position);
    }
    catch
    {
      return 999f;
    }
  }
  public static List<VRRig> AuraRigs(float radius = 4f)
  {
    List<VRRig> vrRigList = new List<VRRig>();
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (!DetectedPorts.IsLocal(activeRig) && (double) DetectedPorts.RigDistance(activeRig) < (double) radius)
        vrRigList.Add(activeRig);
    }
    return vrRigList;
  }
  public static bool IsTouched(VRRig rig, float radius = 1f)
  {
    try
    {
      Vector3 position1 = GorillaTagger.Instance.leftHandTransform.position;
      Vector3 position2 = GorillaTagger.Instance.rightHandTransform.position;
      Vector3 position3 = ((Component) rig).transform.position;
      Vector3 vector3 = position3;
      return (double) Vector3.Distance(position1, vector3) < (double) radius || (double) Vector3.Distance(position2, position3) < (double) radius;
    }
    catch
    {
      return false;
    }
  }
  private bool PropDelay()
  {
    if ((double) Time.time <= (double) this.customPropertyDelay)
      return false;
    this.customPropertyDelay = Time.time + 0.25f;
    return true;
  }
  private bool AllowSend()
  {
    try
    {
      float realtimeSinceStartup = Time.realtimeSinceStartup;
      lock (this.sendTimes)
      {
        while (this.sendTimes.Count > 0 && (double) realtimeSinceStartup - (double) this.sendTimes.Peek() >= 1.0)
        {
          double num = (double) this.sendTimes.Dequeue();
        }
        if (this.sendTimes.Count >= this.sendCap)
          return false;
        this.sendTimes.Enqueue(realtimeSinceStartup);
        return true;
      }
    }
    catch
    {
      return true;
    }
  }
  private static bool IsSelf(NetPlayer plr)
  {
    if (plr == null)
      return true;
    try
    {
      if (plr.IsLocal)
        return true;
    }
    catch
    {
    }
    try
    {
      NetPlayer localPlayer = NetworkSystem.Instance.LocalPlayer;
      if (localPlayer != null)
      {
        if (plr.ActorNumber == localPlayer.ActorNumber)
          return true;
      }
    }
    catch
    {
    }
    try
    {
      Player localPlayer = PhotonNetwork.LocalPlayer;
      if (localPlayer != null)
      {
        if (plr.ActorNumber == localPlayer.ActorNumber)
          return true;
      }
    }
    catch
    {
    }
    return false;
  }
  public void HitPlayer(NetPlayer plr, int times = 1)
  {
    if (plr == null)
      return;
    if (DetectedPorts.IsSelf(plr))
      return;
    try
    {
      ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
      hashtable.Add((byte) 0, (object) plr.ActorNumber);
      RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
      raiseEventOptions.TargetActors = new int[1]
      {
        plr.ActorNumber
      };
      for (int index = 0; index < times && this.AllowSend(); ++index)
        PhotonNetwork.NetworkingClient.OpRaiseEvent((byte) 207, (object) hashtable, raiseEventOptions, SendOptions.SendReliable);
    }
    catch
    {
    }
  }
  public void RaiseDestroy(VRRig rig, int viewID = -1, int[] targets = null)
  {
    try
    {
      PhotonView view = DetectedPorts.GetView(rig);
      if (((Object) view == (Object) null))
        return;
      ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
      hashtable.Add((byte) 0, (object) (viewID == -1 ? view.ViewID : viewID));
      RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
      if (targets != null)
      {
        raiseEventOptions.TargetActors = targets;
      }
      else
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(rig);
        raiseEventOptions.TargetActors = new int[1]
        {
          netPlayer != null ? netPlayer.ActorNumber : 0
        };
      }
      PhotonNetwork.NetworkingClient.OpRaiseEvent((byte) 204, (object) hashtable, raiseEventOptions, SendOptions.SendReliable);
    }
    catch
    {
    }
  }
  private static int[] OthersThan(PhotonView view)
  {
    try
    {
      return ((IEnumerable<Player>) PhotonNetwork.PlayerList).Where<Player>((Func<Player, bool>) (p => p != view.Owner)).Select<Player, int>((Func<Player, int>) (p => p.ActorNumber)).ToArray<int>();
    }
    catch
    {
      return new int[0];
    }
  }
  private void ArmCapture(string mode)
  {
    this.captureMode = mode;
    this.captureArmedAt = Time.time;
    try
    {
      this.state.Type("Clear");
    }
    catch
    {
    }
    try
    {
      this.state.OpenSearch();
    }
    catch
    {
    }
    Debug.Log((object) ("Type now, then Done: " + mode));
  }
  private void PollCapture()
  {
    if (this.captureMode == null)
      return;
    try
    {
      if (this.state.KeyboardOpen)
      {
        if ((double) Time.time - (double) this.captureArmedAt < 60.0)
          return;
        this.captureMode = (string) null;
      }
      else
      {
        string captureMode = this.captureMode;
        this.captureMode = (string) null;
        string str = "";
        try
        {
          str = this.state.Query ?? "";
        }
        catch
        {
        }
        switch (captureMode)
        {
          case "code":
            if (str.Length <= 0)
              break;
            this.antibanCode = str.ToUpperInvariant();
            if (this.antibanCode.Length > 10)
              this.antibanCode = this.antibanCode.Substring(0, 10);
            Debug.Log((object) ("Antiban code: " + this.antibanCode));
            this.JoinCurrentCode();
            break;
          case "name":
            if (str.Length <= 0)
              break;
            this.customName = str.Length > 12 ? str.Substring(0, 12) : str;
            Debug.Log((object) ("Name set: " + this.customName));
            break;
          case "room":
            if (str.Length <= 0)
              break;
            string code = str.ToUpperInvariant();
            if (code.Length > 10)
              code = code.Substring(0, 10);
            this.JoinRoomCode(code);
            break;
        }
      }
    }
    catch
    {
      this.captureMode = (string) null;
    }
  }
  public async void JoinRoomCode(string code)
  {
    if (string.IsNullOrEmpty(code) || this.joining)
      return;
    this.joining = true;
    try
    {
      PhotonNetworkController instance = PhotonNetworkController.Instance;
      NetworkSystem network = NetworkSystem.Instance;
      if (((Object) instance == (Object) null) || ((Object) network == (Object) null) || ((Object) GorillaComputer.instance == (Object) null))
      {
        Debug.Log((object) "Join Room: networking not ready.");
        this.joining = false;
      }
      else
      {
        GorillaNetworkJoinTrigger trigger = DetectedPorts.ResolveTrigger();
        if (((Object) trigger == (Object) null))
        {
          Debug.Log((object) "Join Room: no map trigger loaded.");
          this.joining = false;
        }
        else
        {
          RoomConfig roomConfig = DetectedPorts.BuildConfig(instance, trigger, true, trigger.GetFullDesiredGameModeString());
          instance.currentJoinTrigger = trigger;
          AccessTools.Field(typeof (PhotonNetworkController), "currentJoinType")?.SetValue((object) instance, (object) (JoinType) 0);
          Debug.Log((object) ("Join Room: joining " + code));
          NetJoinResult result = await network.ConnectToRoom(code, roomConfig, -1);
          await Task.Delay(1500);
          try
          {
            int length = network.PlayerListOthers.Length;
            string str = "";
            try
            {
              str = network.GameModeString;
            }
            catch
            {
            }
            Debug.Log((object) $"Join Room: code={code} result={result.ToString()} others={length.ToString()} modded={(str != null && str.Contains("MODDED_")).ToString()}");
          }
          catch
          {
          }
          network = (NetworkSystem) null;
        }
      }
    }
    finally
    {
      this.joining = false;
    }
  }
  private static string RandomCode(int len = 6)
  {
    System.Random r = new System.Random();
    return new string(Enumerable.Repeat<string>("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", len).Select<string, char>((Func<string, char>) (s => s[r.Next(s.Length)])).ToArray<char>());
  }
  public async void JoinCurrentCode()
  {
    string code = (this.antibanCode ?? "").ToUpperInvariant();
    if (string.IsNullOrEmpty(code))
    {
      Debug.Log((object) "Antiban Join: no code stored.");
      code = (string) null;
    }
    else if (this.joining)
    {
      code = (string) null;
    }
    else
    {
      this.joining = true;
      try
      {
        PhotonNetworkController instance = PhotonNetworkController.Instance;
        NetworkSystem network = NetworkSystem.Instance;
        if (((Object) instance == (Object) null) || ((Object) network == (Object) null) || ((Object) GorillaComputer.instance == (Object) null))
        {
          Debug.Log((object) "Antiban Join: networking not ready.");
          this.joining = false;
          code = (string) null;
        }
        else
        {
          GorillaNetworkJoinTrigger trigger = DetectedPorts.ResolveTrigger();
          if (((Object) trigger == (Object) null))
          {
            Debug.Log((object) "Antiban Join: no map trigger loaded.");
            this.joining = false;
            code = (string) null;
          }
          else
          {
            string[] strArray = trigger.GetFullDesiredGameModeString().Split(';');
            if (strArray.Length < 3)
            {
              Debug.Log((object) "Antiban Join: bad mode string.");
              this.joining = false;
              code = (string) null;
            }
            else
            {
              strArray[2] = "MODDED_CASUAL";
              RoomConfig roomConfig = DetectedPorts.BuildConfig(instance, trigger, false, string.Join(";", strArray));
              try
              {
                instance.customRoomID = code;
                instance.isPrivate = true;
              }
              catch
              {
              }
              instance.currentJoinTrigger = trigger;
              AccessTools.Field(typeof (PhotonNetworkController), "currentJoinType")?.SetValue((object) instance, (object) (JoinType) 0);
              Debug.Log((object) ("Antiban Join: joining " + code));
              NetJoinResult result = await network.ConnectToRoom(code, roomConfig, -1);
              await Task.Delay(1500);
              try
              {
                int length = network.PlayerListOthers.Length;
                string str = "";
                try
                {
                  str = network.GameModeString;
                }
                catch
                {
                }
                bool flag = str != null && str.Contains("MODDED_");
                Debug.Log((object) $"Antiban Join: code={code} result={result.ToString()} others={length.ToString()} modded={flag.ToString()}");
              }
              catch
              {
              }
              network = (NetworkSystem) null;
              code = (string) null;
            }
          }
        }
      }
      finally
      {
        this.joining = false;
      }
    }
  }
  private static GorillaNetworkJoinTrigger ResolveTrigger()
  {
    try
    {
      PhotonNetworkController instance = PhotonNetworkController.Instance;
      GorillaNetworkJoinTrigger currentJoinTrigger = ((Object) instance != (Object) null) ? instance.currentJoinTrigger : (GorillaNetworkJoinTrigger) null;
      if (((Object) currentJoinTrigger != (Object) null) && currentJoinTrigger.networkZone != "private")
        return currentJoinTrigger;
      GorillaNetworkJoinTrigger networkJoinTrigger1 = (GorillaNetworkJoinTrigger) null;
      float num = float.PositiveInfinity;
      Vector3 position = ((Component) GorillaTagger.Instance).transform.position;
      foreach (GorillaNetworkJoinTrigger networkJoinTrigger2 in Object.FindObjectsByType<GorillaNetworkJoinTrigger>((FindObjectsSortMode) 0))
      {
        if (!((Object) networkJoinTrigger2 == (Object) null) && !(networkJoinTrigger2.networkZone == "private"))
        {
          Vector3 vector3 = (((Component) networkJoinTrigger2).transform.position - position);
          float sqrMagnitude = (vector3).sqrMagnitude;
          if ((double) sqrMagnitude < (double) num)
          {
            num = sqrMagnitude;
            networkJoinTrigger1 = networkJoinTrigger2;
          }
        }
      }
      return networkJoinTrigger1;
    }
    catch
    {
      return (GorillaNetworkJoinTrigger) null;
    }
  }
  private static RoomConfig BuildConfig(
    PhotonNetworkController controller,
    GorillaNetworkJoinTrigger trigger,
    bool isPublic,
    string gameMode)
  {
    Type type = typeof (PhotonNetworkController).Assembly.GetType("RoomSystem", true);
    GameModeType result = (GameModeType) 0;
    try
    {
      if (!Enum.TryParse<GameModeType>(GorillaComputer.instance.currentGameMode.Value, true, out result))
        result = (GameModeType) 0;
    }
    catch
    {
    }
    byte num = (byte) AccessTools.Method(type, "GetRoomSizeForCreate", (Type[]) null, (Type[]) null).Invoke((object) null, new object[4]
    {
      (object) trigger.zone,
      (object) result,
      (object) false,
      (object) false
    });
    RoomConfig roomConfig = new RoomConfig();
    roomConfig.createIfMissing = true;
    roomConfig.isJoinable = true;
    roomConfig.isPublic = isPublic;
    roomConfig.MaxPlayers = num;
    roomConfig.CustomProps[(object) nameof (gameMode)] = (object) gameMode;
    ExitGames.Client.Photon.Hashtable customProps = roomConfig.CustomProps;
    if (!(AccessTools.Field(typeof (PhotonNetworkController), "platformTag")?.GetValue((object) controller) is string str))
      str = "OTHER";
    customProps[(object) "platform"] = (object) str;
    roomConfig.CustomProps[(object) "queueName"] = (object) GorillaComputer.instance.currentQueue;
    return roomConfig;
  }
  private static bool InStump(GorillaFriendCollider stump, string userId)
  {
    try
    {
      return stump.playerIDsCurrentlyTouching.Contains(((GorillaFriendCollider.TouchingPlayerInfo)userId));
    }
    catch
    {
      return false;
    }
  }
  public static void BetaFollow(GorillaFriendCollider stump, NetPlayer target)
  {
    try
    {
      PhotonNetworkController instance = PhotonNetworkController.Instance;
      try
      {
        instance.FriendIDList.Add(target.UserId);
      }
      catch (Exception ex)
      {
        Debug.Log((object) ("Follow: FriendIDList add failed: " + ex.Message));
      }
      object[] objArray = new object[2]
      {
        (object) instance.shuffler,
        (object) instance.keyStr
      };
      NetEventOptions netEventOptions = new NetEventOptions();
      netEventOptions.TargetActors = new int[1]
      {
        target.ActorNumber
      };
      if (!DetectedPorts.InStump(stump, NetworkSystem.Instance.LocalPlayer.UserId) || !DetectedPorts.InStump(stump, target.UserId) || target.IsLocal)
        return;
      Type type = typeof (PhotonNetworkController).Assembly.GetType("RoomSystem");
      if (type == (Type) null)
        return;
      MethodInfo method = type.GetMethod("SendEvent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, (Binder) null, new Type[4]
      {
        typeof (byte),
        typeof (object[]),
        typeof (NetEventOptions).MakeByRefType(),
        typeof (bool)
      }, (ParameterModifier[]) null);
      if (method == (MethodInfo) null)
        return;
      method.Invoke((object) null, new object[4]
      {
        (object) (byte) 4,
        (object) objArray,
        (object) netEventOptions,
        (object) false
      });
    }
    catch (Exception ex)
    {
      Debug.Log((object) ("Follow failed: " + ex?.ToString()));
    }
  }
  public async void StumpKickAntiban()
  {
    try
    {
      if (!NetworkSystem.Instance.InRoom || !NetworkSystem.Instance.SessionIsPrivate)
        return;
      GorillaFriendCollider stump = (GorillaFriendCollider) null;
      try
      {
        stump = GorillaComputer.instance.friendJoinCollider;
      }
      catch
      {
      }
      if (((Object) stump == (Object) null) || !DetectedPorts.InStump(stump, NetworkSystem.Instance.LocalPlayer.UserId))
        return;
      string newCode = DetectedPorts.RandomCode(10);
      try
      {
        PhotonNetworkController.Instance.FriendIDList.Clear();
      }
      catch
      {
      }
      try
      {
        PhotonNetworkController.Instance.shuffler = UnityEngine.Random.Range(0, 99).ToString().PadLeft(2, '0') + UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
        PhotonNetworkController.Instance.keyStr = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
      }
      catch
      {
      }
      foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
      {
        try
        {
          NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
          if (netPlayer != null)
          {
            if (!netPlayer.IsLocal)
            {
              if (DetectedPorts.InStump(stump, netPlayer.UserId))
                DetectedPorts.BetaFollow(stump, netPlayer);
            }
          }
        }
        catch
        {
        }
      }
      await Task.Delay(300);
      this.antibanCode = newCode;
      this.JoinCurrentCode();
      newCode = (string) null;
    }
    catch
    {
    }
  }
  public async void StumpKickAll()
  {
    try
    {
      if (!NetworkSystem.Instance.InRoom || !NetworkSystem.Instance.SessionIsPrivate)
        return;
      GorillaFriendCollider stump = (GorillaFriendCollider) null;
      try
      {
        stump = GorillaComputer.instance.friendJoinCollider;
      }
      catch
      {
      }
      if (((Object) stump == (Object) null))
        return;
      if (!DetectedPorts.InStump(stump, NetworkSystem.Instance.LocalPlayer.UserId))
        return;
      try
      {
        PhotonNetworkController.Instance.FriendIDList.Clear();
      }
      catch
      {
      }
      try
      {
        PhotonNetworkController instance1 = PhotonNetworkController.Instance;
        int num = UnityEngine.Random.Range(0, 99);
        string str1 = num.ToString().PadLeft(2, '0');
        num = UnityEngine.Random.Range(0, 99999999);
        string str2 = num.ToString().PadLeft(8, '0');
        string str3 = str1 + str2;
        instance1.shuffler = str3;
        PhotonNetworkController instance2 = PhotonNetworkController.Instance;
        num = UnityEngine.Random.Range(0, 99999999);
        string str4 = num.ToString().PadLeft(8, '0');
        instance2.keyStr = str4;
      }
      catch
      {
      }
      foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
      {
        try
        {
          NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
          if (netPlayer != null)
          {
            if (!netPlayer.IsLocal)
            {
              if (DetectedPorts.InStump(stump, netPlayer.UserId))
                DetectedPorts.BetaFollow(stump, netPlayer);
            }
          }
        }
        catch
        {
        }
      }
      await Task.Delay(300);
      string str = DetectedPorts.RandomCode(8);
      try
      {
        PhotonNetworkController.Instance.customRoomID = str;
        PhotonNetworkController.Instance.isPrivate = true;
        await PhotonNetworkController.Instance.AttemptToJoinSpecificRoomAsync(str, (JoinType) 1, (Action<NetJoinResult>) null, -1);
      }
      catch
      {
      }
    }
    catch
    {
    }
  }
  public async void KickGun(VRRig target)
  {
    try
    {
      if (((Object) target == (Object) null) || DetectedPorts.IsLocal(target) || !NetworkSystem.Instance.InRoom || !this.state.PrivateModdedRoom)
        return;
      GorillaFriendCollider stump = (GorillaFriendCollider) null;
      try
      {
        stump = GorillaComputer.instance.friendJoinCollider;
      }
      catch
      {
      }
      if (((Object) stump == (Object) null) || !DetectedPorts.InStump(stump, NetworkSystem.Instance.LocalPlayer.UserId))
        return;
      NetPlayer netPlayer = DetectedPorts.GetNetPlayer(target);
      if (netPlayer == null || netPlayer.IsLocal || !DetectedPorts.InStump(stump, netPlayer.UserId))
        return;
      DetectedPorts.RPCProtection();
      try
      {
        PhotonNetworkController.Instance.FriendIDList.Clear();
      }
      catch
      {
      }
      try
      {
        PhotonNetworkController instance1 = PhotonNetworkController.Instance;
        string str1 = UnityEngine.Random.Range(0, 99).ToString().PadLeft(2, '0');
        int num = UnityEngine.Random.Range(0, 99999999);
        string str2 = num.ToString().PadLeft(8, '0');
        string str3 = str1 + str2;
        instance1.shuffler = str3;
        PhotonNetworkController instance2 = PhotonNetworkController.Instance;
        num = UnityEngine.Random.Range(0, 99999999);
        string str4 = num.ToString().PadLeft(8, '0');
        instance2.keyStr = str4;
      }
      catch
      {
      }
      DetectedPorts.BetaFollow(stump, netPlayer);
      Debug.Log((object) $"Kick: gun follow sent for {netPlayer.NickName}.");
      await Task.Delay(300);
      await this.JoinRoomByCode(DetectedPorts.RandomCode(8));
    }
    catch
    {
    }
  }
  public async void KickAll()
  {
    try
    {
      if (!NetworkSystem.Instance.InRoom || !this.state.PrivateModdedRoom)
        return;
      GorillaFriendCollider stump = (GorillaFriendCollider) null;
      try
      {
        stump = GorillaComputer.instance.friendJoinCollider;
      }
      catch
      {
      }
      if (((Object) stump == (Object) null) || !DetectedPorts.InStump(stump, NetworkSystem.Instance.LocalPlayer.UserId))
        return;
      DetectedPorts.RPCProtection();
      try
      {
        PhotonNetworkController.Instance.FriendIDList.Clear();
      }
      catch
      {
      }
      try
      {
        PhotonNetworkController instance1 = PhotonNetworkController.Instance;
        string str1 = UnityEngine.Random.Range(0, 99).ToString().PadLeft(2, '0');
        int num = UnityEngine.Random.Range(0, 99999999);
        string str2 = num.ToString().PadLeft(8, '0');
        string str3 = str1 + str2;
        instance1.shuffler = str3;
        PhotonNetworkController instance2 = PhotonNetworkController.Instance;
        num = UnityEngine.Random.Range(0, 99999999);
        string str4 = num.ToString().PadLeft(8, '0');
        instance2.keyStr = str4;
      }
      catch
      {
      }
      int num1 = 0;
      foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
      {
        try
        {
          NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
          if (netPlayer != null)
          {
            if (!netPlayer.IsLocal)
            {
              if (DetectedPorts.InStump(stump, netPlayer.UserId))
              {
                DetectedPorts.BetaFollow(stump, netPlayer);
                ++num1;
              }
            }
          }
        }
        catch
        {
        }
      }
      Debug.Log((object) $"Kick: all follow sent to {num1.ToString()}.");
      await Task.Delay(300);
      await this.JoinRoomByCode(DetectedPorts.RandomCode(8));
    }
    catch
    {
    }
  }
  private async Task JoinRoomByCode(string room)
  {
    try
    {
      PhotonNetworkController.Instance.customRoomID = room;
      PhotonNetworkController.Instance.isPrivate = true;
      await PhotonNetworkController.Instance.AttemptToJoinSpecificRoomAsync(room, (JoinType) 1, (Action<NetJoinResult>) null, -1);
      Debug.Log((object) $"Kick: moved to {room}.");
    }
    catch (Exception ex)
    {
      Debug.Log((object) ("Kick join failed: " + ex.Message));
    }
  }
  public static void RPCProtection()
  {
    try
    {
      if (!PhotonNetwork.InRoom)
        return;
      MonkeAgent.instance.rpcErrorMax = int.MaxValue;
      MonkeAgent.instance.rpcCallLimit = int.MaxValue;
      MonkeAgent.instance.logErrorMax = int.MaxValue;
      PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
      PhotonNetwork.QuickResends = int.MaxValue;
      PhotonNetwork.SendAllOutgoingCommands();
    }
    catch
    {
    }
  }
  public void PoisonLag(int[] actors, int amount = 1000, float delay = 2f)
  {
    try
    {
      if (!NetworkSystem.Instance.InRoom || (double) Time.time < (double) this.lagDebounce || actors == null || actors.Length == 0)
        return;
      this.lagDebounce = Time.time + delay;
      RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
      raiseEventOptions.TargetActors = actors;
      raiseEventOptions.CachingOption = (EventCaching) 0;
      for (int index = 0; index < amount; ++index)
        PhotonNetwork.NetworkingClient.OpRaiseEvent((byte) 3, (object) null, raiseEventOptions, SendOptions.SendUnreliable);
      DetectedPorts.RPCProtection();
    }
    catch
    {
    }
  }
  public void PoisonLag(NetPlayer np, int amount = 1000, float delay = 2f)
  {
    if (np == null || DetectedPorts.IsSelf(np))
      return;
    this.PoisonLag(new int[1]{ np.ActorNumber }, amount, delay);
  }
  private int[] AuraActors()
  {
    List<int> intList = new List<int>();
    try
    {
      foreach (VRRig auraRig in DetectedPorts.AuraRigs())
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(auraRig);
        if (netPlayer != null && !DetectedPorts.IsSelf(netPlayer))
          intList.Add(netPlayer.ActorNumber);
      }
    }
    catch
    {
    }
    return intList.ToArray();
  }
  public void LagAll()
  {
    try
    {
      List<int> intList = new List<int>();
      foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
        if (netPlayer != null && !DetectedPorts.IsSelf(netPlayer))
          intList.Add(netPlayer.ActorNumber);
      }
      this.PoisonLag(intList.ToArray());
    }
    catch
    {
    }
  }
  public void LagAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    this.PoisonLag(this.AuraActors());
  }
  public void LagOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    List<int> intList = new List<int>();
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (DetectedPorts.IsTouched(activeRig))
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
        if (netPlayer != null && !DetectedPorts.IsSelf(netPlayer))
          intList.Add(netPlayer.ActorNumber);
      }
    }
    this.PoisonLag(intList.ToArray());
  }
  public void FreezeRig(VRRig target, int lagAmount = 1000)
  {
    this.sendCap = 480;
    try
    {
      NetPlayer netPlayer = DetectedPorts.GetNetPlayer(target);
      if (netPlayer == null || DetectedPorts.IsSelf(netPlayer))
        return;
      this.PoisonLag(new int[1]{ netPlayer.ActorNumber }, lagAmount);
      float realtimeSinceStartup = Time.realtimeSinceStartup;
      float num;
      if (this.destroyNext.TryGetValue(netPlayer.ActorNumber, out num) && (double) realtimeSinceStartup < (double) num)
        return;
      this.destroyNext[netPlayer.ActorNumber] = realtimeSinceStartup + 1f;
      PhotonView view = DetectedPorts.GetView(target);
      if (((Object) view == (Object) null))
        return;
      this.viewIdArchive[target] = view.ViewID;
      this.RaiseDestroy(target);
    }
    catch
    {
    }
  }
  private SnowballThrowable FindThrowable(string anchorName)
  {
    try
    {
      SnowballThrowable throwable1;
      if (this.projectileCache.TryGetValue(anchorName, out throwable1) && ((Object) throwable1 != (Object) null))
        return throwable1;
      float num;
      if (this.projRetryAt.TryGetValue(anchorName, out num) && (double) Time.realtimeSinceStartup - (double) num < 5.0)
        return (SnowballThrowable) null;
      SnowballMaker[] snowballMakerArray = (SnowballMaker[]) null;
      try
      {
        snowballMakerArray = new SnowballMaker[2]
        {
          SnowballMaker.leftHandInstance,
          SnowballMaker.rightHandInstance
        };
      }
      catch
      {
      }
      if (snowballMakerArray != null)
      {
        foreach (SnowballMaker snowballMaker in snowballMakerArray)
        {
          SnowballThrowable[] snowballThrowableArray = (SnowballThrowable[]) null;
          try
          {
            snowballThrowableArray = ((Object) snowballMaker != (Object) null) ? snowballMaker.snowballs : (SnowballThrowable[]) null;
          }
          catch
          {
          }
          if (snowballThrowableArray != null)
          {
            foreach (SnowballThrowable throwable2 in snowballThrowableArray)
            {
              try
              {
                if (!((Object) throwable2 == (Object) null))
                {
                  if (!((Object) ((Component) throwable2).transform == (Object) null))
                  {
                    if (!((Object) ((Component) throwable2).transform.parent == (Object) null))
                    {
                      string name = ((Object) ((Component) ((Component) throwable2).transform.parent).gameObject).name;
                      if (!(name == anchorName))
                      {
                        if (!(name == anchorName + "(Clone)"))
                          continue;
                      }
                      this.projectileCache[anchorName] = throwable2;
                      this.projRetryAt.Remove(anchorName);
                      return throwable2;
                    }
                  }
                }
              }
              catch
              {
              }
            }
          }
        }
      }
      foreach (SnowballThrowable throwable3 in Resources.FindObjectsOfTypeAll<SnowballThrowable>())
      {
        try
        {
          if (!((Object) throwable3 == (Object) null))
          {
            if (!((Object) ((Component) throwable3).transform == (Object) null))
            {
              if (!((Object) ((Component) throwable3).transform.parent == (Object) null))
              {
                string name = ((Object) ((Component) ((Component) throwable3).transform.parent).gameObject).name;
                if (!(name == anchorName) && !(name == anchorName + "(Clone)"))
                {
                  if (!(((Object) throwable3).name == anchorName))
                    continue;
                }
                this.projectileCache[anchorName] = throwable3;
                this.projRetryAt.Remove(anchorName);
                return throwable3;
              }
            }
          }
        }
        catch
        {
        }
      }
      if (this.projMissed.Add(anchorName))
        Debug.Log((object) $"Projectile: no throwable for {anchorName}.");
      this.projRetryAt[anchorName] = Time.realtimeSinceStartup;
    }
    catch
    {
    }
    return (SnowballThrowable) null;
  }
  public event Action<GameObject, Vector3, Vector3, float> SharedProjectile;
  public void ProjectileSpam(string anchorName)
  {
    float realtimeSinceStartup = Time.realtimeSinceStartup;
    float num;
    if (this.projCsNext.TryGetValue(anchorName, out num) && (double) realtimeSinceStartup < (double) num)
      return;
    this.projCsNext[anchorName] = realtimeSinceStartup + 0.03f;
    try
    {
      Transform rightHandTransform = GorillaTagger.Instance.rightHandTransform;
      Vector3 position = rightHandTransform.position;
      Vector3 vector3 = ((rightHandTransform.forward * 20f) + new Vector3((float) (this.projRandom.NextDouble() - 0.5) * 1f, (float) (this.projRandom.NextDouble() - 0.5) * 1f, (float) (this.projRandom.NextDouble() - 0.5) * 1f));
      GameObject gameObject1 = (GameObject) null;
      if (anchorName == "SlingshotProjectile")
      {
        try
        {
          if (DetectedPorts.weaponPrefabField == (FieldInfo) null)
            DetectedPorts.weaponPrefabField = typeof (ProjectileWeapon).GetField("projectilePrefab", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
          ProjectileWeapon projectileWeapon = VRRig.LocalRig.projectileWeapon;
          if (((Object) projectileWeapon != (Object) null))
            gameObject1 = DetectedPorts.weaponPrefabField.GetValue((object) projectileWeapon) as GameObject;
        }
        catch
        {
        }
      }
      else
      {
        SnowballThrowable throwable = this.FindThrowable(anchorName);
        if (((Object) throwable != (Object) null))
          gameObject1 = throwable.projectilePrefab;
      }
      if (((Object) gameObject1 == (Object) null))
      {
        if (!this.projMissed.Add("prefab:" + anchorName))
          return;
        Debug.Log((object) $"Projectile: no prefab for {anchorName}.");
      }
      else
      {
        GameObject gameObject2 = new GameObject("SpamProjectileHolder");
        try
        {
          if (DetectedPorts.addIncrementMethod == (MethodInfo) null)
          {
            DetectedPorts.projectileTrackerType = typeof (SlingshotProjectile).Assembly.GetType("ProjectileTracker");
            if (DetectedPorts.projectileTrackerType != (Type) null)
              DetectedPorts.addIncrementMethod = DetectedPorts.projectileTrackerType.GetMethod("AddAndIncrementLocalProjectile", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
          }
          if (DetectedPorts.addIncrementMethod != (MethodInfo) null)
            this.projIncrement = (int) DetectedPorts.addIncrementMethod.Invoke((object) null, new object[4]
            {
              (object) gameObject2.AddComponent<SlingshotProjectile>(),
              (object) vector3,
              (object) position,
              (object) 1f
            });
          else
            ++this.projIncrement;
        }
        catch
        {
          ++this.projIncrement;
        }
        Object.Destroy((Object) gameObject2);
        SlingshotProjectile component;
        try
        {
          component = ObjectPools.instance.Instantiate(gameObject1, true).GetComponent<SlingshotProjectile>();
        }
        catch (Exception ex)
        {
          if (!this.projMissed.Add("pool:" + anchorName))
            return;
          Debug.Log((object) $"Projectile: pool failed for {anchorName}: {ex.Message}");
          return;
        }
        if (!((Object) component != (Object) null))
          return;
        try
        {
          try
          {
            component.gravityMultiplier = 0.0f;
          }
          catch
          {
            Traverse.Create((object) component).Field("gravityMultiplier").SetValue((object) 0.0f);
          }
          component.Launch(position, vector3, NetworkSystem.Instance.LocalPlayer, false, false, this.projIncrement, 1f, true, Color.white);
          Action<GameObject, Vector3, Vector3, float> sharedProjectile = this.SharedProjectile;
          if (sharedProjectile == null)
            return;
          sharedProjectile(((Component) component).gameObject, position, vector3, 1f);
        }
        catch (Exception ex)
        {
          if (!this.projMissed.Add("launch:" + anchorName))
            return;
          Debug.Log((object) $"Projectile: launch failed for {anchorName}: {ex.Message}");
        }
      }
    }
    catch (Exception ex)
    {
      if (!this.projMissed.Add("spam:" + anchorName))
        return;
      Debug.Log((object) $"Projectile: spam failed for {anchorName}: {ex.Message}");
    }
  }
  public void ProjectileSpamTick()
  {
    foreach (KeyValuePair<string, string> projectileSpammer in DetectedPorts.projectileSpammers)
    {
      if (this.state.IsOn(projectileSpammer.Key))
        this.ProjectileSpam(projectileSpammer.Value);
    }
    if (!this.state.IsOn("Random Projectile Spammer"))
      return;
    List<string> stringList = new List<string>((IEnumerable<string>) DetectedPorts.projectileSpammers.Values);
    this.ProjectileSpam(stringList[this.projRandom.Next(stringList.Count)]);
  }
  public void DestroyAllPlayers()
  {
    try
    {
      foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
      {
        try
        {
          if (!DetectedPorts.IsLocal(activeRig))
          {
            NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
            if (netPlayer != null)
              PhotonNetwork.OpRemoveCompleteCacheOfPlayer(netPlayer.ActorNumber);
          }
        }
        catch
        {
        }
      }
    }
    catch
    {
    }
  }
  public void DestroySelf()
  {
    try
    {
      PhotonNetwork.OpRemoveCompleteCacheOfPlayer(NetworkSystem.Instance.LocalPlayer.ActorNumber);
    }
    catch
    {
    }
  }
  public void QuarantineRig(VRRig target)
  {
    PhotonView view1 = DetectedPorts.GetView(target);
    if (((Object) view1 == (Object) null))
      return;
    new ExitGames.Client.Photon.Hashtable().Add((byte) 0, (object) view1.ViewID);
    this.RaiseDestroy(target);
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (!((Object) activeRig == (Object) target))
      {
        PhotonView view2 = DetectedPorts.GetView(activeRig);
        if (!((Object) view2 == (Object) null))
        {
          NetPlayer netPlayer = DetectedPorts.GetNetPlayer(target);
          this.RaiseDestroy(activeRig, view2.ViewID, new int[1]
          {
            netPlayer != null ? netPlayer.ActorNumber : 0
          });
        }
      }
    }
  }
  public void QuarantineAll()
  {
    foreach (VRRig target in ((IEnumerable<VRRig>) DetectedPorts.ActiveRigs()).ToList<VRRig>())
      this.QuarantineRig(target);
  }
  public void QuarantineAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig target in DetectedPorts.AuraRigs().ToList<VRRig>())
      this.QuarantineRig(target);
  }
  public void QuarantineOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (DetectedPorts.IsTouched(activeRig))
        this.QuarantineRig(activeRig);
    }
  }
  public void LeaderboardQuarantine()
  {
    try
    {
      foreach (GorillaScoreBoard allScoreboard in GorillaScoreboardTotalUpdater.allScoreboards)
      {
        try
        {
          TextMeshPro buttonText = allScoreboard.buttonText;
          if (((Object) buttonText != (Object) null))
          {
            if (((TMP_Text) buttonText).text.Contains("REPORT"))
              ((TMP_Text) buttonText).text = ((TMP_Text) buttonText).text.Replace("REPORT", "QUARANTINE");
          }
        }
        catch
        {
        }
      }
      foreach (GorillaPlayerScoreboardLine playerScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines.Where<GorillaPlayerScoreboardLine>((Func<GorillaPlayerScoreboardLine, bool>) (l => l.linePlayer != NetworkSystem.Instance.LocalPlayer)))
      {
        if (playerScoreboardLine.reportInProgress)
        {
          playerScoreboardLine.SetReportState(false, (GorillaPlayerLineButton.ButtonType) 1);
          playerScoreboardLine.reportButton.isOn = true;
          playerScoreboardLine.reportButton.UpdateColor();
          if (((Object) playerScoreboardLine.playerVRRig != (Object) null))
            this.QuarantineRig(playerScoreboardLine.playerVRRig);
        }
        if (playerScoreboardLine.reportButton.isOn && playerScoreboardLine.reportInProgress)
        {
          playerScoreboardLine.SetReportState(false, (GorillaPlayerLineButton.ButtonType) 5);
          playerScoreboardLine.reportButton.isOn = false;
          playerScoreboardLine.reportButton.UpdateColor();
          this.HitPlayer(playerScoreboardLine.linePlayer);
        }
      }
    }
    catch
    {
    }
  }
  public void GhostRig(VRRig target)
  {
    PhotonView view = DetectedPorts.GetView(target);
    if (((Object) view == (Object) null))
      return;
    this.viewIdArchive[target] = view.ViewID;
    this.RaiseDestroy(target);
  }
  public void GhostAll()
  {
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      try
      {
        this.GhostRig(activeRig);
      }
      catch
      {
      }
    }
  }
  public void GhostAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig target in DetectedPorts.AuraRigs().ToList<VRRig>())
    {
      try
      {
        this.GhostRig(target);
      }
      catch
      {
      }
    }
  }
  public void GhostOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (!DetectedPorts.IsLocal(activeRig) && !this.viewIdArchive.ContainsKey(activeRig) && DetectedPorts.IsTouched(activeRig))
        this.GhostRig(activeRig);
    }
  }
  public void MalformedSplash()
  {
    try
    {
      GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlaySplashEffect", (RpcTarget) 0, new object[6]
      {
        (object) ((Component) GorillaTagger.Instance.offlineVRRig).transform.position,
        (object) Quaternion.identity,
        (object) float.NaN,
        (object) 0.5f,
        (object) false,
        (object) true
      });
      this.FlushAndReplay();
      DetectedPorts.AntiRPCKick();
    }
    catch
    {
    }
  }
  public void FlushAndReplay()
  {
    try
    {
      DetectedPorts.RPCProtection();
      PhotonNetwork.SendAllOutgoingCommands();
    }
    catch
    {
    }
  }
  public async void CrashAllOnce()
  {
    try
    {
      if (!PhotonNetwork.InRoom || !this.state.PrivateModdedRoom)
        return;
      DetectedPorts.RPCProtection();
      int num1 = 0;
      int num2 = 0;
      int num3 = 0;
      foreach (Player playerListOther in PhotonNetwork.PlayerListOthers)
      {
        Player target = playerListOther;
        try
        {
          VRRig vrRig = (VRRig) null;
          foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
          {
            NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
            if (netPlayer != null && netPlayer.ActorNumber == target.ActorNumber)
            {
              vrRig = activeRig;
              break;
            }
          }
          if (!((Object) vrRig == (Object) null))
          {
            if (!DetectedPorts.IsLocal(vrRig))
            {
              try
              {
                NetPlayer netPlayer = DetectedPorts.GetNetPlayer(vrRig);
                if (netPlayer != null)
                  this.freezeUntil[netPlayer.ActorNumber] = Time.realtimeSinceStartup + 5f;
              }
              catch
              {
              }
              this.FreezeRig(vrRig, 300);
              PhotonView view = DetectedPorts.GetView(vrRig);
              if (((Object) view == (Object) null))
              {
                ++num3;
                continue;
              }
              ++num1;
              int[] array = ((IEnumerable<Player>) PhotonNetwork.PlayerList).Where<Player>((Func<Player, bool>) (p => p.ActorNumber != target.ActorNumber)).Select<Player, int>((Func<Player, int>) (p => p.ActorNumber)).ToArray<int>();
              if (array.Length != 0)
              {
                ExitGames.Client.Photon.Hashtable hashtable1 = new ExitGames.Client.Photon.Hashtable();
                hashtable1.Add((byte) 0, (object) view.ViewID);
                ExitGames.Client.Photon.Hashtable hashtable2 = hashtable1;
                LoadBalancingClient networkingClient = PhotonNetwork.NetworkingClient;
                ExitGames.Client.Photon.Hashtable hashtable3 = hashtable2;
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
                raiseEventOptions.TargetActors = array;
                SendOptions sendReliable = SendOptions.SendReliable;
                networkingClient.OpRaiseEvent((byte) 204, (object) hashtable3, raiseEventOptions, sendReliable);
                continue;
              }
              continue;
            }
          }
          ++num2;
        }
        catch
        {
        }
      }
      Debug.Log((object) $"Crash: all fired hits={num1.ToString()} noRig={num2.ToString()} noView={num3.ToString()}.");
      this.FlushAndReplay();
      DetectedPorts.AntiRPCKick();
    }
    catch
    {
    }
  }
  public void LeaderboardCrash()
  {
    try
    {
      if (!this.state.PrivateModdedRoom)
        return;
      foreach (GorillaScoreBoard allScoreboard in GorillaScoreboardTotalUpdater.allScoreboards)
      {
        try
        {
          TextMeshPro buttonText = allScoreboard.buttonText;
          if (((Object) buttonText != (Object) null))
          {
            if (((TMP_Text) buttonText).text.Contains("REPORT"))
              ((TMP_Text) buttonText).text = ((TMP_Text) buttonText).text.Replace("REPORT", "CRASH");
          }
        }
        catch
        {
        }
      }
      foreach (GorillaPlayerScoreboardLine playerScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines.Where<GorillaPlayerScoreboardLine>((Func<GorillaPlayerScoreboardLine, bool>) (l => l.linePlayer != NetworkSystem.Instance.LocalPlayer)))
      {
        if (playerScoreboardLine.reportInProgress)
        {
          playerScoreboardLine.SetReportState(false, (GorillaPlayerLineButton.ButtonType) 1);
          playerScoreboardLine.reportButton.isOn = true;
          playerScoreboardLine.reportButton.UpdateColor();
          if (((Object) playerScoreboardLine.playerVRRig != (Object) null))
          {
            if (!DetectedPorts.IsLocal(playerScoreboardLine.playerVRRig))
            {
              try
              {
                NetPlayer netPlayer = DetectedPorts.GetNetPlayer(playerScoreboardLine.playerVRRig);
                if (netPlayer != null)
                  this.freezeUntil[netPlayer.ActorNumber] = Time.realtimeSinceStartup + 5f;
              }
              catch
              {
              }
              DetectedPorts.RPCProtection();
              this.FreezeRig(playerScoreboardLine.playerVRRig, 300);
              Debug.Log((object) "Crash: leaderboard fired.");
              PhotonView view = DetectedPorts.GetView(playerScoreboardLine.playerVRRig);
              if (((Object) view != (Object) null))
                this.RaiseDestroy(playerScoreboardLine.playerVRRig, view.ViewID, DetectedPorts.OthersThan(view));
              this.FlushAndReplay();
              DetectedPorts.AntiRPCKick();
            }
          }
        }
        if (playerScoreboardLine.reportButton.isOn && playerScoreboardLine.reportInProgress)
        {
          playerScoreboardLine.SetReportState(false, (GorillaPlayerLineButton.ButtonType) 5);
          playerScoreboardLine.reportButton.isOn = false;
          playerScoreboardLine.reportButton.UpdateColor();
          NetPlayer plr = (NetPlayer) null;
          try
          {
            plr = DetectedPorts.GetNetPlayer(playerScoreboardLine.playerVRRig);
          }
          catch
          {
          }
          if (plr != null)
            this.HitPlayer(plr);
        }
      }
    }
    catch
    {
    }
  }
  public void LeaderboardGhost()
  {
    try
    {
      foreach (GorillaScoreBoard allScoreboard in GorillaScoreboardTotalUpdater.allScoreboards)
      {
        try
        {
          TextMeshPro buttonText = allScoreboard.buttonText;
          if (((Object) buttonText != (Object) null))
          {
            if (((TMP_Text) buttonText).text.Contains("REPORT"))
              ((TMP_Text) buttonText).text = ((TMP_Text) buttonText).text.Replace("REPORT", "GHOST");
          }
        }
        catch
        {
        }
      }
      foreach (GorillaPlayerScoreboardLine playerScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines.Where<GorillaPlayerScoreboardLine>((Func<GorillaPlayerScoreboardLine, bool>) (l => l.linePlayer != NetworkSystem.Instance.LocalPlayer)))
      {
        if (playerScoreboardLine.reportInProgress)
        {
          playerScoreboardLine.SetReportState(false, (GorillaPlayerLineButton.ButtonType) 1);
          playerScoreboardLine.reportButton.isOn = true;
          playerScoreboardLine.reportButton.UpdateColor();
          if (((Object) playerScoreboardLine.playerVRRig != (Object) null))
          {
            PhotonView view = DetectedPorts.GetView(playerScoreboardLine.playerVRRig);
            if (((Object) view != (Object) null))
            {
              this.viewIdArchive[playerScoreboardLine.playerVRRig] = view.ViewID;
              this.RaiseDestroy(playerScoreboardLine.playerVRRig);
            }
          }
        }
        if (playerScoreboardLine.reportButton.isOn && playerScoreboardLine.reportInProgress)
        {
          playerScoreboardLine.SetReportState(false, (GorillaPlayerLineButton.ButtonType) 5);
          playerScoreboardLine.reportButton.isOn = false;
          playerScoreboardLine.reportButton.UpdateColor();
          try
          {
            int viewID = this.viewIdArchive[playerScoreboardLine.playerVRRig];
            this.RaiseDestroy(playerScoreboardLine.playerVRRig, viewID);
          }
          catch
          {
          }
        }
      }
    }
    catch
    {
    }
  }
  public void RevertLeaderboard()
  {
    try
    {
      foreach (GorillaScoreBoard allScoreboard in GorillaScoreboardTotalUpdater.allScoreboards)
      {
        try
        {
          TextMeshPro buttonText = allScoreboard.buttonText;
          if (!((Object) buttonText == (Object) null))
          {
            if (((TMP_Text) buttonText).text.Contains("QUARANTINE"))
              ((TMP_Text) buttonText).text = ((TMP_Text) buttonText).text.Replace("QUARANTINE", "REPORT");
            if (((TMP_Text) buttonText).text.Contains("GHOST"))
              ((TMP_Text) buttonText).text = ((TMP_Text) buttonText).text.Replace("GHOST", "REPORT");
            if (((TMP_Text) buttonText).text.Contains("CRASH"))
              ((TMP_Text) buttonText).text = ((TMP_Text) buttonText).text.Replace("CRASH", "REPORT");
          }
        }
        catch
        {
        }
      }
      foreach (GorillaPlayerScoreboardLine allScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines)
      {
        allScoreboardLine.SetReportState(false, (GorillaPlayerLineButton.ButtonType) 5);
        allScoreboardLine.reportButton.isOn = false;
        allScoreboardLine.reportButton.UpdateColor();
      }
    }
    catch
    {
    }
  }
  public void LeaderboardMute()
  {
    try
    {
      List<int> intList = new List<int>();
      foreach (VRRig rig in ((IEnumerable<VRRig>) DetectedPorts.ActiveRigs()).Where<VRRig>((Func<VRRig, bool>) (r => !DetectedPorts.IsLocal(r) && r.muted)))
      {
        try
        {
          NetPlayer netPlayer = DetectedPorts.GetNetPlayer(rig);
          if (netPlayer != null)
            intList.Add(netPlayer.ActorNumber);
        }
        catch
        {
        }
      }
      this.PoisonLag(intList.ToArray());
    }
    catch
    {
    }
  }
  public void SetRoomLocked(bool locked)
  {
    try
    {
      if (!PhotonNetwork.InRoom)
        return;
      ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
      if (locked)
      {
        hashtable[(byte) 253] = (object) false;
        hashtable[(byte) 254] = (object) false;
        hashtable[(byte) 235] = (object) 1;
        hashtable[(byte) 236] = (object) 1;
        hashtable[(byte) 249] = (object) true;
        hashtable[(object) "platform"] = (object) "CLOSED";
        hashtable[(object) "closed"] = (object) true;
      }
      else
      {
        hashtable[(byte) 253] = (object) true;
        hashtable[(byte) 254] = (object) true;
        hashtable[(byte) 235] = (object) -1;
        hashtable[(byte) 236] = (object) 0;
        hashtable[(byte) 249] = (object) false;
        hashtable[(object) "platform"] = (object) "OTHER";
        hashtable[(object) "closed"] = (object) null;
      }
      ((PhotonPeer) PhotonNetwork.CurrentRoom.LoadBalancingClient.LoadBalancingPeer).SendOperation((byte) 252, new Dictionary<byte, object>()
      {
        {
          (byte) 251,
          (object) hashtable
        },
        {
          (byte) 250,
          (object) true
        },
        {
          (byte) 231,
          (object) null
        }
      }, SendOptions.SendReliable);
      string nickName = PhotonNetwork.LocalPlayer.NickName;
      PhotonNetwork.LocalPlayer.NickName = "update";
      PhotonNetwork.LocalPlayer.NickName = nickName;
      PhotonNetwork.SendAllOutgoingCommands();
      Debug.Log(locked ? (object) "Room closed." : (object) "Room opened.");
    }
    catch (Exception ex)
    {
      Debug.Log((object) ("Room lock failed: " + ex.Message));
    }
  }
  public void VoiceMuteAll()
  {
    try
    {
      LoadBalancingTransport client = ((VoiceConnection) PhotonVoiceNetwork.Instance).Client;
      if (client == null)
        return;
      object obj;
      try
      {
        Recorder talkRecorder = DetectedPorts.GetTalkRecorder();
        obj = ((Object) talkRecorder != (Object) null) ? (object) talkRecorder.SamplingRate : (object) 48000;
      }
      catch
      {
        obj = (object) 48000;
      }
      object[] objArray = new object[3]
      {
        (object) (byte) 0,
        (object) (byte) 1,
        (object) new object[1]
        {
          (object) new Dictionary<byte, object>()
          {
            {
              (byte) 1,
              (object) (int) byte.MaxValue
            },
            {
              (byte) 2,
              obj
            },
            {
              (byte) 3,
              (object) 2
            },
            {
              (byte) 4,
              (object) 20000
            },
            {
              (byte) 5,
              (object) 30000
            },
            {
              (byte) 10,
              (object) null
            },
            {
              (byte) 11,
              (object) (byte) 0
            },
            {
              (byte) 12,
              (object) (Codec) 11
            }
          }
        }
      };
      RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
      raiseEventOptions.Receivers = (ReceiverGroup) 1;
      SendOptions sendOptions = new SendOptions();
      (sendOptions).Reliability = false;
      sendOptions.Channel = (byte) 0;
      for (int index = 0; index < 2; ++index)
        ((LoadBalancingClient) client).OpRaiseEvent((byte) 202, (object) objArray, raiseEventOptions, sendOptions);
    }
    catch
    {
    }
  }
  public void MuteAll() => this.VoiceMuteAll();
  public void MuteAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    this.PoisonLag(this.AuraActors());
  }
  public void MuteOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    List<int> intList = new List<int>();
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (DetectedPorts.IsTouched(activeRig))
      {
        NetPlayer netPlayer = DetectedPorts.GetNetPlayer(activeRig);
        if (netPlayer != null && !DetectedPorts.IsSelf(netPlayer))
          intList.Add(netPlayer.ActorNumber);
      }
    }
    this.PoisonLag(intList.ToArray());
  }
  public void UnghostRig(VRRig target)
  {
    try
    {
      int viewID;
      if (!this.viewIdArchive.TryGetValue(target, out viewID))
        return;
      this.RaiseDestroy(target, viewID);
    }
    catch
    {
    }
  }
  public void UnghostAll()
  {
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
      this.UnghostRig(activeRig);
  }
  public void UnghostAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig auraRig in DetectedPorts.AuraRigs())
      this.UnghostRig(auraRig);
  }
  public void UnghostOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (DetectedPorts.IsTouched(activeRig) && this.viewIdArchive.ContainsKey(activeRig))
        this.UnghostRig(activeRig);
    }
  }
  public void SpamGhostAll()
  {
    this.GhostAll();
    this.UnghostAll();
  }
  public void SpamGhostAura()
  {
    this.GhostAura();
    this.UnghostAura();
  }
  public void SpamGhostOnTouch()
  {
    this.GhostOnTouch();
    this.UnghostOnTouch();
  }
  public void IsolateRig(VRRig target)
  {
    NetPlayer netPlayer = DetectedPorts.GetNetPlayer(target);
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if ((!this.isolateOthers || !DetectedPorts.IsLocal(activeRig)) && !((Object) activeRig == (Object) target))
      {
        PhotonView view = DetectedPorts.GetView(activeRig);
        if (!((Object) view == (Object) null))
          this.RaiseDestroy(activeRig, view.ViewID, new int[1]
          {
            netPlayer != null ? netPlayer.ActorNumber : 0
          });
      }
    }
  }
  public void IsolateAll()
  {
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (!this.isolateOthers || !DetectedPorts.IsLocal(activeRig))
      {
        PhotonView view = DetectedPorts.GetView(activeRig);
        if (!((Object) view == (Object) null))
        {
          int[] array = ((IEnumerable<Player>) PhotonNetwork.PlayerList).Where<Player>((Func<Player, bool>) (p => p.ActorNumber != view.Owner.ActorNumber)).Select<Player, int>((Func<Player, int>) (p => p.ActorNumber)).ToArray<int>();
          this.RaiseDestroy(activeRig, view.ViewID, array);
        }
      }
    }
  }
  public void IsolateAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig auraRig in DetectedPorts.AuraRigs())
    {
      if (!this.isolateOthers || !DetectedPorts.IsLocal(auraRig))
      {
        PhotonView view = DetectedPorts.GetView(auraRig);
        if (!((Object) view == (Object) null))
          this.RaiseDestroy(auraRig, view.ViewID, new int[1]
          {
            view.Owner.ActorNumber
          });
      }
    }
  }
  public void IsolateOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (!DetectedPorts.IsLocal(activeRig) && DetectedPorts.IsTouched(activeRig))
        this.IsolateRig(activeRig);
    }
  }
  public void ChangeNamePlayer(Player p, string n)
  {
    try
    {
      ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable()
      {
        [byte.MaxValue] = (object) n
      };
      p.SetCustomProperties(hashtable, (ExitGames.Client.Photon.Hashtable) null, (WebFlags) null);
    }
    catch
    {
    }
  }
  public void ChangeNameAll()
  {
    foreach (Player playerListOther in PhotonNetwork.PlayerListOthers)
      this.ChangeNamePlayer(playerListOther, this.customName);
  }
  public void ChangeNameAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig auraRig in DetectedPorts.AuraRigs())
      this.ChangeNamePlayer(DetectedPorts.GetPhotonPlayer(auraRig), this.customName);
  }
  public void ChangeNameOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (DetectedPorts.IsTouched(activeRig))
        this.ChangeNamePlayer(DetectedPorts.GetPhotonPlayer(activeRig), this.customName);
    }
  }
  public void BanPlayer(Player p)
  {
    try
    {
      ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable()
      {
        [byte.MaxValue] = (object) "BADGORILLA"
      };
      p.SetCustomProperties(hashtable, (ExitGames.Client.Photon.Hashtable) null, (WebFlags) null);
      try
      {
        MonkeAgent.instance.SendReport("evading the name ban", p.UserId, p.NickName);
      }
      catch
      {
      }
    }
    catch
    {
    }
  }
  public void BanAll()
  {
    foreach (Player playerListOther in PhotonNetwork.PlayerListOthers)
      this.BanPlayer(playerListOther);
  }
  public void SetMasterSelf()
  {
    try
    {
      if (!PhotonNetwork.InRoom)
        return;
      PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
      Debug.Log((object) "Set Master requested.");
    }
    catch (Exception ex)
    {
      Debug.Log((object) ("Set Master failed: " + ex.Message));
    }
  }
  public void BypassPlayer(Player p)
  {
    try
    {
      if (p == null || p.CustomProperties == null || ((Dictionary<object, object>) p.CustomProperties).Count == 0)
        return;
      ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
      foreach (object obj in ((Dictionary<object, object>) p.CustomProperties).Keys.ToList<object>())
      {
        string str = obj?.ToString();
        if (str != null && !str.Equals("didTutorial"))
          hashtable[(object) str] = (object) null;
      }
      if (((Dictionary<object, object>) hashtable).Count <= 0)
        return;
      p.SetCustomProperties(hashtable, (ExitGames.Client.Photon.Hashtable) null, (WebFlags) null);
    }
    catch
    {
    }
  }
  public void BypassAll()
  {
    foreach (Player player in PhotonNetwork.PlayerList)
      this.BypassPlayer(player);
  }
  public void BypassAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig auraRig in DetectedPorts.AuraRigs())
      this.BypassPlayer(DetectedPorts.GetPhotonPlayer(auraRig));
  }
  public void BypassOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (DetectedPorts.IsTouched(activeRig))
        this.BypassPlayer(DetectedPorts.GetPhotonPlayer(activeRig));
    }
  }
  public void BreakPlayer(Player p)
  {
    try
    {
      ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
      foreach (string fakeModKey in this.fakeModKeys)
        hashtable[(object) fakeModKey] = (object) true;
      p.SetCustomProperties(hashtable, (ExitGames.Client.Photon.Hashtable) null, (WebFlags) null);
    }
    catch
    {
    }
  }
  public void BreakAll()
  {
    foreach (Player player in PhotonNetwork.PlayerList)
      this.BreakPlayer(player);
  }
  public void BreakAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig auraRig in DetectedPorts.AuraRigs())
      this.BreakPlayer(DetectedPorts.GetPhotonPlayer(auraRig));
  }
  public void BreakOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (DetectedPorts.IsTouched(activeRig))
        this.BreakPlayer(DetectedPorts.GetPhotonPlayer(activeRig));
    }
  }
  public void SetTutorial(Player p, bool val)
  {
    try
    {
      ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
      ((Dictionary<object, object>) hashtable).Add((object) "didTutorial", (object) val);
      p.SetCustomProperties(hashtable, (ExitGames.Client.Photon.Hashtable) null, (WebFlags) null);
    }
    catch
    {
    }
  }
  public void IncludeAll()
  {
    foreach (Player player in PhotonNetwork.PlayerList)
      this.SetTutorial(player, true);
  }
  public void IncludeAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig auraRig in DetectedPorts.AuraRigs())
      this.SetTutorial(DetectedPorts.GetPhotonPlayer(auraRig), true);
  }
  public void IncludeOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (DetectedPorts.IsTouched(activeRig))
        this.SetTutorial(DetectedPorts.GetPhotonPlayer(activeRig), true);
    }
  }
  public void ExcludeAll()
  {
    foreach (Player player in PhotonNetwork.PlayerList)
      this.SetTutorial(player, false);
  }
  public void ExcludeAura()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig auraRig in DetectedPorts.AuraRigs())
      this.SetTutorial(DetectedPorts.GetPhotonPlayer(auraRig), false);
  }
  public void ExcludeOnTouch()
  {
    if (!NetworkSystem.Instance.InRoom)
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      if (DetectedPorts.IsTouched(activeRig))
        this.SetTutorial(DetectedPorts.GetPhotonPlayer(activeRig), false);
    }
  }
  public void BreakGamemodeOn(bool breaking)
  {
    try
    {
      ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
      ((Dictionary<object, object>) hashtable).Add((object) "didTutorial", (object) !breaking);
      foreach (Player player in PhotonNetwork.PlayerList)
        player.SetCustomProperties(hashtable, (ExitGames.Client.Photon.Hashtable) null, (WebFlags) null);
    }
    catch
    {
    }
  }
  public void BreakNetworkTriggers()
  {
    try
    {
      string[] strArray = DetectedPorts.SplitMode(DetectedPorts.CurrentGameModeString());
      strArray[2] = "BREAK_" + UnityEngine.Random.Range(0, 99999).ToString();
      PhotonNetwork.CurrentRoom.SetCustomProperties(DetectedPorts.MakeModeProps(string.Join(";", strArray)), (ExitGames.Client.Photon.Hashtable) null, (WebFlags) null);
    }
    catch
    {
    }
  }
  public void KickNetworkTriggers()
  {
    try
    {
      ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
      ((Dictionary<object, object>) hashtable).Add((object) "gameMode", (object) string.Empty);
      PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable, (ExitGames.Client.Photon.Hashtable) null, (WebFlags) null);
    }
    catch
    {
    }
  }
  private static string CurrentGameModeString()
  {
    try
    {
      object obj = (object) null;
      if (PhotonNetwork.CurrentRoom != null)
      {
        if (((RoomInfo) PhotonNetwork.CurrentRoom).CustomProperties != null)
        {
          if (((Dictionary<object, object>) ((RoomInfo) PhotonNetwork.CurrentRoom).CustomProperties).TryGetValue((object) "gameMode", out obj))
          {
            if (obj is string str)
            {
              if (!string.IsNullOrEmpty(str))
                return str;
            }
          }
        }
      }
    }
    catch
    {
    }
    try
    {
      return NetworkSystem.Instance.GameModeString;
    }
    catch
    {
    }
    return ";;;;";
  }
  private static string[] SplitMode(string s)
  {
    string[] strArray1 = (s ?? "").Split(';');
    if (strArray1.Length < 5)
    {
      string[] strArray2 = new string[5];
      for (int index = 0; index < 5; ++index)
        strArray2[index] = index < strArray1.Length ? strArray1[index] : "";
      strArray1 = strArray2;
    }
    return strArray1;
  }
  private static ExitGames.Client.Photon.Hashtable MakeModeProps(string mode)
  {
    ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
    ((Dictionary<object, object>) hashtable).Add((object) "gameMode", (object) mode);
    return hashtable;
  }
  public void ChangeGamemode(string gameType)
  {
    try
    {
      if (!NetworkSystem.Instance.InRoom)
        return;
      if (!PhotonNetwork.IsMasterClient)
      {
        Debug.Log((object) "Change Gamemode: master only.");
      }
      else
      {
        GameModeType gameModeType;
        try
        {
          gameModeType = (GameModeType) Enum.Parse(typeof (GameModeType), gameType, true);
        }
        catch
        {
          Debug.Log((object) ("Change Gamemode: unknown mode " + gameType));
          return;
        }
        Type type = typeof (PhotonNetwork).Assembly.GetType("GameModeSerializer");
        if (type == (Type) null)
          type = typeof (NetworkSystem).Assembly.GetType("GameModeSerializer");
        if (type == (Type) null)
        {
          Debug.Log((object) "Change Gamemode: no serializer type.");
        }
        else
        {
          Object objectOfType = Object.FindObjectOfType(type);
          if ((objectOfType == (Object) null))
          {
            Debug.Log((object) "Change Gamemode: no serializer instance.");
          }
          else
          {
            PropertyInfo property = type.GetProperty("gameModeKeyInt", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (property == (PropertyInfo) null)
            {
              Debug.Log((object) "Change Gamemode: no key prop.");
            }
            else
            {
              property.SetValue((object) objectOfType, (object) (int) gameModeType);
              Debug.Log((object) $"Change Gamemode: set to {gameModeType.ToString()}.");
            }
          }
        }
      }
    }
    catch (Exception ex)
    {
      Debug.Log((object) ("Change Gamemode failed: " + ex.Message));
    }
  }
  public void SpazGamemode()
  {
    if ((double) Time.time <= (double) this.spazGamemodeDelay)
      return;
    string[] strArray = new string[10]
    {
      "Casual",
      "Infection",
      "HuntDown",
      "Paintbrawl",
      "Ambush",
      "FreezeTag",
      "Ghost",
      "Custom",
      "Guardian",
      "PropHunt"
    };
    this.ChangeGamemode(strArray[UnityEngine.Random.Range(0, strArray.Length)]);
    this.spazGamemodeDelay = Time.time + 0.1f;
  }
  public static bool IsMaster()
  {
    try
    {
      return NetworkSystem.Instance.LocalPlayer.IsMasterClient;
    }
    catch
    {
      return false;
    }
  }
  public static bool RequireMaster() => DetectedPorts.IsMaster();
  private static VRRig OwnRig()
  {
    try
    {
      return GorillaTagger.Instance.offlineVRRig;
    }
    catch
    {
      return (VRRig) null;
    }
  }
  private static string CurrentMap()
  {
    try
    {
      VRRig offlineVrRig = GorillaTagger.Instance.offlineVRRig;
      if (((Object) offlineVrRig != (Object) null))
      {
        if (((Object) offlineVrRig.zoneEntity != (Object) null))
        {
          GTZone currentZone = offlineVrRig.zoneEntity.currentZone;
          return EnumUtilExt.GetName<GTZone>(offlineVrRig.zoneEntity.currentZone);
        }
      }
    }
    catch
    {
    }
    return "";
  }
  private static HalloweenGhostChaser Ghost()
  {
    try
    {
      GameObject gameObject = GameObject.Find("Environment Objects/05Maze_PersistentObjects/Ghosts/Halloween Ghost/FloatingChaseSkeleton");
      return ((Object) gameObject == (Object) null) ? (HalloweenGhostChaser) null : gameObject.GetComponent<HalloweenGhostChaser>();
    }
    catch
    {
      return (HalloweenGhostChaser) null;
    }
  }
  public async void Lucy(object o)
  {
    if (!DetectedPorts.RequireMaster())
    {
    }
    else
    {
      HalloweenGhostChaser h = DetectedPorts.Ghost();
      if (((Object) h == (Object) null))
      {
        Debug.Log((object) "Lucy: maze ghost not in this map.");
      }
      else
      {
        switch (o)
        {
          case bool[] b:
            if (b.Length != 0 && b[0])
            {
              h.timeGongStarted = Time.time;
              h.currentState = (HalloweenGhostChaser.ChaseState) 4;
              h.isSummoned = b.Length > 1 && b[1];
              if (b.Length != 2)
              {
                b = (bool[]) null;
                break;
              }
              await Task.Delay(300);
              h.timeGongStarted = Time.time;
              h.currentState = (HalloweenGhostChaser.ChaseState) 4;
              h.isSummoned = b[1];
              b = (bool[]) null;
              break;
            }
            h.currentState = (HalloweenGhostChaser.ChaseState) 1;
            h.isSummoned = false;
            b = (bool[]) null;
            break;
          case float num:
            if (h.currentState == (HalloweenGhostChaser.ChaseState) 1)
            {
              Debug.Log((object) "Lucy isnt spawned!");
              break;
            }
            if (h.currentState != (HalloweenGhostChaser.ChaseState) 8)
            {
              Debug.Log((object) "Lucy isnt chasing!");
              break;
            }
            h.currentSpeed = num;
            h.lastSpeedIncreased = Time.time + 99999f;
            break;
          default:
            NetPlayer t = o as NetPlayer;
            if (t != null)
            {
              if (h.currentState == (HalloweenGhostChaser.ChaseState) 1)
              {
                Debug.Log((object) "Lucy isnt spawned!");
                break;
              }
              if (h.currentState != (HalloweenGhostChaser.ChaseState) 8)
              {
                Debug.Log((object) "Lucy isnt chasing!");
                break;
              }
              VRRig vrRig = (VRRig) null;
              try
              {
                VRRig[] array = DetectedPorts.ActiveRigs();
                if (array != null)
                  vrRig = Array.Find<VRRig>(array, (Predicate<VRRig>) (r =>
                  {
                    try
                    {
                      return DetectedPorts.GetNetPlayer(r) == t;
                    }
                    catch
                    {
                      return false;
                    }
                  }));
              }
              catch
              {
              }
              if (((Object) vrRig != (Object) null))
                h.followTarget = ((Component) vrRig).transform;
              h.targetPlayer = t;
              break;
            }
            VRRig r1 = o as VRRig;
            if (r1 != null)
            {
              Task.Delay((int) (((double) h.grabDuration + 0.098499998450279236) * 1000.0)).ContinueWith((Action<Task>) (_ =>
              {
                try
                {
                  h.currentState = (HalloweenGhostChaser.ChaseState) 16 ;
                  h.grabTime = Time.time;
                  h.targetPlayer = DetectedPorts.GetNetPlayer(r1);
                }
                catch
                {
                }
              }));
              break;
            }
            if (!(o is Vector3))
            {
              break;
            }
            Vector3 vector3 = (Vector3) o;
            h.currentState = (HalloweenGhostChaser.ChaseState) 8;
            VRRig rig = DetectedPorts.OwnRig();
            if (((Object) rig != (Object) null))
            {
              h.followTarget = ((Component) rig).transform;
              try
              {
                h.targetPlayer = DetectedPorts.GetNetPlayer(rig);
              }
              catch
              {
              }
            }
            try
            {
              ((Component) h).transform.position = vector3;
              break;
            }
            catch
            {
              break;
            }
        }
      }
    }
  }
  public void FloatTouch()
  {
    if (!DetectedPorts.IsMaster())
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      try
      {
        if (DetectedPorts.IsTouched(activeRig))
          this.Lucy((object) activeRig);
      }
      catch
      {
      }
    }
  }
  public void FloatOnTouch()
  {
    if (!DetectedPorts.IsMaster())
      return;
    VRRig vrRig = DetectedPorts.OwnRig();
    if (((Object) vrRig == (Object) null))
      return;
    foreach (VRRig activeRig in DetectedPorts.ActiveRigs())
    {
      try
      {
        if ((double) Vector3.Distance(((Component) activeRig).transform.position, ((Component) vrRig).transform.position) < 1.0)
          this.Lucy((object) activeRig);
      }
      catch
      {
      }
    }
  }
  public void GrabLucy()
  {
    if (!DetectedPorts.RequireMaster())
      return;
    HalloweenGhostChaser halloweenGhostChaser = DetectedPorts.Ghost();
    if (((Object) halloweenGhostChaser == (Object) null))
      return;
    Vector3 vector3;
    vector3 = new Vector3(0.0f, 0.8f, 0.0f);
    try
    {
      if (ControllerInputPoller.instance.rightGrab)
        ((Component) halloweenGhostChaser).transform.position = (GorillaTagger.Instance.rightHandTransform.position + vector3);
      if (!ControllerInputPoller.instance.leftGrab)
        return;
      ((Component) halloweenGhostChaser).transform.position = (GorillaTagger.Instance.leftHandTransform.position + vector3);
    }
    catch
    {
    }
  }
  public void OrbitTick()
  {
    if (!this.orbitLucy)
      return;
    if (!DetectedPorts.RequireMaster())
    {
      this.orbitLucy = false;
    }
    else
    {
      VRRig vrRig = DetectedPorts.OwnRig();
      if (((Object) vrRig == (Object) null))
        return;
      this.orbitAngle += 90f * Time.deltaTime;
      float num = this.orbitAngle * ((float) Math.PI / 180f);
      this.Lucy((object) (((Component) vrRig).transform.position + new Vector3(Mathf.Cos(num) * 1.7f, 2.5f, Mathf.Sin(num) * 1.7f)));
    }
  }
  public void TargetSelf()
  {
    VRRig rig = DetectedPorts.OwnRig();
    if (((Object) rig == (Object) null))
      return;
    NetPlayer o = (NetPlayer) null;
    try
    {
      o = DetectedPorts.GetNetPlayer(rig);
    }
    catch
    {
    }
    if (o == null)
      return;
    this.Lucy((object) o);
  }
  public void TargetSpaz()
  {
    if (!DetectedPorts.RequireMaster())
      return;
    try
    {
      NetPlayer[] playerListOthers = NetworkSystem.Instance.PlayerListOthers;
      if (playerListOthers.Length == 0)
        return;
      this.Lucy((object) playerListOthers[UnityEngine.Random.Range(0, playerListOthers.Length)]);
    }
    catch
    {
    }
  }
  private void ChangeTeam(VRRig rig, int lives)
  {
    if (!DetectedPorts.RequireMaster())
      return;
    try
    {
      if (!(GorillaGameManager.instance is GorillaPaintbrawlManager instance))
      {
        Debug.Log((object) "GodMode: must be in Paintbrawl.");
      }
      else
      {
        NetPlayer netPlayer = (NetPlayer) null;
        try
        {
          netPlayer = DetectedPorts.GetNetPlayer(rig);
        }
        catch
        {
        }
        if (netPlayer == null)
        {
          if (((Object) rig != (Object) null))
          {
            try
            {
              netPlayer = rig.Creator;
            }
            catch
            {
            }
          }
        }
        if (netPlayer == null)
          return;
        instance.playerLives[netPlayer.ActorNumber] = lives;
      }
    }
    catch
    {
    }
  }
  public void BreakMapLighting()
  {
    if (!DetectedPorts.RequireMaster())
      return;
    try
    {
      string[] strArray = new string[10]
      {
        "forest",
        "canyon",
        "beach",
        "mountain",
        "skyJungle",
        "critters",
        "Metropolis",
        "mall",
        "VIMExperience1",
        "bayou"
      };
      string str1 = DetectedPorts.CurrentMap();
      bool flag = false;
      foreach (string str2 in strArray)
      {
        if (str2 == str1)
        {
          flag = true;
          break;
        }
      }
      if (!flag)
      {
        Debug.Log((object) "BreakMapLighting: doesnt work in this map.");
      }
      else
      {
        PhotonView photonView = BetterDayNightManager.instance.photonView;
        if (((Object) photonView == (Object) null))
        {
          Debug.Log((object) "BreakMapLighting: no lighting view found.");
        }
        else
        {
          try
          {
            Player localPlayer = PhotonNetwork.LocalPlayer;
            if (photonView.Owner != localPlayer)
            {
              photonView.OwnerActorNr = localPlayer.ActorNumber;
              photonView.ControllerActorNr = localPlayer.ActorNumber;
            }
          }
          catch
          {
          }
          PhotonNetwork.Destroy(photonView);
          Debug.Log((object) "BreakMapLighting: map lighting broken.");
        }
      }
    }
    catch
    {
      Debug.Log((object) "BreakMapLighting failed.");
    }
  }
}
