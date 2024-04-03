using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;

public class bl_KickVotationUI : MonoBehaviour
{
    [SerializeField] private GameObject Window = null;
    [SerializeField] private TextMeshProUGUI TitleText = null;
    [SerializeField] private TextMeshProUGUI VotingText = null;
    [SerializeField] private TextMeshProUGUI YesText = null;
    [SerializeField] private TextMeshProUGUI NoText = null;
    [SerializeField] private TextMeshProUGUI VoteConfirmation = null;
    [SerializeField] private GameObject KeyInfoUI = null;
    private string CacheTargetName;
    private bl_KickVotation KickManager;
#if LOCALIZATION
    private int[] LocaleTextIDs = new int[] { 54, 55, 110, 111, 56, 52, 53 };
    private string[] LocaleStrings;
#endif

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        Window.SetActive(false);
        KickManager = FindObjectOfType<bl_KickVotation>();
    }

    private void OnEnable()
    {
#if LOCALIZATION
        LocaleStrings = bl_Localization.Instance.GetTextArray(LocaleTextIDs);
        bl_Localization.Instance.OnLanguageChange += OnLangChange;
#endif
        bl_ScoreboardPopUpMenuBase.onScoreboardMenuAction += OnScoreboardPopupAction;
    }

    private void OnDisable()
    {
#if LOCALIZATION
        bl_Localization.Instance.OnLanguageChange -= OnLangChange;
#endif
        bl_ScoreboardPopUpMenuBase.onScoreboardMenuAction -= OnScoreboardPopupAction;
    }

#if LOCALIZATION
    void OnLangChange(Dictionary<string, string> lang)
    {
        LocaleStrings = bl_Localization.Instance.GetTextArray(LocaleTextIDs);
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actionID"></param>
    private void OnScoreboardPopupAction(int actionID)
    {
        if(actionID == 0)
        {
            RequestVotation();
        }
    }

    public void OpenVotatation(Player again, Player By)
    {
        CancelInvoke("Hide");
        CacheTargetName = again.NickName;
#if LOCALIZATION
        TitleText.text = string.Format(LocaleStrings[0], By.NickName);
        VotingText.text = string.Format(LocaleStrings[1], again.NickName);
#else
        TitleText.text = string.Format(bl_GameTexts.VoteBy, By.NickName);
        VotingText.text = string.Format(bl_GameTexts.KickQuestion, again.NickName);
#endif
        YesText.text = "0";
        NoText.text = "0";
        VoteConfirmation.text = string.Empty;
        if (again.ActorNumber != bl_PhotonNetwork.LocalPlayer.ActorNumber)
        {
            KeyInfoUI.SetActive(true);
        }
        else
        {
            KeyInfoUI.SetActive(false);
        }
        Window.SetActive(true);
    }

    public void OnSendLocalVote(bool yes)
    {
        KeyInfoUI.SetActive(false);
#if LOCALIZATION
        string vote = yes ? "<color=green>" + LocaleStrings[2] +"</color>" : "<color=red>" + LocaleStrings[3] + "</color>";
        VoteConfirmation.text = string.Format(LocaleStrings[4], vote);
#else
        string vote = yes ? "<color=green>YES</color>" : "<color=red>NO</color>";
        VoteConfirmation.text = string.Format(bl_GameTexts.YouVote, vote);
#endif
    }

    public void OnReceiveVote(int yes, int no)
    {
        YesText.text = yes.ToString();
        NoText.text = no.ToString();
    }

    public void OnFinish(bool yes)
    {
#if LOCALIZATION
        if (yes)
        {
            VotingText.text = string.Format(LocaleStrings[5], CacheTargetName);
        }
        else
        {
            VotingText.text = string.Format(LocaleStrings[6], CacheTargetName);
        }
#else
        if (yes)
        {
            VotingText.text = string.Format(bl_GameTexts.KickConfirmation, CacheTargetName);
        }
        else
        {
            VotingText.text = string.Format(bl_GameTexts.KickFailed, CacheTargetName);
        }
#endif
        KeyInfoUI.SetActive(false);
        Invoke("Hide", 3);
    }

    /// <summary>
    /// 
    /// </summary>
    public void RequestVotation()
    {
        KickManager.RequestKick(bl_ScoreboardPopUpMenuBase.TargetPlayer);
    }

    void Hide()
    {
        Window.SetActive(false);
    }
}