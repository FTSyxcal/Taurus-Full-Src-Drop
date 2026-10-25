
using BepInEx.Logging;
using Photon.Pun;
using System;
using System.Globalization;
using Taurus.DiscordRPC;
using Taurus.DiscordRPC.Events;
using Taurus.DiscordRPC.Logging;
using UnityEngine;
using Object = UnityEngine.Object;
using ILogger = Taurus.DiscordRPC.Logging.ILogger;
using LogLevel = Taurus.DiscordRPC.Logging.LogLevel;
namespace Taurus;
internal sealed class DiscordPresence : IDisposable
{
  private const string AppId = "1547212248861384824";
  private DiscordRpcClient client;
  private readonly ManualLogSource log;
  private DateTime? sessionStart;
  private float nextUpdate;
  private float nextRetry;
  private string previous;
  private bool loggedPayload;
  public DiscordPresence(ManualLogSource log)
  {
    this.log = log;
    this.Connect();
  }
  private void Connect()
  {
    try
    {
      if (this.client != null && !this.client.IsDisposed)
        return;
      this.client = new DiscordRpcClient("1547212248861384824")
      {
        Logger = (ILogger) new DiscordLogManager()
        {
          Level = LogLevel.Warning
        }
      };
      this.client.OnReady += (OnReadyEvent) ((sender, message) =>
      {
        this.previous = (string) null;
        this.log.LogInfo((object) "Taurus Discord RPC connected.");
      });
      this.client.OnClose += (OnCloseEvent) ((sender, message) => this.log.LogWarning((object) "Discord RPC disconnected; reconnecting."));
      this.client.OnError += (OnErrorEvent) ((sender, message) => this.log.LogWarning((object) $"Discord RPC rejected an update: {message.Code.ToString()} {message.Message}"));
      this.client.OnPresenceUpdate += (OnPresenceUpdateEvent) ((sender, message) => this.log.LogInfo((object) "Taurus Discord RPC activity accepted."));
      this.client.Initialize();
      this.previous = (string) null;
      this.loggedPayload = false;
    }
    catch (Exception ex)
    {
      this.log.LogWarning((object) ("Discord RPC unavailable: " + ex.Message));
      try
      {
        this.client?.Dispose();
      }
      catch
      {
      }
      this.client = (DiscordRpcClient) null;
      this.nextRetry = Time.unscaledTime + 30f;
    }
  }
  public void Tick(NetworkSystem network)
  {
    try
    {
      if (this.client == null || this.client.IsDisposed)
      {
        if ((double) Time.unscaledTime < (double) this.nextRetry)
          return;
        this.nextRetry = Time.unscaledTime + 30f;
        this.Connect();
      }
      else
      {
        if ((double) Time.unscaledTime < (double) this.nextUpdate)
          return;
        this.nextUpdate = Time.unscaledTime + 1f;
        bool joined = ((Object) network != (Object) null) && network.InRoom;
        if (joined && !this.sessionStart.HasValue)
          this.sessionStart = new DateTime?(DateTime.UtcNow);
        else if (!joined)
          this.sessionStart = new DateTime?();
        string str1 = "Main Menu";
        string str2 = "";
        int count = 0;
        int max = 10;
        if (joined)
        {
          try
          {
            str2 = network.RoomName ?? "";
          }
          catch
          {
          }
          try
          {
            string[] strArray = (network.GameModeString ?? "").Split(';');
            if (strArray.Length != 0 && strArray[0].Length > 0)
              str1 = strArray[0];
            if (strArray.Length > 2)
            {
              if (strArray[2].Length > 0)
              {
                string str3 = strArray[2];
              }
            }
          }
          catch
          {
          }
          try
          {
            if (PhotonNetwork.CurrentRoom != null)
            {
              count = (int) PhotonNetwork.CurrentRoom.PlayerCount;
              max = (int) PhotonNetwork.CurrentRoom.MaxPlayers;
            }
          }
          catch
          {
          }
        }
        string titleCase = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str1.ToLowerInvariant());
        string str4 = DiscordPresenceText.State(joined, count, max);
        string str5 = DiscordPresenceText.Details(joined, titleCase);
        string str6 = $"{str5}/{str4}";
        if (str6 == this.previous)
          return;
        DiscordRpcClient client = this.client;
        RichPresence richPresence1 = new RichPresence();
        richPresence1.Details = str5;
        richPresence1.State = str4;
        richPresence1.Assets = new Taurus.DiscordRPC.Assets()
        {
          LargeImageKey = "gt_logo",
          LargeImageText = "Taurus Paid",
          SmallImageKey = joined ? "online" : "offline",
          SmallImageText = joined ? str2 : "Idle"
        };
        RichPresence richPresence2 = richPresence1;
        Timestamps timestamps;
        if (!this.sessionStart.HasValue)
        {
          timestamps = (Timestamps) null;
        }
        else
        {
          timestamps = new Timestamps();
          timestamps.Start = new DateTime?(this.sessionStart.Value);
        }
        richPresence2.Timestamps = timestamps;
        richPresence1.Buttons = new Taurus.DiscordRPC.Button[1]
        {
          new Taurus.DiscordRPC.Button()
          {
            Label = "Join our discord!",
            Url = "https://discord.gg/itzgame-s-fries-1536853966741180476"
          }
        };
        RichPresence presence = richPresence1;
        client.SetPresence(presence);
        this.previous = str6;
        if (this.loggedPayload)
          return;
        this.loggedPayload = true;
        this.log.LogInfo((object) ("Taurus Discord RPC presence: " + str4));
      }
    }
    catch (Exception ex)
    {
      this.log.LogWarning((object) ("Discord presence failed: " + ex.Message));
    }
  }
  public void Clear()
  {
    try
    {
      if (this.client != null)
      {
        if (!this.client.IsDisposed)
          this.client.ClearPresence();
      }
    }
    catch
    {
    }
    this.previous = (string) null;
  }
  public void Dispose()
  {
    try
    {
      if (this.client != null)
      {
        if (!this.client.IsDisposed)
        {
          this.client.ClearPresence();
          this.client.Dispose();
        }
      }
    }
    catch
    {
    }
    this.client = (DiscordRpcClient) null;
  }
}
