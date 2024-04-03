using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class bl_PlayerUIBank : MonoBehaviour
{
    [Header("REFERENCES")]
    public Canvas PlayerUICanvas;
    public GameObject KillZoneUI;
    public GameObject WeaponStatsUI;
    public GameObject playerStatsUI;
    public GameObject MaxKillsUI;
    public GameObject SpeakerIcon;
    public GameObject TimeUIRoot;
    public Image PlayerStateIcon;
    public Image HealthBar;
    public CanvasGroup DamageAlpha;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI HealthText;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        UpdateUIDisplay();
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateUIDisplay()
    {
        TimeUIRoot.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.Time));
        WeaponStatsUI.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.WeaponData));
        playerStatsUI.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.PlayerStats));
        if (bl_WeaponLoadoutUIBase.Instance != null) bl_WeaponLoadoutUIBase.Instance.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.Loadout));
        bl_EventHandler.DispatchUIMaskChange(bl_UIReferences.Instance.UIMask);
    }
}