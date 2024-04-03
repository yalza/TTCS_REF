using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using MFPS.Runtime.AI;

public class bl_PlayerScoreboardUI : bl_PlayerScoreboardUIBase
{
    #region Public members
    [SerializeField] private TextMeshProUGUI NameText = null;
    [SerializeField] private TextMeshProUGUI KillsText = null;
    [SerializeField] private TextMeshProUGUI DeathsText = null;
    [SerializeField] private TextMeshProUGUI ScoreText = null;
    [SerializeField] private GameObject KickButton = null;
    [SerializeField] private Image LevelIcon = null;
    public TextMeshProUGUI levelNumberText;
    public GameObject localHighlight;
    #endregion

    #region Private members
    private bool isInitializated = false;
    private Team InitTeam = Team.None;
    #endregion

    /// <summary>
    /// Called when the first time that this player appear in the scoreboard
    /// </summary>
    public override void Init(Player player, MFPSBotProperties bot = null)
    {
        Bot = bot;
        isBotBinding = bot != null;

        if (Bot != null)
        {
            UpdateBot();
            return;
        }

        RealPlayer = player;
        gameObject.name = player.NickName + player.ActorNumber;
        InitTeam = player.GetPlayerTeam();
        UpdatePlayerUI(RealPlayer);

#if !LM
        LevelIcon.gameObject.SetActive(false);
#endif
    }

    /// <summary>
    /// Called each time the scoreboard is update (when the scoreboard is open)
    /// </summary>
    public override bool Refresh()
    {
        if (Bot != null || isBotBinding) {  return UpdateBot(); }

        if (RealPlayer == null || RealPlayer.GetPlayerTeam() != InitTeam)
        {
            if (!bl_PlayerScoreboardBase.Instance.RemoveUIBinding(this))
            {
                Destroy();
            }
            return false;
        }

        UpdatePlayerUI(RealPlayer);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdatePlayerUI(Player player)
    {
        NameText.text = player.NickNameAndRole();
        if (!player.CustomProperties.ContainsKey(PropertiesKeys.KillsKey)) return;

        if (localHighlight != null) localHighlight.SetActive(player.IsLocal);
        KillsText.text = player.GetKills().ToString();
        DeathsText.text = player.GetDeaths().ToString();
        ScoreText.text = player.GetPlayerScore().ToString();

        if (bl_GameData.Instance.MasterCanKickPlayers && player.ActorNumber != bl_PhotonNetwork.LocalPlayer.ActorNumber)
        {
            if (KickButton != null) KickButton.SetActive(bl_PhotonNetwork.IsMasterClient);
        }
        else { if (KickButton != null) KickButton.SetActive(false); }

#if LM
        LevelIcon.gameObject.SetActive(true);
        var li = bl_LevelManager.Instance.GetPlayerLevelInfo(RealPlayer);
        LevelIcon.sprite = li.Icon;
        if (levelNumberText != null) levelNumberText.text = li.LevelID.ToString();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override bool UpdateBot()
    {
        if (Bot == null || string.IsNullOrEmpty(Bot.Name) || !bl_AIMananger.Instance.BotsStatistics.Exists(x => x.Name == Bot.Name))
        {
            if (!bl_PlayerScoreboardBase.Instance.RemoveUIBinding(this))
            {
                Destroy();
                return false;
            }
        }

        gameObject.name = Bot.Name;
        if (localHighlight != null) localHighlight.SetActive(false);
        NameText.text = Bot.Name;
        KillsText.text = Bot.Kills.ToString();
        DeathsText.text = Bot.Deaths.ToString();
        ScoreText.text = Bot.Score.ToString();
        InitTeam = Bot.Team;
        if (KickButton != null) KickButton.SetActive(false);

#if LM
        var li = bl_LevelManager.Instance.GetLevel(Bot.Score);
        LevelIcon.sprite = li.Icon;
        if (levelNumberText != null) levelNumberText.text = li.LevelID.ToString();
        LevelIcon.gameObject.SetActive(true);
#endif

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Kick()
    {
        if (bl_PhotonNetwork.IsMasterClient)
        {
            bl_PhotonNetwork.Instance.KickPlayer(RealPlayer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnClick()
    {
        var player = Bot == null ? RealPlayer : null;
        bl_PlayerScoreboardBase.Instance.OnPlayerClicked(player);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        if (RealPlayer == null && !isBotBinding && isInitializated)
        {
            Destroy(gameObject);
            isInitializated = true;
        }
        else if (isBotBinding && Bot == null && isInitializated)
        {
            Destroy(gameObject);
            isInitializated = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Destroy()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetScore()
    {
        if (Bot == null) { return RealPlayer.GetPlayerScore(); }
        else { return Bot.Score; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override Team GetTeam()
    {
        return InitTeam;
    }
}