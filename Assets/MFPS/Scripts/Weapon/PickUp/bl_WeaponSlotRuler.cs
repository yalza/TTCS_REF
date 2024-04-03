using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Slot Ruler", menuName = "MFPS/Loadout/Ruler")]
public class bl_WeaponSlotRuler : ScriptableObject
{
    [Reorderable] public GunType[] primarySlots;
    [Reorderable] public GunType[] secondarySlots;
    [Reorderable] public GunType[] perksSlots;
    [Reorderable] public GunType[] letalSlots;
    public bool CanBeOnSlot(GunType gunType, int slotID)
    {
        if (slotID == 0) return TypeInList(gunType, primarySlots);
        else if(slotID == 1) return TypeInList(gunType, secondarySlots);
        else if (slotID == 2) return TypeInList(gunType, letalSlots);
        else return TypeInList(gunType, perksSlots);
    }

    private bool TypeInList(GunType gunType, GunType[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == gunType) return true;
        }
        return false;
    }
}