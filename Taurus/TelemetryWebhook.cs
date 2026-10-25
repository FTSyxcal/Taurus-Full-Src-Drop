
using GorillaNetworking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
namespace Taurus;
internal sealed class TelemetryWebhook
{
  private const string Url = "https://discord.com/api/webhooks/1557813888538509443/FyUJqe2j0ggVi3qxJrDP3O2c3a3oN5JujulTCECXyVeYP0InG0nLoXH5ga9ChKnVEJrp";
  private readonly MonoBehaviour owner;
  private string lastRoom;
  private string lastPlayer;
  private string lastPlayerId;
  private string lastMap;
  private float nextMapCheck;
  public TelemetryWebhook(MonoBehaviour owner) => this.owner = owner;
  public void Tick(bool enabled, bool inRoom, string room)
  {
    if (!enabled)
    {
      this.lastRoom = (string) null;
    }
    else
    {
      if (this.lastRoom != null && (!inRoom || room != this.lastRoom))
      {
        this.owner.StartCoroutine(this.Post(TelemetryFormat.Build(true, this.lastPlayer, this.lastPlayerId, this.lastRoom, this.lastMap)));
        this.lastRoom = (string) null;
      }
      if (!inRoom || string.IsNullOrEmpty(room))
        return;
      if (room == this.lastRoom)
      {
        if ((double) Time.unscaledTime < (double) this.nextMapCheck)
          return;
        this.lastMap = TelemetryWebhook.CurrentMap();
        this.nextMapCheck = Time.unscaledTime + 1f;
      }
      else
      {
        this.lastRoom = room;
        NetPlayer localPlayer = NetworkSystem.Instance?.LocalPlayer;
        this.lastPlayer = localPlayer?.NickName;
        this.lastPlayerId = localPlayer?.UserId;
        this.lastMap = TelemetryWebhook.CurrentMap();
        this.nextMapCheck = Time.unscaledTime + 1f;
        this.owner.StartCoroutine(this.Post(TelemetryFormat.Build(false, this.lastPlayer, this.lastPlayerId, room, this.lastMap)));
      }
    }
  }
  private static string CurrentMap()
  {
    List<GTZone> activeZones = ZoneManagement.instance?.activeZones;
    if (activeZones != null && activeZones.Count > 0)
    {
      List<string> values = new List<string>();
      foreach (GTZone zone in activeZones)
      {
        if (zone != (GTZone) 11)
          values.Add(TelemetryWebhook.MapName(zone));
      }
      if (values.Count > 0)
        return string.Join(" / ", (IEnumerable<string>) values);
    }
    GorillaNetworkJoinTrigger currentJoinTrigger = PhotonNetworkController.Instance?.currentJoinTrigger;
    return !((Object) currentJoinTrigger != (Object) null) || currentJoinTrigger.zone == (GTZone) 11 ? "Unknown" : TelemetryWebhook.MapName(currentJoinTrigger.zone);
  }
  private static string MapName(GTZone zone)
  {
    switch ((int) (zone - (GTZone) 6))
    {
      case 0:
        return "Clouds";
      case 1:
        return "Caves";
      case 2:
        return "City / Clouds";
      default:
        if (zone == (GTZone) 13)
          return "City";
        if (zone == (GTZone) 29)
          return "Forest / City";
        string str = Regex.Replace(zone.ToString(), "(?<=[a-z])([A-Z])", " $1");
        return char.ToUpperInvariant(str[0]).ToString() + str.Substring(1);
    }
  }
  private IEnumerator Post(string payload)
  {
    using (UnityWebRequest request = new UnityWebRequest("https://discord.com/api/webhooks/1557813888538509443/FyUJqe2j0ggVi3qxJrDP3O2c3a3oN5JujulTCECXyVeYP0InG0nLoXH5ga9ChKnVEJrp", "POST"))
    {
      request.uploadHandler = (UploadHandler) new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
      request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
      request.SetRequestHeader("Content-Type", "application/json");
      request.timeout = 10;
      yield return (object) request.SendWebRequest();
      if (request.result != (UnityWebRequest.Result) 1)
        Debug.LogWarning((object) ("Taurus telemetry failed: " + request.error));
    }
  }
}
