
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaNetworking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public static class LobbyActions
{
  public static void JoinRandomPublic()
  {
    PhotonNetworkController instance = PhotonNetworkController.Instance;
    GorillaNetworkJoinTrigger networkJoinTrigger1 = !((Object) instance == (Object) null) ? instance.currentJoinTrigger : throw new InvalidOperationException("Networking is not ready.");
    if (((Object) networkJoinTrigger1 == (Object) null) || networkJoinTrigger1.networkZone == "private")
    {
      float num = float.PositiveInfinity;
      foreach (GorillaNetworkJoinTrigger networkJoinTrigger2 in Object.FindObjectsByType<GorillaNetworkJoinTrigger>((FindObjectsSortMode) 0))
      {
        if (!(networkJoinTrigger2.networkZone == "private"))
        {
          Vector3 vector3 = (((Component) networkJoinTrigger2).transform.position - ((Component) GorillaTagger.Instance).transform.position);
          float sqrMagnitude = (vector3).sqrMagnitude;
          if ((double) sqrMagnitude < (double) num)
          {
            num = sqrMagnitude;
            networkJoinTrigger1 = networkJoinTrigger2;
          }
        }
      }
    }
    if (((Object) networkJoinTrigger1 == (Object) null) || networkJoinTrigger1.networkZone == "private")
      throw new InvalidOperationException("No public map join trigger is loaded.");
    instance.AttemptToJoinPublicRoom(networkJoinTrigger1, (JoinType) 0, (List<(string, string)>) null, false);
  }
  public static async Task<string> CreatePublic()
  {
    PhotonNetworkController instance = PhotonNetworkController.Instance;
    NetworkSystem network = NetworkSystem.Instance;
    if (((Object) instance == (Object) null) || ((Object) network == (Object) null) || ((Object) GorillaComputer.instance == (Object) null) || ((Object) GorillaTagger.Instance == (Object) null))
      throw new InvalidOperationException("Networking is not ready.");
    GorillaNetworkJoinTrigger networkJoinTrigger1 = instance.currentJoinTrigger;
    if (((Object) networkJoinTrigger1 == (Object) null) || networkJoinTrigger1.networkZone == "private")
    {
      networkJoinTrigger1 = (GorillaNetworkJoinTrigger) null;
      float num = float.PositiveInfinity;
      foreach (GorillaNetworkJoinTrigger networkJoinTrigger2 in Object.FindObjectsByType<GorillaNetworkJoinTrigger>((FindObjectsSortMode) 0))
      {
        if (!(networkJoinTrigger2.networkZone == "private"))
        {
          Vector3 vector3 = (((Component) networkJoinTrigger2).transform.position - ((Component) GorillaTagger.Instance).transform.position);
          float sqrMagnitude = (vector3).sqrMagnitude;
          if ((double) sqrMagnitude < (double) num)
          {
            num = sqrMagnitude;
            networkJoinTrigger1 = networkJoinTrigger2;
          }
        }
      }
    }
    if (((Object) networkJoinTrigger1 == (Object) null))
      throw new InvalidOperationException("No public map join trigger is loaded.");
    GameModeType result;
    if (!Enum.TryParse<GameModeType>(GorillaComputer.instance.currentGameMode.Value, true, out result))
      throw new InvalidOperationException("Unknown game mode.");
    byte num1 = (byte) AccessTools.Method(typeof (PhotonNetworkController).Assembly.GetType("RoomSystem", true), "GetRoomSizeForCreate", (Type[]) null, (Type[]) null).Invoke((object) null, new object[4]
    {
      (object) networkJoinTrigger1.zone,
      (object) result,
      (object) false,
      (object) false
    });
    RoomConfig roomConfig = new RoomConfig()
    {
      createIfMissing = true,
      isJoinable = true,
      isPublic = true,
      MaxPlayers = num1
    };
    roomConfig.CustomProps[(object) "gameMode"] = (object) networkJoinTrigger1.GetFullDesiredGameModeString();
    Hashtable customProps = roomConfig.CustomProps;
    if (!(AccessTools.Field(typeof (PhotonNetworkController), "platformTag")?.GetValue((object) instance) is string str1))
      str1 = "OTHER";
    customProps[(object) "platform"] = (object) str1;
    roomConfig.CustomProps[(object) "queueName"] = (object) GorillaComputer.instance.currentQueue;
    instance.currentJoinTrigger = networkJoinTrigger1;
    AccessTools.Field(typeof (PhotonNetworkController), "currentJoinType")?.SetValue((object) instance, (object) (JoinType) 0);
    string roomName = (string) AccessTools.Method(typeof (PhotonNetworkController), "RandomRoomName", (Type[]) null, (Type[]) null).Invoke((object) instance, (object[]) null);
    NetJoinResult room = await network.ConnectToRoom(roomName, roomConfig, -1);
    if (!network.InRoom || network.CurrentRoom == null || !network.CurrentRoom.isPublic || network.RoomName != roomName)
      throw new InvalidOperationException("Public room creation failed: " + room.ToString());
    string str2 = roomName;
    network = (NetworkSystem) null;
    roomName = (string) null;
    return str2;
  }
}
