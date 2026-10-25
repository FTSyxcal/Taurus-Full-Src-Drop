
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class MenuDesign : IDisposable
{
  public static readonly string[] Names = MenuPacket.Designs;
  private readonly Transform root;
  private readonly Dictionary<Transform, Vector3> positions = new Dictionary<Transform, Vector3>();
  private readonly Dictionary<MeshFilter, Mesh> originals = new Dictionary<MeshFilter, Mesh>();
  private readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
  private readonly Dictionary<int, ImportedAssetMenu> assets = new Dictionary<int, ImportedAssetMenu>();
  private readonly Dictionary<int, ImportedNativeMenu> native = new Dictionary<int, ImportedNativeMenu>();
  private readonly MaterialPropertyBlock colors = new MaterialPropertyBlock();
  private int active = -1;
  private float switchedAt = float.NegativeInfinity;
  private float switchDirection = 1f;
  private bool rounded;
  private ImportedXyfer imported;
  private KyraMenu kyra;
  private TelAvivMenu telAviv;
  private FizzoMenu fizzo;
  private ParrotMenu parrot;
  public MenuDesign(Transform root)
  {
    this.root = root;
    if (((Object) root.Find("DesignSettings") == (Object) null))
    {
      GameObject gameObject = Object.Instantiate<GameObject>(((Component) root.Find("ButtonHome")).gameObject, root, false);
      ((Object) gameObject).name = "DesignSettings";
      gameObject.SetActive(false);
    }
    foreach (Transform componentsInChild in ((Component) root).GetComponentsInChildren<Transform>(true))
      this.positions[componentsInChild] = componentsInChild.localPosition;
    foreach (MeshFilter componentsInChild in ((Component) root).GetComponentsInChildren<MeshFilter>(true))
    {
      if (((Object) componentsInChild).name != "Icon" && ((Object) componentsInChild).name != "FavoriteIcon" && ((Object) componentsInChild).name != "BrowserSurface" && !((Component) componentsInChild).transform.IsChildOf(root.Find("ColorPicker")))
        this.originals[componentsInChild] = componentsInChild.sharedMesh;
    }
  }
  public bool Apply(MenuState state)
  {
    int design = state.Category == 11 ? 0 : state.Design;
    if (design == this.active && this.rounded == state.Rounded)
      return false;
    if (design != this.active && this.active >= 0)
    {
      int length = MenuPacket.DesignOrder.Length;
      this.switchDirection = (Array.IndexOf<int>(MenuPacket.DesignOrder, design) - Array.IndexOf<int>(MenuPacket.DesignOrder, this.active) + length) % length <= length / 2 ? 1f : -1f;
      this.switchedAt = Time.unscaledTime;
    }
    this.imported?.SetActive(false);
    this.kyra?.SetActive(false);
    this.telAviv?.SetActive(false);
    this.fizzo?.SetActive(false);
    this.parrot?.SetActive(false);
    foreach (ImportedAssetMenu importedAssetMenu in this.assets.Values)
      importedAssetMenu.SetActive(false);
    foreach (ImportedNativeMenu importedNativeMenu in this.native.Values)
      importedNativeMenu.SetActive(false);
    this.active = design;
    this.rounded = state.Rounded;
    foreach (KeyValuePair<Transform, Vector3> position in this.positions)
    {
      if (((Object) position.Key != (Object) null))
      {
        position.Key.localPosition = position.Value;
        position.Key.localScale = Vector3.one;
      }
    }
    foreach (KeyValuePair<MeshFilter, Mesh> original in this.originals)
    {
      if (!((Object) original.Key == (Object) null))
      {
        MeshFilter key1 = original.Key;
        Bounds bounds;
        Mesh mesh;
        if (!this.rounded)
        {
          string key2 = "square/" + ((Object) original.Key).name;
          bounds = original.Value.bounds;
          Vector3 size = (bounds).size;
          mesh = this.Shape(key2, size, 0.0f);
        }
        else
          mesh = original.Value;
        key1.sharedMesh = mesh;
        BoxCollider component = ((Component) original.Key).GetComponent<BoxCollider>();
        if (((Object) component != (Object) null))
        {
          component.center = Vector3.zero;
          BoxCollider boxCollider = component;
          bounds = original.Value.bounds;
          Vector3 size = (bounds).size;
          boxCollider.size = size;
        }
      }
    }
    ((Component) this.root.Find("DesignSettings")).gameObject.SetActive(false);
    switch (design)
    {
      case 1:
        if (this.imported == null)
          this.imported = new ImportedXyfer(this.root);
        this.imported.SetActive(true);
        break;
      case 10:
        if (this.kyra == null)
          this.kyra = new KyraMenu(this.root, new Func<string, Vector3, float, int, Mesh>(this.Shape));
        this.kyra.SetActive(true);
        break;
      case 11:
        if (this.telAviv == null)
          this.telAviv = new TelAvivMenu(this.root);
        this.telAviv.SetActive(true);
        break;
      case 12:
        if (this.fizzo == null)
          this.fizzo = new FizzoMenu(this.root, new Func<string, Vector3, float, Mesh>(this.Shape));
        this.fizzo.SetActive(true);
        break;
      case 13:
        if (this.parrot == null)
          this.parrot = new ParrotMenu(this.root);
        this.parrot.SetActive(true);
        break;
      default:
        if (design == 5 || design >= 6)
        {
          ImportedNativeMenu importedNativeMenu;
          if (!this.native.TryGetValue(design, out importedNativeMenu))
            this.native[design] = importedNativeMenu = new ImportedNativeMenu(this.root, design);
          importedNativeMenu.SetActive(true);
          break;
        }
        if (design > 1)
        {
          ImportedAssetMenu importedAssetMenu;
          if (!this.assets.TryGetValue(design, out importedAssetMenu))
            this.assets[design] = importedAssetMenu = new ImportedAssetMenu(this.root, design);
          importedAssetMenu.SetActive(true);
          break;
        }
        break;
    }
    return true;
  }
  public void FormatRows(MenuState state)
  {
  }
  public Vector3 SlideOffset(float time)
  {
    float num = 1f - Mathf.Clamp01((float) (((double) time - (double) this.switchedAt) / 0.20000000298023224));
    return (Vector3.down * this.switchDirection * 0.012f * num * num * num);
  }
  public void Animate(MenuState state, float time)
  {
    if (this.active == 10)
      this.kyra.Tick(time);
    else if (this.active == 11)
      this.telAviv.Tick(time);
    else if (this.active == 12)
      this.fizzo.Tick(time);
    else if (this.active == 13)
      this.parrot.Tick(time);
    else if (this.active == 1)
      this.imported.Tick(time);
    else if (this.active == 5 || this.active >= 6)
      this.native[this.active].Tick();
    else if (this.active > 1)
    {
      this.assets[this.active].Tick();
    }
    else
    {
      Renderer component = ((Component) this.root.Find("Title")).GetComponent<Renderer>();
      component.GetPropertyBlock(this.colors);
      MaterialPropertyBlock colors = this.colors;
      Bounds localBounds = component.localBounds;
      double x1 = (double) (localBounds).min.x;
      localBounds = component.localBounds;
      double x2 = (double) (localBounds).max.x;
      Vector4 vector4 = new Vector4((float) x1, (float) x2, 0.0f, 0.0f);
      colors.SetVector("_TextBounds", vector4);
      component.SetPropertyBlock(this.colors);
      float num = this.rounded ? 0.018f : 0.0f;
      this.SetOutline("Frame", new Vector4(0.37f, 0.55f, num, 0.0f));
      this.SetOutline("DisconnectFrame", new Vector4(0.37f, 0.045f, num, 0.0f));
    }
  }
  public void RefreshImported(MenuState state)
  {
    if (this.active == 10)
      this.kyra.Refresh(state);
    else if (this.active == 11)
      this.telAviv.Refresh(state);
    else if (this.active == 12)
      this.fizzo.Refresh(state);
    else if (this.active == 13)
      this.parrot.Refresh(state);
    else if (this.active == 1)
      this.imported.Refresh(state);
    else if (this.active == 5 || this.active >= 6)
    {
      this.native[this.active].Refresh(state);
    }
    else
    {
      if (this.active <= 1)
        return;
      this.assets[this.active].Refresh(state);
    }
  }
  private void SetOutline(string name, Vector4 size)
  {
    Renderer component = ((Component) this.root.Find(name)).GetComponent<Renderer>();
    component.GetPropertyBlock(this.colors);
    this.colors.SetVector("_Size", size);
    component.SetPropertyBlock(this.colors);
  }
  private Mesh Shape(string key, Vector3 size, float radius) => this.Shape(key, size, radius, 15);
  private Mesh Shape(string key, Vector3 size, float radius, int cornerMask)
  {
    Mesh mesh1;
    if (this.meshes.TryGetValue(key, out mesh1))
      return mesh1;
    Mesh mesh2;
    if ((double) radius <= 0.0)
    {
      Vector3[] vector3Array = new Vector3[8];
      for (int index = 0; index < 8; ++index)
        vector3Array[index] = Vector3.Scale((size / 2f), new Vector3((index & 1) == 0 ? -1f : 1f, (index & 2) == 0 ? -1f : 1f, (index & 4) == 0 ? -1f : 1f));
      mesh2 = new Mesh()
      {
        vertices = vector3Array,
        triangles = new int[36]
        {
          0,
          2,
          3,
          0,
          3,
          1,
          4,
          5,
          7,
          4,
          7,
          6,
          0,
          1,
          5,
          0,
          5,
          4,
          2,
          6,
          7,
          2,
          7,
          3,
          0,
          4,
          6,
          0,
          6,
          2,
          1,
          3,
          7,
          1,
          7,
          5
        }
      };
    }
    else
    {
      radius = Mathf.Min(radius, Mathf.Min(size.y, size.z) / 2f);
      Vector3[] vector3Array = new Vector3[138];
      List<int> intList = new List<int>();
      for (int index = 0; index < 68; ++index)
      {
        int num1 = index / 17;
        float num2 = (cornerMask & 1 << num1) != 0 ? radius : 0.0f;
        float num3 = (float) (((double) num1 * 90.0 + (double) (index % 17) * 90.0 / 16.0) * (Math.PI / 180.0));
        float num4 = (float) ((num1 == 0 || num1 == 3 ? 1.0 : -1.0) * ((double) size.y / 2.0 - (double) num2) + (double) Mathf.Cos(num3) * (double) num2);
        float num5 = (float) ((num1 < 2 ? 1.0 : -1.0) * ((double) size.z / 2.0 - (double) num2) + (double) Mathf.Sin(num3) * (double) num2);
        vector3Array[index] = new Vector3(size.x / 2f, num4, num5);
        vector3Array[68 + index] = new Vector3((float) (-(double) size.x / 2.0), num4, num5);
        int num6 = (index + 1) % 68;
        intList.AddRange((IEnumerable<int>) new int[12]
        {
          136,
          index,
          num6,
          137,
          68 + num6,
          68 + index,
          index,
          68 + index,
          68 + num6,
          index,
          68 + num6,
          num6
        });
      }
      vector3Array[136] = new Vector3(size.x / 2f, 0.0f, 0.0f);
      vector3Array[137] = new Vector3((float) (-(double) size.x / 2.0), 0.0f, 0.0f);
      mesh2 = new Mesh()
      {
        vertices = vector3Array,
        triangles = intList.ToArray()
      };
    }
    ((Object) mesh2).name = "TaurusDesign/" + key;
    mesh2.RecalculateBounds();
    mesh2.RecalculateNormals();
    this.meshes.Add(key, mesh2);
    return mesh2;
  }
  public void Dispose()
  {
    this.imported?.Dispose();
    this.kyra?.Dispose();
    this.telAviv?.Dispose();
    this.fizzo?.Dispose();
    this.parrot?.Dispose();
    foreach (ImportedAssetMenu importedAssetMenu in this.assets.Values)
      importedAssetMenu.Dispose();
    foreach (ImportedNativeMenu importedNativeMenu in this.native.Values)
      importedNativeMenu.Dispose();
    foreach (KeyValuePair<MeshFilter, Mesh> original in this.originals)
    {
      if (((Object) original.Key != (Object) null))
        original.Key.sharedMesh = original.Value;
    }
    foreach (Object @object in this.meshes.Values)
      MenuDesign.Destroy(@object);
    MenuDesign.Destroy((Object) ((Component) this.root.Find("DesignSettings")).gameObject);
  }
  private static void Destroy(UnityEngine.Object item)
  {
    if (Application.isPlaying)
      Object.Destroy(item);
    else
      Object.DestroyImmediate(item);
  }
}
