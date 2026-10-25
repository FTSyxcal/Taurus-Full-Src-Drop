
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class ImportedSurface : IDisposable
{
  private static int sequence;
  private readonly Transform root;
  private readonly Renderer[] originals;
  private readonly Camera camera;
  private readonly GameObject screen;
  private readonly Mesh mesh;
  private readonly Material material;
  private readonly List<Material> materials = new List<Material>();
  private readonly int createdFrame;
  private Vector3 center;
  private Vector3 right;
  private Vector3 up;
  private float scale;
  private bool active;
  private bool pending;
  private int settleFrame;
  private int lastRenderFrame = -1;
  public GameObject Source { get; }
  public RenderTexture Texture { get; }
  public int RenderCount { get; private set; }
  public ImportedSurface(
    Transform root,
    GameObject source,
    Vector3 forward,
    Vector3 up,
    bool nativeSize = false)
  {
    this.root = root;
    this.Source = source;
    this.createdFrame = Time.frameCount;
    this.originals = ((Component) root).GetComponentsInChildren<Renderer>(true);
    this.Source.transform.position = new Vector3(32f, (float) (-1000.0 - (double) ++ImportedSurface.sequence * 2.0), 0.0f);
    foreach (Component componentsInChild in this.Source.GetComponentsInChildren<Transform>(true))
      componentsInChild.gameObject.layer = 31 ;
    if (Application.isPlaying)
      Object.DontDestroyOnLoad((Object) this.Source);
    Shader shader = ((TMP_Asset) ((Component) root.Find("XyferFont")).GetComponent<TMP_Text>().font).material.shader;
    foreach (Renderer componentsInChild in this.Source.GetComponentsInChildren<Renderer>(true))
    {
      TMP_Text component = ((Component) componentsInChild).GetComponent<TMP_Text>();
      Material[] materialArray = new Material[componentsInChild.sharedMaterials.Length];
      for (int index = 0; index < materialArray.Length; ++index)
      {
        Material sharedMaterial = componentsInChild.sharedMaterials[index];
        if (!((Object) sharedMaterial == (Object) null))
        {
          if (nativeSize)
            materialArray[index] = sharedMaterial;
          else if (nativeSize && ((Object) sharedMaterial.shader).name != "GorillaTag/UberShader")
          {
            materialArray[index] = sharedMaterial;
          }
          else
          {
            Material material;
            if (((Object) component != (Object) null))
            {
              material = new Material(sharedMaterial)
              {
                shader = shader
              };
              component.fontSharedMaterial = material;
            }
            else
            {
              Color color = sharedMaterial.HasProperty("_BaseColor") ? sharedMaterial.GetColor("_BaseColor") : (sharedMaterial.HasProperty("_Color") ? sharedMaterial.color : Color.white);
              material = new Material(((Component) root.Find("Background")).GetComponent<Renderer>().sharedMaterial)
              {
                color = color
              };
              material.mainTexture = sharedMaterial.HasProperty("_BaseMap") ? sharedMaterial.GetTexture("_BaseMap") : (sharedMaterial.HasProperty("_MainTex") ? sharedMaterial.mainTexture : (UnityEngine.Texture) null);
            }
            this.materials.Add(material);
            materialArray[index] = material;
          }
        }
      }
      componentsInChild.sharedMaterials = materialArray;
    }
    GameObject gameObject = new GameObject("ImportedDesignCamera");
    this.camera = gameObject.AddComponent<Camera>();
    ((Component) this.camera).transform.rotation = Quaternion.LookRotation(forward, up);
    this.up = ((Component) this.camera).transform.up;
    this.right = ((Component) this.camera).transform.right;
    Bounds bounds = ImportedSurface.BoundsOf(this.Source.transform);
    if (nativeSize)
      (bounds).Expand(0.008f);
    this.center = (bounds).center;
    Vector2 vector2 = this.ProjectSize(bounds);
    this.scale = nativeSize ? 1f : Mathf.Min(0.43f / vector2.x, 0.55f / vector2.y);
    float num1 = vector2.x * this.scale;
    float num2 = vector2.y * this.scale;
    Transform transform = ((Component) this.camera).transform;
    Vector3 center = this.center;
    Vector3 vector3_1 = forward;
    Vector3 size1 = (bounds).size;
    double num3 = (double) (size1).magnitude + 1.0;
    Vector3 vector3_2 = (vector3_1 * (float) num3);
    Vector3 vector3_3 = (center - vector3_2);
    transform.position = vector3_3;
    this.camera.orthographic = true;
    this.camera.orthographicSize = vector2.y / 2f;
    this.camera.nearClipPlane = 0.01f;
    Camera camera = this.camera;
    Vector3 size2 = (bounds).size;
    double num4 = (double) (size2).magnitude * 2.0 + 2.0;
    camera.farClipPlane = (float) num4;
    this.camera.clearFlags = (CameraClearFlags) 2;
    this.camera.backgroundColor = Color.clear;
    this.camera.cullingMask = int.MinValue;
    this.camera.useOcclusionCulling = false;
    MenuRender.Configure(this.camera);
    if (Application.isPlaying)
      Object.DontDestroyOnLoad((Object) gameObject);
    this.Texture = new RenderTexture(Mathf.CeilToInt(num1 * 4000f), Mathf.CeilToInt(num2 * 4000f), 24)
    {
      antiAliasing = 4
    };
    this.camera.targetTexture = this.Texture;
    this.camera.aspect = vector2.x / vector2.y;
    foreach (Canvas componentsInChild in this.Source.GetComponentsInChildren<Canvas>(true))
    {
      componentsInChild.renderMode = (RenderMode) 2;
      componentsInChild.worldCamera = this.camera;
    }
    this.screen = new GameObject("OriginalDesignSurface", new Type[2]
    {
      typeof (MeshFilter),
      typeof (MeshRenderer)
    });
    this.screen.layer = ((Component) root).gameObject.layer;
    this.screen.transform.SetParent(root, false);
    this.screen.transform.localPosition = new Vector3(0.075f, 0.0f, 0.0f);
    this.mesh = new Mesh()
    {
      vertices = new Vector3[4]
      {
        new Vector3(0.0f, num1 / 2f, (float) (-(double) num2 / 2.0)),
        new Vector3(0.0f, (float) (-(double) num1 / 2.0), (float) (-(double) num2 / 2.0)),
        new Vector3(0.0f, (float) (-(double) num1 / 2.0), num2 / 2f),
        new Vector3(0.0f, num1 / 2f, num2 / 2f)
      },
      uv = new Vector2[4]
      {
        Vector2.zero,
        Vector2.right,
        Vector2.one,
        Vector2.up
      },
      triangles = new int[6]{ 0, 2, 1, 0, 3, 2 }
    };
    this.mesh.RecalculateBounds();
    this.mesh.RecalculateNormals();
    this.screen.GetComponent<MeshFilter>().sharedMesh = this.mesh;
    this.material = new Material(((Component) root.Find("ImportedUIReference")).GetComponent<Renderer>().sharedMaterial)
    {
      mainTexture = (UnityEngine.Texture) this.Texture
    };
    this.screen.GetComponent<Renderer>().sharedMaterial = this.material;
  }
  public static Bounds BoundsOf(Transform control)
  {
    BoxCollider component = ((Component) control).GetComponent<BoxCollider>();
    if (((Object) component != (Object) null))
    {
      Bounds bounds;
      bounds = new Bounds(control.TransformPoint(component.center), Vector3.zero);
      for (int index = 0; index < 8; ++index)
        (bounds).Encapsulate(control.TransformPoint((component.center + Vector3.Scale((component.size / 2f), new Vector3((index & 1) == 0 ? -1f : 1f, (index & 2) == 0 ? -1f : 1f, (index & 4) == 0 ? -1f : 1f)))));
      return bounds;
    }
    Bounds bounds1;
    bounds1 = new Bounds(control.position, Vector3.zero);
    bool flag = true;
    foreach (Renderer componentsInChild in ((Component) control).GetComponentsInChildren<Renderer>(true))
    {
      if (flag)
      {
        bounds1 = componentsInChild.bounds;
        flag = false;
      }
      else
        (bounds1).Encapsulate(componentsInChild.bounds);
    }
    foreach (Graphic componentsInChild in ((Component) control).GetComponentsInChildren<Graphic>(true))
    {
      Vector3[] vector3Array = new Vector3[4];
      componentsInChild.rectTransform.GetWorldCorners(vector3Array);
      foreach (Vector3 vector3 in vector3Array)
        (bounds1).Encapsulate(vector3);
    }
    return bounds1;
  }
  private static Vector3 Abs(Vector3 value)
  {
    return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
  }
  private Vector2 ProjectSize(Bounds bounds)
  {
    return new Vector2(Vector3.Dot(ImportedSurface.Abs(this.right), (bounds).size), Vector3.Dot(ImportedSurface.Abs(this.up), (bounds).size));
  }
  public void Map(Transform proxy, Transform control)
  {
    Bounds bounds = ImportedSurface.BoundsOf(control);
    Vector3 vector3 = ((bounds).center - this.center);
    proxy.position = this.root.TransformPoint(new Vector3(0.08f, -Vector3.Dot(vector3, this.right) * this.scale, Vector3.Dot(vector3, this.up) * this.scale));
    Vector2 vector2 = (this.ProjectSize(bounds) * this.scale);
    BoxCollider component = ((Component) proxy).GetComponent<BoxCollider>();
    component.center = Vector3.zero;
    component.size = (new Vector3(0.014f, vector2.x, vector2.y) / proxy.localScale.x);
  }
  public void SetActive(bool value)
  {
    this.active = value;
    this.Source.SetActive(value);
    this.screen.SetActive(value);
    foreach (Renderer original in this.originals)
    {
      if (((Object) original != (Object) null))
        original.enabled = !value;
    }
    this.pending = value;
  }
  public void HideOriginals(MenuState state)
  {
    foreach (Renderer original in this.originals)
    {
      if (((Object) original != (Object) null))
        original.enabled = false;
    }
    string[] strArray = new string[1]{ "Keyboard" };
    foreach (string str in strArray)
    {
      if (state.KeyboardOpen)
      {
        foreach (Renderer componentsInChild in ((Component) this.root.Find(str)).GetComponentsInChildren<Renderer>(true))
          componentsInChild.enabled = true;
      }
    }
    this.root.Find("ColorPicker").localPosition = new Vector3(0.03f, 0.0f, 0.025f);
    this.root.Find("Keyboard").localPosition = new Vector3(0.03f, 0.0f, 0.0f);
  }
  public void Dirty()
  {
    this.pending = true;
    this.settleFrame = Time.frameCount + 1;
  }
  public void Tick()
  {
    if (this.active && !this.Texture.IsCreated())
    {
      this.Texture.Create();
      this.pending = true;
    }
    if (!this.active || !this.pending || !MenuRender.Ready || Application.isPlaying && this.lastRenderFrame == Time.frameCount)
      return;
    Canvas.ForceUpdateCanvases();
    foreach (TMP_Text componentsInChild in this.Source.GetComponentsInChildren<TMP_Text>())
    {
      if (componentsInChild.havePropertiesChanged)
        componentsInChild.ForceMeshUpdate(false, false);
    }
    Canvas.ForceUpdateCanvases();
    if (!MenuRender.TryRender(this.camera))
      return;
    this.pending = Application.isPlaying && Time.frameCount < this.settleFrame;
    this.lastRenderFrame = Time.frameCount;
    ++this.RenderCount;
  }
  public void Dispose()
  {
    ImportedSurface.Destroy((Object) this.Source);
    ImportedSurface.Destroy((Object) ((Component) this.camera).gameObject);
    ImportedSurface.Destroy((Object) this.screen);
    this.Texture.Release();
    ImportedSurface.Destroy((Object) this.Texture);
    ImportedSurface.Destroy((Object) this.mesh);
    ImportedSurface.Destroy((Object) this.material);
    foreach (Object material in this.materials)
      ImportedSurface.Destroy(material);
  }
  private static void Destroy(UnityEngine.Object item)
  {
    if (Application.isPlaying)
      Object.Destroy(item);
    else
      Object.DestroyImmediate(item);
  }
}
