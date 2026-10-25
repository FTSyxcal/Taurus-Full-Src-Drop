
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class GunLib : IDisposable
{
  private readonly GameObject root;
  private readonly LineRenderer line;
  private readonly GameObject ball;
  private readonly Renderer ballRenderer;
  private readonly Mesh reticleMesh;
  private Camera viewCamera;
  private Color previousLeft;
  private Color previousRight;
  private bool colored;
  private int design = -1;
  private readonly MaterialPropertyBlock colors = new MaterialPropertyBlock();
  private bool wasTriggered;
  private RaycastHit[] hits = new RaycastHit[64 ];
  private Camera[] cameras = new Camera[8];
  private Camera shoulder;
  private float nextCameraSearch;
  public Transform LockedBody { get; private set; }
  public Vector3 TargetPoint { get; private set; }
  public Vector3 TargetNormal { get; private set; }
  public bool IsVisible => this.root.activeSelf && this.ball.activeSelf;
  public bool HasHit { get; private set; }
  public Vector3 VisualOrigin => !this.IsVisible ? Vector3.zero : this.line.GetPosition(0);
  public bool IsPressed => this.wasTriggered;
  public GunLib CreateRemoteVisual()
  {
    return new GunLib(((Renderer) this.line).sharedMaterial, this.ballRenderer.sharedMaterial);
  }
  public void ShowRemote(Vector3 start, Vector3 end, bool locked)
  {
    this.root.SetActive(true);
    this.ball.SetActive(true);
    for (int index = 0; index < this.line.positionCount; ++index)
      this.line.SetPosition(index, Vector3.Lerp(start, end, (float) index / (float) (this.line.positionCount - 1)));
    this.ball.transform.position = end;
    Camera main = Camera.main;
    if (((Object) main != (Object) null))
      this.ball.transform.rotation = ((Component) main).transform.rotation;
    this.ball.transform.localScale = (Vector3.one * (locked ? 0.115f : 0.14f));
    if (!locked)
      return;
    this.ball.transform.Rotate(Vector3.forward, 45f, (Space) 1);
  }
  public event Action<Transform> Triggered;
  public event Action<Vector3> PointTriggered;
  public GunLib(Material lineMaterial, Material ballMaterial)
  {
    this.root = new GameObject("TaurusGun");
    this.root.layer = 2;
    if (Application.isPlaying)
      Object.DontDestroyOnLoad((Object) this.root);
    this.line = this.root.AddComponent<LineRenderer>();
    ((Renderer) this.line).sharedMaterial = lineMaterial;
    this.line.positionCount = 2;
    this.line.startWidth = 0.009f;
    this.line.endWidth = 1f / 500f;
    this.line.numCapVertices = 4;
    this.line.useWorldSpace = true;
    this.line.alignment = (LineAlignment) 0;
    ((Renderer) this.line).shadowCastingMode = (ShadowCastingMode) 0;
    ((Renderer) this.line).receiveShadows = false;
    this.ball = new GameObject("TaurusDiamondReticle", new Type[2]
    {
      typeof (MeshFilter),
      typeof (MeshRenderer)
    });
    this.ball.layer = 2;
    this.ball.transform.SetParent(this.root.transform, false);
    this.ball.transform.localScale = (Vector3.one * 0.14f);
    this.reticleMesh = GunLib.CreateReticle();
    this.ball.GetComponent<MeshFilter>().sharedMesh = this.reticleMesh;
    this.ballRenderer = this.ball.GetComponent<Renderer>();
    this.ballRenderer.sharedMaterial = ballMaterial;
    this.ballRenderer.shadowCastingMode = (ShadowCastingMode) 0;
    this.ballRenderer.receiveShadows = false;
    this.root.SetActive(false);
  }
  public void Update(
    Transform hand,
    bool aiming,
    bool trigger,
    IReadOnlyList<GunLib.BodyTarget> targets,
    Transform localRig = null,
    Transform localPlayer = null)
  {
    if (((Object) hand == (Object) null))
      this.Hide();
    else
      this.Update((hand.position + (hand.forward * 0.25f)), hand.forward, aiming, trigger, targets, localRig, localPlayer: localPlayer);
  }
  public Camera DesktopCamera()
  {
    if (((Object) this.shoulder == (Object) null) && (double) Time.unscaledTime >= (double) this.nextCameraSearch)
    {
      this.nextCameraSearch = Time.unscaledTime + 1f;
      GameObject gameObject = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera") ?? GameObject.Find("Shoulder Camera");
      if (((Object) gameObject != (Object) null))
        this.shoulder = gameObject.GetComponent<Camera>();
    }
    if (((Object) this.shoulder != (Object) null) && ((Behaviour) this.shoulder).isActiveAndEnabled)
      return this.shoulder;
    if (Camera.allCamerasCount > this.cameras.Length)
      Array.Resize<Camera>(ref this.cameras, Camera.allCamerasCount);
    int allCameras = Camera.GetAllCameras(this.cameras);
    Camera camera1 = (Camera) null;
    Camera main = Camera.main;
    for (int index = 0; index < allCameras; ++index)
    {
      Camera camera2 = this.cameras[index];
      if (!((Object) camera2 == (Object) null) && ((Behaviour) camera2).isActiveAndEnabled && !((Object) camera2.targetTexture != (Object) null) && camera2.targetDisplay == 0 && (camera2.cullingMask & int.MaxValue) != 0 && camera2.pixelWidth > 0 && camera2.pixelHeight > 0 && (((Object) camera1 == (Object) null) || (double) camera2.depth > (double) camera1.depth || (double) camera2.depth == (double) camera1.depth && ((Object) camera2 == (Object) main)))
        camera1 = camera2;
    }
    return camera1;
  }
  public void UseCamera(Camera camera)
  {
    this.viewCamera = camera;
    int num = 2;
    if (((Object) camera != (Object) null))
    {
      for (int index = 0; index < 31 ; ++index)
      {
        if ((camera.cullingMask & 1 << index) != 0)
        {
          num = index;
          break;
        }
      }
    }
    this.root.layer = this.ball.layer = num;
  }
  public static bool DesktopRay(Camera camera, Vector2 cursor, bool centered, out Ray ray)
  {
    ray = new Ray();
    if (((Object) camera == (Object) null) || camera.pixelWidth <= 0 || camera.pixelHeight <= 0 || !GunLib.Finite(new Vector3(cursor.x, cursor.y, 0.0f)))
      return false;
    Rect pixelRect = camera.pixelRect;
    if (((Object) camera.targetTexture != (Object) null) && Application.isPlaying && Screen.width > 0 && Screen.height > 0)
    {
      Rect rect;
      rect = new Rect(0.0f, 0.0f, (float) Screen.width, (float) Screen.height);
      if (!centered && !(rect).Contains(cursor))
        return false;
      ray = camera.ViewportPointToRay(centered ? new Vector3(0.5f, 0.5f) : new Vector3(cursor.x / (rect).width, cursor.y / (rect).height));
      return GunLib.Finite((ray).origin) && GunLib.Finite((ray).direction);
    }
    if (centered)
      cursor = (pixelRect).center;
    else if (!(pixelRect).Contains(cursor))
      return false;
    ray = camera.ScreenPointToRay(((Vector2)cursor));
    return GunLib.Finite((ray).origin) && GunLib.Finite((ray).direction);
  }
  public void Update(
    Vector3 origin,
    Vector3 direction,
    bool aiming,
    bool trigger,
    IReadOnlyList<GunLib.BodyTarget> targets,
    Transform localRig = null,
    Vector3? visualOrigin = null,
    Transform localPlayer = null)
  {
    if (!aiming || !GunLib.Finite(origin) || !GunLib.Finite(direction) || (double) (direction).sqrMagnitude < 9.9999997473787516E-05 || float.IsInfinity((direction).sqrMagnitude))
    {
      this.Hide();
    }
    else
    {
      (direction).Normalize();
      targets = (IReadOnlyList<GunLib.BodyTarget>) ((object) targets ?? (object) Array.Empty<GunLib.BodyTarget>());
      this.root.SetActive(true);
      if (!trigger || ((Object) this.LockedBody == (Object) null) || !((Component) this.LockedBody).gameObject.activeInHierarchy)
        this.LockedBody = (Transform) null;
      if (((Object) this.LockedBody != (Object) null))
      {
        bool flag = false;
        foreach (GunLib.BodyTarget target in (IEnumerable<GunLib.BodyTarget>) targets)
        {
          if (((Object) target.Body == (Object) this.LockedBody) && GunLib.Finite(target.Body.position))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
          this.LockedBody = (Transform) null;
      }
      if (((Object) this.LockedBody != (Object) null))
      {
        Vector3 vector3 = (this.LockedBody.position - origin);
        direction = (vector3).normalized;
      }
      float num1 = 100f;
      Vector3 vector3_1 = (origin + (direction * 5f));
      bool flag1 = false;
      Vector3 vector3_2 = Vector3.up;
      int num2;
      for (num2 = Physics.RaycastNonAlloc(origin, direction, this.hits, 100f, 2147483643, (QueryTriggerInteraction) 1); num2 == this.hits.Length && this.hits.Length < 4096 ; num2 = Physics.RaycastNonAlloc(origin, direction, this.hits, 100f, 2147483643, (QueryTriggerInteraction) 1))
        Array.Resize<RaycastHit>(ref this.hits, this.hits.Length * 2);
      RaycastHit[] raycastHitArray = this.hits;
      if (num2 == this.hits.Length)
      {
        raycastHitArray = Physics.RaycastAll(origin, direction, 100f, 2147483643, (QueryTriggerInteraction) 1);
        num2 = raycastHitArray.Length;
      }
      for (int index = 0; index < num2; ++index)
      {
        Transform transform = ((Component) (raycastHitArray[index]).collider).transform;
        if ((!((Object) localRig != (Object) null) || !transform.IsChildOf(localRig)) && (!((Object) localPlayer != (Object) null) || !transform.IsChildOf(localPlayer)))
        {
          bool flag2 = false;
          foreach (GunLib.BodyTarget target in (IEnumerable<GunLib.BodyTarget>) targets)
          {
            if (((Object) target.Root != (Object) null) && transform.IsChildOf(target.Root))
            {
              flag2 = true;
              break;
            }
          }
          if (!flag2 && (double) (raycastHitArray[index]).distance < (double) num1)
          {
            num1 = (raycastHitArray[index]).distance;
            vector3_1 = (raycastHitArray[index]).point;
            vector3_2 = (raycastHitArray[index]).normal;
            flag1 = true;
          }
        }
      }
      Transform transform1 = this.LockedBody;
      if (((Object) transform1 != (Object) null) && (double) Vector3.Distance(origin, transform1.position) - 0.30000001192092896 > (double) num1)
        transform1 = this.LockedBody = (Transform) null;
      if (((Object) transform1 == (Object) null))
      {
        foreach (GunLib.BodyTarget target in (IEnumerable<GunLib.BodyTarget>) targets)
        {
          if (!((Object) target.Body == (Object) null) && ((Component) target.Body).gameObject.activeInHierarchy && GunLib.Finite(target.Body.position) && !float.IsNaN(target.Radius) && !float.IsInfinity(target.Radius) && (double) target.Radius > 0.0)
          {
            Vector3 vector3_3 = (target.Body.position - origin);
            float num3 = Vector3.Dot(vector3_3, direction);
            float num4 = Mathf.Max(0.0f, (vector3_3).sqrMagnitude - num3 * num3);
            float num5 = target.Radius * target.Radius;
            if ((double) num3 >= 0.0 && (double) num4 <= (double) num5)
            {
              float num6 = Mathf.Max(0.0f, num3 - Mathf.Sqrt(num5 - num4));
              if ((double) num6 < (double) num1)
              {
                num1 = num6;
                transform1 = target.Body;
              }
            }
          }
        }
        if (trigger)
          this.LockedBody = transform1;
      }
      this.TargetPoint = ((Object) transform1 != (Object) null) ? transform1.position : vector3_1;
      this.TargetNormal = ((Object) transform1 != (Object) null) ? Vector3.up : vector3_2;
      this.HasHit = ((Object) transform1 != (Object) null) | flag1;
      this.ball.SetActive(true);
      this.ball.transform.position = this.TargetPoint;
      Camera camera = ((Object) this.viewCamera != (Object) null) ? this.viewCamera : Camera.main;
      this.ball.transform.rotation = ((Object) camera != (Object) null) ? ((Component) camera).transform.rotation : Quaternion.FromToRotation(Vector3.forward, (-direction));
      this.ball.transform.localScale = (Vector3.one * (((Object) this.LockedBody != (Object) null) ? 0.115f : 0.14f));
      this.ball.transform.Rotate(Vector3.forward, ((Object) this.LockedBody != (Object) null) ? 45f : 0.0f, (Space) 1);
      Vector3 vector3_4 = !visualOrigin.HasValue || !GunLib.Finite(visualOrigin.Value) ? origin : visualOrigin.Value;
      for (int index = 0; index < this.line.positionCount; ++index)
        this.line.SetPosition(index, Vector3.Lerp(vector3_4, this.TargetPoint, (float) index / (float) (this.line.positionCount - 1)));
      if (trigger && !this.wasTriggered && this.HasHit)
      {
        Action<Vector3> pointTriggered = this.PointTriggered;
        if (pointTriggered != null)
          pointTriggered(this.TargetPoint);
        if (((Object) transform1 != (Object) null))
        {
          Action<Transform> triggered = this.Triggered;
          if (triggered != null)
            triggered(transform1);
        }
      }
      this.wasTriggered = trigger;
    }
  }
  public void Hide()
  {
    this.root.SetActive(false);
    this.LockedBody = (Transform) null;
    this.HasHit = false;
    this.wasTriggered = false;
  }
  public void SetDesign(int value)
  {
    if (this.design == value)
      return;
    this.design = value;
    this.colored = false;
  }
  public void SetColors(Color left, Color right)
  {
    if (this.colored && (left == this.previousLeft) && (right == this.previousRight))
      return;
    this.colored = true;
    this.previousLeft = left;
    this.previousRight = right;
    this.colors.SetColor("_LeftColor", left);
    this.colors.SetColor("_RightColor", right);
    this.ballRenderer.SetPropertyBlock(this.colors);
    this.colors.SetFloat("_UseColors", 1f);
    ((Renderer) this.line).SetPropertyBlock(this.colors);
    this.line.startColor = left;
    this.line.endColor = right;
  }
  private static bool Finite(Vector3 v)
  {
    return !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) && !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
  }
  private static Mesh CreateReticle()
  {
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    for (int index = 0; index < 4; ++index)
    {
      float num = (float) ((double) index * 3.1415927410125732 * 0.5);
      Vector2 a = (new Vector2(Mathf.Cos(num), Mathf.Sin(num)) * 0.5f);
      Vector2 b = (new Vector2(Mathf.Cos(num + 1.57079637f), Mathf.Sin(num + 1.57079637f)) * 0.5f);
      Segment(a, Vector2.Lerp(a, b, 0.36f));
      Segment(Vector2.Lerp(a, b, 0.64f), b);
    }
    Mesh reticle = new Mesh();
    ((Object) reticle).name = "Taurus segmented diamond";
    reticle.vertices = vertices.ToArray();
    reticle.triangles = triangles.ToArray();
    reticle.RecalculateBounds();
    reticle.RecalculateNormals();
    return reticle;
    void Segment(Vector2 a, Vector2 b)
    {
      Vector2 vector2_1 = new Vector2(-(b - a).y, (b - a).x);
      Vector2 vector2_2 = ((vector2_1).normalized * 0.035f);
      int count = vertices.Count;
      vertices.Add(((Vector2)(a + vector2_2)));
      vertices.Add(((Vector2)(b + vector2_2)));
      vertices.Add(((Vector2)(b - vector2_2)));
      vertices.Add(((Vector2)(a - vector2_2)));
      triangles.AddRange((IEnumerable<int>) new int[12]
      {
        count,
        count + 1,
        count + 2,
        count,
        count + 2,
        count + 3,
        count + 2,
        count + 1,
        count,
        count + 3,
        count + 2,
        count
      });
    }
  }
  public void Dispose()
  {
    if (Application.isPlaying)
      Object.Destroy((Object) this.root);
    else
      Object.DestroyImmediate((Object) this.root);
    if (Application.isPlaying)
      Object.Destroy((Object) this.reticleMesh);
    else
      Object.DestroyImmediate((Object) this.reticleMesh);
  }
  public readonly struct BodyTarget(Transform body, float radius, Transform root = null)
  {
    public readonly Transform Body = body;
    public readonly float Radius = radius;
    public readonly Transform Root = root ?? body;
  }
}
