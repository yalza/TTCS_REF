using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class bl_WaitingPlayerUI : bl_WaitingPlayerUIBase
{

    public TextMeshProUGUI NameText;
    public Image LevelImg;
    public Image TeamColorImg;
    public GameObject ReadyUI;
    public GameObject KickButton;
    public GameObject MasterClientUI;
    private Player ThisPlayer;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override Player GetPlayer()
    {
        return ThisPlayer;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    public override void SetInfo(Player player)
    {
        ThisPlayer = player;
        NameText.text = string.Format(player.NickNameAndRole());
        TeamColorImg.color = player.GetPlayerTeam().GetTeamColor();
        MasterClientUI.SetActive(player.IsMasterClient);
        if(bl_GameData.Instance.MasterCanKickPlayers && player.ActorNumber != bl_PhotonNetwork.LocalPlayer.ActorNumber)
        {
            KickButton.SetActive(bl_PhotonNetwork.IsMasterClient);
        }
        else { KickButton.SetActive(false); }
        UpdateState();

#if LM
        LevelImg.gameObject.SetActive(true);
        LevelImg.sprite = bl_LevelManager.Instance.GetPlayerLevelInfo(player).Icon;
#else
        LevelImg.gameObject.SetActive(false);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateState()
    {
        ReadyUI.SetActive(bl_WaitingRoomBase.Instance.IsPlayerReady(ThisPlayer));
    }

    /// <summary>
    /// 
    /// </summary>
    public void KickThis()
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.CloseConnection(ThisPlayer);
    }
}