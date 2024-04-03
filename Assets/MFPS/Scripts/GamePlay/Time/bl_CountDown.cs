using UnityEngine;
using TMPro;

public class bl_CountDown : bl_CountDownBase
{
    public GameObject Content;
    public TextMeshProUGUI CountDownText;
    public AudioClip CountAudio;

    private Animator CountAnim;
    private AudioSource ASource;
    private int countDown = 5;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_PhotonNetwork.AddNetworkCallback(PropertiesKeys.CountdownEvent, OnNetworkEvent);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_PhotonNetwork.RemoveNetworkCallback(OnNetworkEvent);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnNetworkEvent(ExitGames.Client.Photon.Hashtable data)
    {
        OnReceiveCount((int)data["c"]);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void StartCountDown(bool overrideIfStarted = false)
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;
        if (IsCounting && !overrideIfStarted)
        {
            Debug.Log($"Countdown has already started.");
            return;
        }


        countDown = bl_GameData.Instance.CountDownTime;
        bl_MatchTimeManagerBase.Instance.SetTimeState(RoomTimeState.Countdown, true);
        bl_GameManager.Instance.SetGameState(MatchState.Starting);
        IsCounting = true;

        InvokeRepeating(nameof(SetCountDown), 1, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    public override void SetCount(int count)
    {
        OnCountChanged(count);
    }

    /// <summary>
    /// 
    /// </summary>
    void SetCountDown()
    {
        countDown--;
        if (countDown <= 0)
        {
            CancelInvoke(nameof(SetCountDown));
            IsCounting = false;
        }

        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("c", countDown);
        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.CountdownEvent, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    void OnReceiveCount(int count)
    {
        if (!bl_PhotonNetwork.IsMasterClient)
        {
            countDown = count;
        }
        if (countDown <= 0)
        {
            CancelInvoke(nameof(SetCountDown));
            bl_MatchTimeManagerBase.Instance.InitAfterCountdown();
        }
        else
        {
            bl_MatchTimeManagerBase.Instance.SetTimeState(RoomTimeState.Countdown);
        }
        OnCountChanged(countDown);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    private void OnCountChanged(int count)
    {
        if (CountAudio != null)
        {
            if (ASource == null) { ASource = GetComponent<AudioSource>(); }
            ASource.clip = CountAudio;
            ASource.Play();
        }

        CountDownText.text = count.ToString();
        if(count > 0)
        {
            Content.SetActive(true);

            CountAnim = Content.GetComponent<Animator>();
            CountAnim.Play("count", 0, 0);
        }
        else
        {
            Content.SetActive(false);
        }
    }
}