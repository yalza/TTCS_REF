using UnityEngine;

public abstract class bl_GunPickUpManagerBase : bl_PhotonHelper
{
    /// <summary>
    /// 
    /// </summary>
    public struct ThrowData
    {
        public int GunID;
        public Vector3 Origin;
        public Vector3 Direction;
        public int[] Data;
        public bool AutoDestroy;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct PickUpData
    {
        public string Identifier;
        public int GunID;
        public bl_GunPickUpBase.AmmoInformation Ammunition;
    }

    public bl_GunPickUpBase LastTrigger { get; set; }

    /// <summary>
    /// Throw a weapon on all clients
    /// </summary>
    public abstract void ThrowGun(ThrowData throwData);

    /// <summary>
    /// Send a call to all the room clients to let them know that player will pick up a weapon
    /// </summary>
    public abstract void SendPickUp(PickUpData pickUpData);

    /// <summary>
    /// 
    /// </summary>
    private static bl_GunPickUpManagerBase _instance;
    public static bl_GunPickUpManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_GunPickUpManagerBase>(); }
            return _instance;
        }
    }
}