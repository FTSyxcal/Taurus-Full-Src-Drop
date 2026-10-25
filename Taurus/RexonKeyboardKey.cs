
using System;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class RexonKeyboardKey : MonoBehaviour
{
  public string KeyValue;
  public RexonKeyboard Owner;
  private float NextAllowedPressTime;
  private void OnTriggerEnter(Collider other)
  {
    bool flag1 = this.Owner != null && ((Object) other == (Object) this.Owner.LeftHandPointer);
    bool flag2 = this.Owner != null && ((Object) other == (Object) this.Owner.RightHandPointer);
    if (!(flag1 | flag2) || (double) Time.time < (double) this.NextAllowedPressTime)
      return;
    this.NextAllowedPressTime = Time.time + 0.15f;
    Action<bool> feedback = this.Owner.Feedback;
    if (feedback != null)
      feedback(flag1);
    this.Owner.HandleKeyPress(this.KeyValue);
  }
}
