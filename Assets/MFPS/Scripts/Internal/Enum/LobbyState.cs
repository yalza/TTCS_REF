using UnityEngine;

namespace MFPS.Internal.Structures
{
    public enum LobbyState
    {
        PlayerName,
        MainMenu,
        Host,
        Join,
        Settings,
        ChangeServer,
        Quit
    }

    public enum LobbyJoinMethod
    {
        DirectToMap = 1,
        WaitingRoom = 2,
    }

    public enum LobbyConnectionState
    {
        Disconnected,
        Connected,
        Connecting,
        Authenticating,
        WaitingForUserName,
        ChangingRegion,
    }
}