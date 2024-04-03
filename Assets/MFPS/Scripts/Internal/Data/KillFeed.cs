using UnityEngine;

namespace MFPS.Internal.Structures
{
    public class KillFeed
    {
        public string Killer;
        public string Killed;
        public string Message;
        public bool HeadShot;
        public int GunID;
        public Team KillerTeam;
        public Color KillerColor;
        public Color KilledColor;
        public float TimeToShow;
        public KillFeedMessageType messageType;
    }

    public enum KillFeedMessageType
    {
        WeaponKillEvent = 0,
        TeamHighlightMessage = 1,
        Message = 2,
    }

    public enum KillFeedWeaponShowMode
    {
        WeaponName = 0,
        WeaponIcon = 1
    }

    /// <summary>
    /// 
    /// </summary>
    public struct MFPSLocalNotification
    {
        public string Message { get; private set; }

        public MFPSLocalNotification(string message)
        {
            Message = message;
            bl_EventHandler.onLocalNotification?.Invoke(this);
        }
    }
}