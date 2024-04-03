using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Name Plate Style", menuName = "MFPS/UI/Name Plate Style")]
public class bl_NamePlateStyle : ScriptableObject
{
    [Header("Text Style")]
    public GUIStyle style;

    [Header("Icon")]
    public Texture2D IndicatorIcon;
    public float IndicatorIconSize = 15;

    [Header("Health Bar")]
    public Texture2D HealthBarTexture;
    public float HealthBarThickness = 7;
    public Color HealthBackColor = new Color(0, 0, 0, 0.5f);
    public Color HealthBarColor = new Color(0, 1, 0, 0.9f);
    public Vector2 HealthBackOffset = Vector2.zero;

    [Header("Voice")]
    public Texture2D TalkingIcon;
}