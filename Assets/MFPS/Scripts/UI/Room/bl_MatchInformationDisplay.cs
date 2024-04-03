using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class bl_MatchInformationDisplay : MonoBehaviour
{
    [Header("Settings")]
    public float Delay = 1.2f;
    public float VisibleTime = 3.5f;
    public float FadeDuration = 1;
    public string fakeDate = "DAY 21 10:25:36";
    [Header("References")]
    public CanvasGroup RootAlpha;
    public TextMeshProUGUI MapNameText;
    public TextMeshProUGUI DateText;
    public TextMeshProUGUI GameModeText;
    public TextMeshProUGUI TeamText;

    /// <summary>
    /// 
    /// </summary>
    public void DisplayInfo()
    {
        MFPSRoomInfo props = PhotonNetwork.CurrentRoom.GetRoomInfo();
        MapNameText.text = props.mapName.ToUpper();
        DateText.text = fakeDate;
        GameModeText.text = props.gameMode.GetName().ToUpper();
        TeamText.text = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam().GetTeamName().ToUpper();
        StartCoroutine(DoDisplay());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DoDisplay()
    {
        yield return new WaitForSeconds(Delay);
        RootAlpha.gameObject.SetActive(true);
        float d = 0;
        while(d < 1)
        {
            d += Time.deltaTime / FadeDuration;
            RootAlpha.alpha = d;
            yield return null;
        }
        yield return new WaitForSeconds(VisibleTime);
        while (d > 0)
        {
            d -= Time.deltaTime / FadeDuration;
            RootAlpha.alpha = d;
            yield return null;
        }
        RootAlpha.gameObject.SetActive(false);
    }

    private static bl_MatchInformationDisplay _instance;
    public static bl_MatchInformationDisplay Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_MatchInformationDisplay>(); }
            return _instance;
        }
    }
}