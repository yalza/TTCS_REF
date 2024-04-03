using UnityEngine;

/// <summary>
/// Inherited classes should take care of display to the player the information of the weapon
/// that they are about to pick up.
/// </summary>
public abstract class bl_PickUpUIBase : MonoBehaviour
{
    /// <summary>
    /// Called when the player enter in the trigger or focus a gun pick up in the map
    /// </summary>
    public abstract void OnOverWeapon(bl_GunPickUpBase toPickUpWeapon);

    /// <summary>
    /// 
    /// </summary>
    public abstract void Show(DisplayInformation information);

    /// <summary>
    /// 
    /// </summary>
    public abstract void Hide();

    public struct DisplayInformation
    {
        public string Text;
        public Sprite Icon;
        public string InputName;
    }

    private static bl_PickUpUIBase _instance;
    public static bl_PickUpUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_PickUpUIBase>(); }
            return _instance;
        }
    }
}