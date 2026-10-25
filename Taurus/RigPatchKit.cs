
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public static class RigPatchKit
{
  private static Harmony harmony;
  public static bool LucyFreeze;
  public static Vector3? SpoofPosition;
  public static void Apply()
  {
    if (RigPatchKit.harmony != null)
      return;
    try
    {
      RigPatchKit.harmony = new Harmony("local.gtag.menutemplate.rigkit");
      foreach (Type nestedType in typeof (RigPatchKit).GetNestedTypes(BindingFlags.NonPublic))
      {
        try
        {
          RigPatchKit.harmony.CreateClassProcessor(nestedType).Patch();
        }
        catch (Exception ex)
        {
          Debug.LogWarning((object) $"RigPatchKit skipped {nestedType.Name}: {ex.Message}");
        }
      }
    }
    catch (Exception ex)
    {
      Debug.LogWarning((object) ("RigPatchKit: " + ex.Message));
    }
  }
  [HarmonyPatch(typeof (VRRig), "OnDisable")]
  private static class VRRigOnDisable
  {
    private static bool Prefix(VRRig __instance)
    {
      try
      {
        return !__instance.isLocal;
      }
      catch
      {
        return true;
      }
    }
  }
  [HarmonyPatch(typeof (VRRig), "PostTick")]
  private static class VRRigPostTickGate
  {
    private static bool Prefix(VRRig __instance)
    {
      try
      {
        if (__instance.isLocal)
          return ((Behaviour) __instance).enabled;
      }
      catch
      {
      }
      return true;
    }
  }
  [HarmonyPatch(typeof (GorillaTagManager), "ReportTag")]
  private static class TagReportGate
  {
    public static readonly List<NetPlayer> blacklistedPlayers = new List<NetPlayer>();
    public static readonly List<NetPlayer> invinciblePlayers = new List<NetPlayer>();
    private static bool Prefix(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
    {
      try
      {
        if (!RigPatchKit.TagReportGate.blacklistedPlayers.Contains(taggingPlayer))
          return !RigPatchKit.TagReportGate.invinciblePlayers.Contains(taggedPlayer);
      }
      catch
      {
      }
      return false;
    }
  }
  [HarmonyPatch(typeof (HalloweenGhostChaser), "LateUpdate")]
  private static class LucyFreezePatch
  {
    private static bool Prefix() => !RigPatchKit.LucyFreeze;
  }
  [HarmonyPatch(typeof (PhotonNetwork), "OnSerializeWrite")]
  private static class SerializeSpoofPatch
  {
    private static void Prefix(PhotonView view, out Vector3? __state)
    {
      __state = new Vector3?();
      try
      {
        if (!RigPatchKit.SpoofPosition.HasValue || !((Object) VRRig.LocalRig != (Object) null) || !((Object) GorillaTagger.Instance != (Object) null) || !((Object) GorillaTagger.Instance.myVRRig != (Object) null) || !((Object) view == (Object) GorillaTagger.Instance.myVRRig.GetView))
          return;
        __state = new Vector3?(((Component) VRRig.LocalRig).transform.position);
        ((Component) VRRig.LocalRig).transform.position = RigPatchKit.SpoofPosition.Value;
      }
      catch
      {
      }
    }
    private static void Finalizer(ref Vector3? __state)
    {
      try
      {
        if (!__state.HasValue || !((Object) VRRig.LocalRig != (Object) null))
          return;
        ((Component) VRRig.LocalRig).transform.position = __state.Value;
      }
      catch
      {
      }
    }
  }
}
