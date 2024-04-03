using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_ManipulableUIShell : MonoBehaviour
{
    public bool allowModifySize = true;
    public Vector2 allowedSizeRange = new Vector2(0.5f, 1.7f);
    public bool allowModifyOpacity = true;
    public Vector2 allowedOpacity = new Vector2(0.02f, 1);
}