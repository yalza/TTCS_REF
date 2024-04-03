using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class bl_MFPSRoomPreview : MonoBehaviour
{

    [Header("References")]
    public Image MapPreview;
    public TextMeshProUGUI MapNameText;

    /// <summary>
    /// 
    /// </summary>
    public void Show(MFPSRoomInfo info)
    {
        var map = info.GetMapInfo();
        MapPreview.sprite = map.Preview;
        MapNameText.text = map.ShowName.ToUpper();
    }
}