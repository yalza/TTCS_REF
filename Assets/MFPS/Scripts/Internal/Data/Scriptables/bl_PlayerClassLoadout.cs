using UnityEngine;

[DisallowMultipleComponent, CreateAssetMenu(fileName = "Player Class Loadout", menuName = "MFPS/Player Class/Loadout")]
public class bl_PlayerClassLoadout : ScriptableObject
{
    [GunID] public int Primary = 0;
    [GunID] public int Secondary = 1;
    [GunID] public int Perks = 2;
    [GunID] public int Letal = 3;

    public void FromString(string str)
    {
        string[] split = str.Split('&');
        Primary = int.Parse(split[0]);
        Secondary = int.Parse(split[1]);
        Perks = int.Parse(split[2]);
        Letal = int.Parse(split[3]);
    }

    public void FromString(string str, int slot)
    {
        string[] loadouts = str.Split(',');
        string[] split = loadouts[slot].Split('&');
        Primary = int.Parse(split[0]);
        Secondary = int.Parse(split[1]);
        Perks = int.Parse(split[2]);
        Letal = int.Parse(split[3]);
    }

    public bl_GunInfo GetPrimaryGunInfo() => bl_GameData.Instance.GetWeapon(Primary);
    public bl_GunInfo GetSecondaryGunInfo() => bl_GameData.Instance.GetWeapon(Secondary);
    public bl_GunInfo GetPerksGunInfo() => bl_GameData.Instance.GetWeapon(Perks);
    public bl_GunInfo GetLetalGunInfo() => bl_GameData.Instance.GetWeapon(Letal);
    public override string ToString() => $"{Primary}&{Secondary}&{Perks}&{Letal}";
}