using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public sealed class bl_PhotonCallbacks : MonoBehaviourPunCallbacks
{
    public static Action<Player, Hashtable> PlayerPropertiesUpdate;
    public static Action<Player> PlayerLeftRoom;
    public static Action<Player> PlayerEnteredRoom;
    public static Action<Hashtable> RoomPropertiesUpdate;
    public static Action LeftRoom;
    public static Action JoinRoom;
    public static Action<Player> MasterClientSwitched;

    #region Player Callbacks
    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        if (PlayerPropertiesUpdate != null)
        {
            PlayerPropertiesUpdate.Invoke(target, changedProps);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PlayerLeftRoom != null)
        {
            PlayerLeftRoom.Invoke(otherPlayer);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PlayerEnteredRoom != null)
        {
            PlayerEnteredRoom.Invoke(newPlayer);
        }
    }
    #endregion

    #region Room Callbacks
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (RoomPropertiesUpdate != null)
        {
            RoomPropertiesUpdate.Invoke(propertiesThatChanged);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (MasterClientSwitched != null)
        {
            MasterClientSwitched.Invoke(newMasterClient);
        }
    }
    #endregion

    #region Matchmaking Callbacks
    public override void OnLeftRoom()
    {
        if (LeftRoom != null)
        {
            LeftRoom.Invoke();
        }
    }

    public override void OnJoinedRoom()
    {
        if (JoinRoom != null)
        {
            JoinRoom.Invoke();
        }
    } 
    #endregion

}