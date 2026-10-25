
using GorillaNetworking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class CosmeticPreview : IDisposable
{
  private CosmeticsController controller;
  private string allowed;
  private CosmeticsController.CosmeticItem[] worn;
  private readonly Dictionary<FieldInfo, List<CosmeticsController.CosmeticItem>> lists = new Dictionary<FieldInfo, List<CosmeticsController.CosmeticItem>>();
  private readonly Harmony harmony = new Harmony("local.itzgame.cosmeticpreview");
  private Exception refreshError;
  private static bool active;
  public void Enable()
  {
    if (((Object) this.controller != (Object) null))
      return;
    CosmeticsController instance = CosmeticsController.instance;
    if (((Object) instance == (Object) null) || !instance.v2_allCosmeticsInfoAssetRef_isLoaded || !instance.v2_isCosmeticPlayFabCatalogDataLoaded || instance.allCosmetics == null || instance.allCosmetics.Count == 0)
      return;
    this.controller = instance;
    try
    {
      this.CaptureOwned();
      this.harmony.Patch((MethodBase) AccessTools.Method(typeof (VRRig), "IsItemAllowed", (Type[]) null, (Type[]) null), (HarmonyMethod) null, new HarmonyMethod(typeof (CosmeticPreview), "AllowLocal", (Type[]) null), (HarmonyMethod) null, (HarmonyMethod) null, (HarmonyMethod) null);
      CosmeticPreview.active = true;
      instance.V2_OnGetCosmeticsPlayFabCatalogData_PostSuccess += new Action(this.OnCatalogReload);
      this.ApplyPreview();
    }
    catch
    {
      this.Dispose();
      throw;
    }
  }
  public void Tick()
  {
    if (this.refreshError != null)
    {
      Exception refreshError = this.refreshError;
      this.refreshError = (Exception) null;
      throw new InvalidOperationException("Cosmetic preview refresh failed.", refreshError);
    }
    if (((Object) this.controller == (Object) CosmeticsController.instance) && ((Object) this.controller != (Object) null))
      return;
    if (((Object) this.controller != (Object) null))
      this.Dispose();
    this.Enable();
  }
  private void CaptureOwned()
  {
    this.allowed = this.controller.concatStringCosmeticsAllowed;
    this.worn = (CosmeticsController.CosmeticItem[]) this.controller.currentWornSet.items.Clone();
    this.lists.Clear();
    foreach (FieldInfo field in typeof (CosmeticsController).GetFields(BindingFlags.Instance | BindingFlags.Public))
    {
      if (field.Name.StartsWith("unlocked", StringComparison.Ordinal) && field.GetValue((object) this.controller) is List<CosmeticsController.CosmeticItem> collection)
        this.lists.Add(field, new List<CosmeticsController.CosmeticItem>((IEnumerable<CosmeticsController.CosmeticItem>) collection));
    }
  }
  private void ApplyPreview()
  {
    HashSet<string> stringSet = new HashSet<string>();
    foreach (CosmeticsController.CosmeticItem unlockedCosmetic in this.controller.unlockedCosmetics)
      stringSet.Add(unlockedCosmetic.itemName);
    foreach (CosmeticsController.CosmeticItem allCosmetic in this.controller.allCosmetics)
    {
      if (!allCosmetic.isNullItem && !string.IsNullOrEmpty(allCosmetic.itemName) && allCosmetic.itemCategory > (CosmeticsController.CosmeticCategory) 0 && allCosmetic.itemCategory < (CosmeticsController.CosmeticCategory) 12 && stringSet.Add(allCosmetic.itemName))
        this.controller.UnlockItem(allCosmetic.itemName, false);
    }
    this.controller.UpdateWardrobeModelsAndButtons();
    Action cosmeticsUpdated = this.controller.OnCosmeticsUpdated;
    if (cosmeticsUpdated != null)
      cosmeticsUpdated();
    this.controller.UpdateWornCosmetics(false, false);
  }
  private void OnCatalogReload()
  {
    try
    {
      this.CaptureOwned();
      this.ApplyPreview();
    }
    catch (Exception ex)
    {
      this.refreshError = ex;
    }
  }
  public void Dispose()
  {
    CosmeticPreview.active = false;
    this.harmony.UnpatchSelf();
    try
    {
      if (((Object) this.controller == (Object) null))
        return;
      this.controller.V2_OnGetCosmeticsPlayFabCatalogData_PostSuccess -= new Action(this.OnCatalogReload);
      foreach (KeyValuePair<FieldInfo, List<CosmeticsController.CosmeticItem>> list in this.lists)
      {
        if (list.Key.GetValue((object) this.controller) is List<CosmeticsController.CosmeticItem> cosmeticItemList)
        {
          cosmeticItemList.Clear();
          cosmeticItemList.AddRange((IEnumerable<CosmeticsController.CosmeticItem>) list.Value);
        }
      }
      this.controller.concatStringCosmeticsAllowed = this.allowed;
      if (this.worn != null)
        Array.Copy((Array) this.worn, (Array) this.controller.currentWornSet.items, Math.Min(this.worn.Length, this.controller.currentWornSet.items.Length));
      this.controller.UpdateWardrobeModelsAndButtons();
      Action cosmeticsUpdated = this.controller.OnCosmeticsUpdated;
      if (cosmeticsUpdated != null)
        cosmeticsUpdated();
      this.controller.UpdateWornCosmetics(false, false);
    }
    finally
    {
      this.controller = (CosmeticsController) null;
      this.worn = (CosmeticsController.CosmeticItem[]) null;
      this.lists.Clear();
      this.refreshError = (Exception) null;
    }
  }
  private static void AllowLocal(VRRig __instance, ref bool __result)
  {
    if (!CosmeticPreview.active || !__instance.isOfflineVRRig)
      return;
    __result = true;
  }
}
