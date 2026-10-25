
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class PokrukCameraAdapter : IDisposable
{
  private static Assembly cameraAssembly;
  private GameObject host;
  private Component controller;
  private Transform shoulder;
  private Transform originalParent;
  private Vector3 originalPosition;
  private Quaternion originalRotation;
  private Camera camera;
  private float originalFov;
  private float originalNearClip;
  private Behaviour virtualCamera;
  private bool virtualCameraEnabled;
  public bool Enabled => ((Object) this.host != (Object) null);
  public void Enable()
  {
    if (this.Enabled)
      return;
    GameObject gameObject = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
    if (((Object) gameObject == (Object) null) || ((Object) GorillaTagger.Instance == (Object) null))
      throw new InvalidOperationException("Camera is not ready.");
    if (PokrukCameraAdapter.cameraAssembly == (Assembly) null)
    {
      using (Stream manifestResourceStream = typeof (Plugin).Assembly.GetManifestResourceStream("ItzGameMenu.PokruksCameraMod.dll"))
      {
        using (MemoryStream destination = new MemoryStream())
        {
          if (manifestResourceStream == null)
            throw new InvalidDataException("Embedded camera mod is missing.");
          manifestResourceStream.CopyTo((Stream) destination);
          PokrukCameraAdapter.cameraAssembly = Assembly.Load(destination.ToArray());
        }
      }
    }
    Type type = PokrukCameraAdapter.cameraAssembly.GetType("CameraMod.Camera.CameraController", true);
    if (type.GetField("Instance").GetValue((object) null) is Component component1 && ((Object) component1 != (Object) null))
      throw new InvalidOperationException("Another Pokruk camera instance is already running.");
    this.shoulder = gameObject.transform;
    this.originalParent = this.shoulder.parent;
    this.originalPosition = this.shoulder.localPosition;
    this.originalRotation = this.shoulder.localRotation;
    this.camera = ((Component) this.shoulder).GetComponent<Camera>();
    this.originalFov = this.camera.fieldOfView;
    this.originalNearClip = this.camera.nearClipPlane;
    Transform transform = this.shoulder.Find("CM vcam1");
    this.virtualCamera = ((Object) transform == (Object) null) ? (Behaviour) null : ((Component) transform).GetComponent("CinemachineVirtualCamera") as Behaviour;
    if (((Object) this.virtualCamera != (Object) null))
      this.virtualCameraEnabled = this.virtualCamera.enabled;
    this.host = new GameObject("ItzGamePokrukCamera");
    Object.DontDestroyOnLoad((Object) this.host);
    try
    {
      this.host.AddComponent(PokrukCameraAdapter.cameraAssembly.GetType("CameraMod.Camera.Comps.InputManager", true));
      Component component2 = this.host.AddComponent(PokrukCameraAdapter.cameraAssembly.GetType("CameraMod.Camera.Comps.UI", true));
      component2.GetType().GetField("watermarkEnabled").SetValue((object) component2, (object) true);
      this.controller = this.host.AddComponent(type);
      FieldInfo field = type.GetField("UpdateMode");
      field.SetValue((object) null, Enum.Parse(field.FieldType, "LateUpdate"));
      type.GetMethod("Init").Invoke((object) this.controller, (object[]) null);
    }
    catch
    {
      this.Dispose();
      throw;
    }
  }
  public void Dispose()
  {
    if (((Object) this.host == (Object) null))
      return;
    if (((Object) this.shoulder != (Object) null))
    {
      this.shoulder.SetParent(this.originalParent, false);
      this.shoulder.localPosition = this.originalPosition;
      this.shoulder.localRotation = this.originalRotation;
      this.camera.fieldOfView = this.originalFov;
      this.camera.nearClipPlane = this.originalNearClip;
    }
    if (((Object) this.virtualCamera != (Object) null))
      this.virtualCamera.enabled = this.virtualCameraEnabled;
    if (((Object) this.controller != (Object) null))
    {
      string[] strArray = new string[3]
      {
        "colorScreenGo",
        "cameraTabletT",
        "HeadCosmeticsHider"
      };
      foreach (string name in strArray)
      {
        switch (this.controller.GetType().GetField(name)?.GetValue((object) this.controller))
        {
          case GameObject gameObject when ((Object) gameObject != (Object) null):
            Object.Destroy((Object) gameObject);
            break;
          case Transform transform when ((Object) transform != (Object) null):
            Object.Destroy((Object) ((Component) transform).gameObject);
            break;
          case Component component when ((Object) component != (Object) null):
            Object.Destroy((Object) component);
            break;
        }
      }
    }
    Object.Destroy((Object) this.host);
    this.host = (GameObject) null;
    this.controller = (Component) null;
  }
}
