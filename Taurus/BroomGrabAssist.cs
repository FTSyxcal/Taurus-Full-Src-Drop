
using GorillaLocomotion;
using GorillaLocomotion.Gameplay;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class BroomGrabAssist : IDisposable
{
  private static readonly FieldInfo grabbedObject = AccessTools.Field(typeof (GorillaGrabber), "currentGrabbable");
  private readonly Action<bool> noclip;
  private readonly Action<string, bool> notify;
  private GorillaGrabber grabber;
  private NoncontrollableBroomstick target;
  private Vector3 localPoint;
  private bool previousGrip;
  private float retryAt;
  private float grabbedAt = -1f;
  public bool Active { get; private set; }
  public BroomGrabAssist(Action<bool> noclip, Action<string, bool> notify)
  {
    this.noclip = noclip;
    this.notify = notify;
  }
  public void Reset(bool held)
  {
    this.previousGrip = held;
    this.Dispose();
  }
  public void Tick(bool enabled, bool held, bool suppressInput)
  {
    bool flag = held && !this.previousGrip;
    this.previousGrip = held;
    GTPlayer instance = GTPlayer.Instance;
    if (((!enabled ? 1 : (!held ? 1 : 0)) | (suppressInput ? 1 : 0)) == 0)
    {
      if (!((Object) instance == (Object) null))
      {
        try
        {
          if (!this.Active)
          {
            if (!flag)
              return;
            this.grabber = ((IEnumerable<GorillaGrabber>) ((Component) instance).GetComponentsInChildren<GorillaGrabber>()).FirstOrDefault<GorillaGrabber>((Func<GorillaGrabber, bool>) (g => g.IsRightHand));
            if (((Object) this.grabber == (Object) null) || this.grabber.isGrabbing)
              return;
            this.target = ((IEnumerable<NoncontrollableBroomstick>) Object.FindObjectsByType<NoncontrollableBroomstick>((FindObjectsSortMode) 0)).Where<NoncontrollableBroomstick>((Func<NoncontrollableBroomstick, bool>) (b => ((Behaviour) b).isActiveAndEnabled)).OrderBy<NoncontrollableBroomstick, float>((Func<NoncontrollableBroomstick, float>) (b =>
            {
              Vector3 vector3 = (((Component) b).transform.position - ((Component) this.grabber).transform.position);
              return (vector3).sqrMagnitude;
            })).FirstOrDefault<NoncontrollableBroomstick>();
            if (((Object) this.target == (Object) null))
            {
              Action<string, bool> notify = this.notify;
              if (notify == null)
                return;
              notify("No broom in the loaded map", true);
              return;
            }
            Collider component = ((Component) this.target).GetComponent<Collider>();
            this.localPoint = ((Component) this.target).transform.InverseTransformPoint(!((Object) component != (Object) null) || !component.enabled ? ((Component) this.target).transform.position : component.ClosestPoint(((Component) this.grabber).transform.position));
            this.Active = true;
            this.retryAt = 0.0f;
            this.grabbedAt = -1f;
          }
          if (((Object) this.grabber == (Object) null) || ((Object) this.target == (Object) null) || !((Behaviour) this.target).isActiveAndEnabled || ((Object) this.grabber.Player != (Object) instance))
          {
            this.Dispose();
            return;
          }
          if (this.grabber.isGrabbing && BroomGrabAssist.grabbedObject.GetValue((object) this.grabber) == this.target)
          {
            if ((double) this.grabbedAt < 0.0)
              this.grabbedAt = Time.unscaledTime;
            if ((double) Time.unscaledTime - (double) this.grabbedAt < 0.20000000298023224)
              return;
            this.Dispose();
            Action<string, bool> notify = this.notify;
            if (notify == null)
              return;
            notify("Grabbed broom", false);
            return;
          }
          this.grabbedAt = -1f;
          if ((double) Time.unscaledTime < (double) this.retryAt)
            return;
          this.retryAt = Time.unscaledTime + 0.1f;
          Vector3 vector3_1 = ((Component) this.target).transform.TransformPoint(this.localPoint);
          this.noclip(true);
          instance.TeleportTo(((((Component) instance).transform.position + vector3_1) - ((Component) this.grabber).transform.position), ((Component) instance).transform.rotation, false, false);
          Physics.SyncTransforms();
          this.grabber.Inject(((Component) this.target).transform, this.localPoint);
          return;
        }
        catch
        {
          this.Dispose();
          throw;
        }
      }
    }
    this.Dispose();
  }
  public void Dispose()
  {
    int num = this.Active ? 1 : 0;
    this.Active = false;
    this.grabber = (GorillaGrabber) null;
    this.target = (NoncontrollableBroomstick) null;
    this.grabbedAt = -1f;
    if (num == 0)
      return;
    this.noclip(false);
  }
}
