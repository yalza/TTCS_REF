using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UGUIMiniMap;

public class bl_MiniMapData : ScriptableObject
{
    public GameObject IconPrefab;
    public GameObject ScreenShotPrefab;
    public bl_MiniMapPlane mapPlane;

    [Separator("MFPS")]
    [LovattoToogle]public bool showEnemysWhenFire = true;
    public bl_MiniMapItem PlayerSetupTemplate;
    public Sprite TeamMateIcon;
    public Color TeammateIconColor = Color.blue;
    public Sprite EnemyIcon;
    public Color EnemyIconColor = Color.red;

    public static bl_MiniMapData _instance;
    public static bl_MiniMapData Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = Resources.Load<bl_MiniMapData>("MiniMapData") as bl_MiniMapData;
            }
            return _instance;
        }
    }
}