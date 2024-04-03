using System;
using UnityEngine;
using MFPS.Internal.Structures;
using MFPSEditor;

[Serializable]
public class bl_GunInfo
{
    [Header("Info")]
    public string Name;
    public GunType Type = GunType.Machinegun;
    [LovattoToogle] public bool Active = true;

    [Header("Settings")]
    [Range(1, 100)] public int Damage;
    [Range(0.01f, 2f)] public float FireRate = 0.1f;
    [Range(0.5f, 10)] public float ReloadTime = 2.5f;
    [Range(0, 1000)] public int Range;
    [Range(0.01f, 5)] public int Accuracy;
    [Range(0, 4)] public float Weight;
    public MFPSItemUnlockability Unlockability;

    [Header("References")]
    public bl_GunPickUpBase PickUpPrefab;
    [SpritePreview(30, true)] public Sprite GunIcon;

    /// <summary>
    /// Can show this weapons in the game lists like class customizer, customizer, unlocks, etc...
    /// </summary>
    /// <returns></returns>
    public bool CanShowWeapon()
    {
        return Active && Unlockability.UnlockMethod != MFPSItemUnlockability.UnlockabilityMethod.Hidden;
    }
}