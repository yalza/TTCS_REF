using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_MapMods : MonoBehaviour
{
    [LovattoToogle] public bool infinityAmmo = false;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalPlayerSpawn()
    {
        var p = bl_MFPS.LocalPlayerReferences;

        if (infinityAmmo) p.gunManager.SetInfinityAmmoToAllEquippeds(true);
    }
}