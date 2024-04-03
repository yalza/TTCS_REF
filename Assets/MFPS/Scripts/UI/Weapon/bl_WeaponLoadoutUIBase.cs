using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class bl_WeaponLoadoutUIBase : MonoBehaviour
{

    [Serializable]
    public enum VisibilityMode
    {
        AutoHide,
        AlwaysShow,
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum LoadoutFlags : byte
    {
        None = 0,
        Animated = 1
    }

    /// <summary>
    /// Show the given weapons in the slots UI
    /// </summary>
    /// <param name="weapons"></param>
    public abstract void SetLoadout(List<bl_Gun> weapons);

    /// <summary>
    /// Mark the given slot as the current equipped weapon
    /// </summary>
    /// <param name="slotID">Slot to be marked as equipped.</param>
    public abstract void SetEquippedSlot(int slotID);

    /// <summary>
    /// Replace the given slot UI with the given weapon info.
    /// </summary>
    /// <param name="slot">Slot to be replace.</param>
    /// <param name="newGun">Weapon to fetch the information from.</param>
    public abstract void ReplaceSlot(int slotID, bl_Gun newWeapon);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="autohide"></param>
    public abstract void SetVisibility(VisibilityMode visibility);

    /// <summary>
    /// Empty the weapon slots UI
    /// Since the slots are not auto updated if a weapon is unequipped, this have to be called manually.
    /// </summary>
    public abstract void ClearSlots(LoadoutFlags flags = LoadoutFlags.None);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract VisibilityMode GetVisibilityMode();

    /// <summary>
    /// 
    /// </summary>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    private static bl_WeaponLoadoutUIBase _instance;
    public static bl_WeaponLoadoutUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_WeaponLoadoutUIBase>(); }
            if(_instance == null) { _instance = bl_UIReferences.Instance.GetComponentInChildren<bl_WeaponLoadoutUIBase>(true); }
            return _instance;
        }
    }
}
