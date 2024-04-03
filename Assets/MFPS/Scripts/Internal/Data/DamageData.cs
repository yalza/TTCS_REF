using UnityEngine;
using Photon.Realtime;

/// <summary>
/// MFPS class with all the Damage information
/// </summary>
public class DamageData
{
    /// <summary>
    /// The amount of damage to apply to the IDamageable object
    /// </summary>
    public int Damage = 10;

    /// <summary>
    /// The cached name of the actor of this damage
    /// Since the damage can't always comes from a <see cref="MFPSActor"/> (player or bot)
    /// This the alternative way to get the name of the actor.
    /// </summary>
    public string From;

    /// <summary>
    /// Cause of the damage
    /// </summary>
    public DamageCause Cause = DamageCause.Player;

    /// <summary>
    /// The position from where this damage origins (bullet origin, explosion origin, etc...)
    /// </summary>
    public Vector3 Direction 
    { 
        get; 
        set; 
    } = Vector3.zero;

    /// <summary>
    /// Is this damage comes from a head shot?
    /// </summary>
    public bool isHeadShot 
    { 
        get; 
        set; 
    } = false;

    /// <summary>
    /// The GunID of the weapon that cause this damage in case it was origin from a weapon
    /// if the damage was not from a weapon you can skip it.
    /// </summary>
    public int GunID 
    { 
        get; 
        set; 
    } = 0;

    /// <summary>
    /// The network player from which this damage
    /// </summary>
    public Player Actor 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// The MFPS Actor of this damage
    /// Can be a real player or bot
    /// </summary>
    public MFPSPlayer MFPSActor 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// The cached network view id of the actor of this damage
    /// </summary>
    public int ActorViewID 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// Create a hashtable with the this damage data
    /// </summary>
    /// <returns></returns>
    public ExitGames.Client.Photon.Hashtable GetAsHashtable()
    {
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("d", Damage);
        data.Add("gi", GunID);
        data.Add("vi", ActorViewID);
        data.Add("c", Cause);
        data.Add("f", From);
        if (Direction != Vector3.zero)
            data.Add("dr", Direction);

        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    public DamageData(ExitGames.Client.Photon.Hashtable data)
    {
        Damage = (int)data["d"];
        GunID = (int)data["gi"];
        ActorViewID = (int)data["vi"];
        Cause = (DamageCause)data["c"];
        From = (string)data["f"];
        if (data.ContainsKey("dr")) Direction = (Vector3)data["dr"];

        MFPSActor = bl_GameManager.Instance.GetMFPSActor(ActorViewID);
    }

    /// <summary>
    /// 
    /// </summary>
    public DamageData() { }
}

/// <summary>
/// 
/// </summary>
public struct MFPSHitData
{
    /// <summary>
    /// The name of the object who was hit
    /// </summary>
    public string HitName;

    /// <summary>
    /// The amount of damage applied to the hit object
    /// </summary>
    public int Damage;

    /// <summary>
    /// The point of the hit collision
    /// </summary>
    public Vector3 HitPosition;

    /// <summary>
    /// The transform reference of the hit object
    /// A null check is required before access this since is can be destroyed
    /// </summary>
    public Transform HitTransform;
}