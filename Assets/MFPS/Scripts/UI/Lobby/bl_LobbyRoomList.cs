using MFPS.Internal;
using MFPS.Runtime.UI;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class bl_LobbyRoomList : bl_LobbyRoomListBase
{
    public UIListHandler roomListHandler;
    [SerializeField] private TextMeshProUGUI NoRoomText = null;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!roomListHandler.IsInitialize) roomListHandler.Initialize();
    }

    /// <summary>
    /// display all available rooms
    /// </summary>
    public override void SetRoomList(List<RoomInfo> rooms)
    {
        //Removed old list
        roomListHandler.Clear();
        UpdateCachedRoomList(rooms);
        InstanceRoomList();     
    }

    /// <summary>
    /// 
    /// </summary>
    void InstanceRoomList()
    {
        if (cachedRoomList.Count > 0)
        {
            NoRoomText.text = string.Empty;
            foreach (RoomInfo info in cachedRoomList.Values)
            {
                if (info.Name == bl_Lobby.Instance.justCreatedRoomName) continue;

                var entry = roomListHandler.InstatiateAndGet<bl_RoomListItemUIBase>();
                entry.SetInfo(info);
            }
        }
        else
        {
            NoRoomText.text = bl_GameTexts.NoRoomsCreated.Localized("norooms");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void ClearList()
    {
        cachedRoomList.Clear();
        roomListHandler.Clear();
    }
}