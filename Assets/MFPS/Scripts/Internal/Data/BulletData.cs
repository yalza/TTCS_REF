using UnityEngine;

/// <summary>
/// Contains all the information about an instanced bullet
/// </summary>
public class BulletData 
{

    /// <summary>
    /// The name of the weapon from which this bullet was fired.
    /// </summary>
    public string WeaponName;

    /// <summary>
    /// The base damage that this bullet will cause
    /// </summary>
    public float Damage;

    /// <summary>
    /// The Position from where this bullet was fired.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The amount of force to applied to the hit object in case this have a RigidBody
    /// </summary>
    public float ImpactForce;

    /// <summary>
    /// The bullet inaccuracy to apply to the projected direction
    /// </summary>
    public Vector3 Inaccuracity = Vector3.zero;

    /// <summary>
    /// Bullet Speed
    /// </summary>
    public float Speed;

    /// <summary>
    /// The max distance that this bullet can travel without hit anything.
    /// </summary>
    public float Range;

    /// <summary>
    /// The amount of bullet drop
    /// </summary>
    public float DropFactor;

    /// <summary>
    /// GunID of the weapon from which this bullet was fire
    /// </summary>
    public int WeaponID;

    /// <summary>
    /// Was this bullet created by a remote player?
    /// </summary>
    public bool isNetwork;

    /// <summary>
    /// Was this bullet created by the actual local player
    /// The difference between this and <see cref="isNetwork"/>
    /// Is that IsNetwork can be False (meaning is not a remote bullet) when a Bot created the bullet
    /// But this ensure that the bullet was created by the real local player.
    /// </summary>
    public bool IsLocalPlayer;

    /// <summary>
    /// The Cached Network View
    /// </summary>
    public int ActorViewID 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// The MFPS Actor who create this bullet
    /// </summary>
    public MFPSPlayer MFPSActor 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// Create the bullet data and fetch info from the <see cref="DamageData"/>
    /// </summary>
    /// <param name="data"></param>
    public BulletData(DamageData data)
    {
        MFPSActor = data.MFPSActor;
        Damage = data.Damage;
        ActorViewID = data.ActorViewID;
        WeaponID = data.GunID;
        Position = data.Direction;
    }

    /// <summary>
    /// Calculate and assign the projectile inaccuracy vector
    /// </summary>
    /// <param name="spreadBase"></param>
    /// <param name="maxSpread"></param>
    /// <returns></returns>
    public BulletData SetInaccuracity(float spreadBase, float maxSpread)
    {
        Inaccuracity = new Vector3(Random.Range(-maxSpread, maxSpread) * spreadBase, Random.Range(-maxSpread, maxSpread) * spreadBase, 1);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    public BulletData()
    {
    }
}