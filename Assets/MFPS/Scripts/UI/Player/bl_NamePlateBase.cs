using UnityEngine;

/// <summary>
/// Base class for Name Plate Drawer
/// Inherited from this script your custom script where you can handle how to draw the players name in game.
/// </summary>
public abstract class bl_NamePlateBase : bl_MonoBehaviour
{

    public static bool BlockDraw = false;

    /// <summary>
    /// 
    /// </summary>
    public string PlayerName { get; set; }

    /// <summary>
    /// Set the name to draw
    /// </summary>
    public abstract void SetName(string playerName);

    /// <summary>
    /// 
    /// </summary>
    public abstract void SetActive(bool active);
}
