using UnityEngine;
using MFPS.Internal.Interfaces;
using TMPro;
using MFPS.Runtime.AI;

/// <summary>
/// Default MFPS after match resume screen
/// If you want to use your custom resume screen, simply append the IMFPSResumeScreen interface to your script
/// and handle the inherited functions.
/// </summary>
public class bl_GameFinish : bl_PhotonHelper, IMFPSResumeScreen
{

    [SerializeField] private TextMeshProUGUI PlayerNameText = null;
    [SerializeField] private TextMeshProUGUI KillsText = null;
    [SerializeField] private TextMeshProUGUI DeathsText = null;
    [SerializeField] private TextMeshProUGUI ScoreText = null;
    [SerializeField] private TextMeshProUGUI KDRText = null;
    [SerializeField] private TextMeshProUGUI TimePlayedText = null;
    [SerializeField] private TextMeshProUGUI WinScoreText = null;
    [SerializeField] private TextMeshProUGUI HeadshotsText = null;
    [SerializeField] private TextMeshProUGUI TotalScoreText = null;
    [SerializeField] private TextMeshProUGUI CoinsText = null;
    [SerializeField] private GameObject Content = null;

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_UIReferences.Instance.ResumeScreen = this;
    }

    /// <summary>
    /// 
    /// </summary>
    public void CollectData()
    {       
        int kills = bl_PhotonNetwork.LocalPlayer.GetKills();

        // if bots eliminations doesn't count for the player stats
        if (bl_GameData.Instance.howConsiderBotsEliminations != BotKillConsideration.SameAsRealPlayers)
        {
            if (bl_RoomSettings.TryGetMatchPersistData("bot-kills", out var value))
            {
                int bk = value is int ? (int)value : 0;
                kills = Mathf.Min(0, kills - bk);
            }
        }

        int deaths = bl_PhotonNetwork.LocalPlayer.GetDeaths();
        int score = bl_PhotonNetwork.LocalPlayer.GetPlayerScore();
        int kd = kills;
        if (kills <= 0) { kd = -deaths; }
        else if (deaths > 0) { kd = kills / deaths; }
        int timePlayed = Mathf.RoundToInt(bl_GameManager.Instance.PlayedTime);
        int scorePerTime = timePlayed * bl_GameData.Instance.ScoreReward.ScorePerTimePlayed;
        int hsscore = bl_GameManager.Instance.Headshots * bl_GameData.Instance.ScoreReward.ScorePerHeadShot;
        bool winner = bl_GameManager.Instance.isLocalPlayerWinner();
        int winScore = (winner) ? bl_GameData.Instance.ScoreReward.ScoreForWinMatch : 0;
        PlayerNameText.text = bl_PhotonNetwork.NickName;
        int tscore = score + winScore + scorePerTime;

        int coins = 0;
        if (tscore > 0 && bl_GameData.Instance.VirtualCoins.CoinScoreValue > 0 && tscore > bl_GameData.Instance.VirtualCoins.CoinScoreValue)
        {
            coins = tscore / bl_GameData.Instance.VirtualCoins.CoinScoreValue;
        }
        KillsText.text = string.Format("{0}: <b>{1}</b>", bl_GameTexts.Kills.Localized(126).ToUpper(), kills);
        DeathsText.text = string.Format("{0}: <b>{1}</b>", bl_GameTexts.Deaths.Localized(58, true).ToUpper(), deaths);
        ScoreText.text = string.Format("{0}: <b>{1}</b>", bl_GameTexts.Score.Localized(59).ToUpper(), score - hsscore);
        WinScoreText.text = string.Format(bl_GameTexts.WinMatch.Localized(61), winScore);
        KDRText.text = string.Format("{0}\n<size=10>KDR</size>", kd);
        TimePlayedText.text = string.Format("{0} <b>{1}</b> +{2}", bl_GameTexts.TimePlayed.Localized(60).ToUpper(), bl_StringUtility.GetTimeFormat((float)timePlayed / 60, timePlayed), scorePerTime);
        HeadshotsText.text = string.Format("{0} <b>{1}</b> +{2}", bl_GameTexts.HeadShot.Localized(16, true).ToUpper(), bl_GameManager.Instance.Headshots, hsscore);
        TotalScoreText.text = string.Format("{0}\n<size=9>{1}</size>", tscore, bl_GameTexts.TotalScore.Localized(35).ToUpper());
        CoinsText.text = string.Format("+{0}\n<size=9>COINS</size>", coins);

#if ULSP
        if (bl_DataBase.Instance != null)
        {
            var p = bl_PhotonNetwork.LocalPlayer;
            bl_ULoginMFPS.SaveLocalPlayerKDS();
            bl_DataBase.Instance.StopAndSaveTime();
            if (coins > 0)
            {
                bl_DataBase.Instance.SaveNewCoins(coins, bl_GameData.Instance.VirtualCoins.XPCoin);
            }
#if CLANS
            bl_DataBase.Instance.SetClanScore(score);
#endif
        }
#else
        if (coins > 0)
        {
            bl_GameData.Instance.VirtualCoins.AddCoins(coins, bl_PhotonNetwork.NickName);
        }
        int oldsavedScore = PlayerPrefs.GetInt(PropertiesKeys.GetUniqueKeyForPlayer("score", LocalName), 0);
        oldsavedScore += tscore;
        PlayerPrefs.SetInt(PropertiesKeys.GetUniqueKeyForPlayer("score", LocalName), oldsavedScore);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void Show()
    {
        Content.SetActive(true);
        Invoke(nameof(GoToLobby), 60);//maximum time out to leave.
    }

    /// <summary>
    /// 
    /// </summary>
    public void GoToLobby()
    {
        CancelInvoke();
        if (bl_PhotonNetwork.IsConnected && bl_PhotonNetwork.InRoom)
        {
            bl_PhotonNetwork.LeaveRoom();
        }
        else
        {
            bl_UtilityHelper.LoadLevel(bl_GameData.Instance.MainMenuScene);
        }
    }
}