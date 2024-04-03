using System.Collections.Generic;
using UnityEngine;
using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class bl_PhotonNetworkStats : MonoBehaviour, IConnectionCallbacks
{
    [SerializeField, Range(1, 5)] private int UpdateRate = 5;
    public LogType m_LogType = LogType.Resume;
    public bool LogTrafficStats;
    [Header("GUI")]
    [SerializeField] Rect m_GUIArea = new Rect(25, 10, 300, 500);
    [SerializeField] private Texture2D ArrowIcon;
    [SerializeField] private Texture2D ArrowIconUp;
    [SerializeField] private GUIStyle m_Style;

    private int DownCommandsStimated = 0;
    private int UpCommandsStimated = 0;
    private int DownPackagesStimated = 0;
    private int UpPackagesStimated = 0;

    public List<StatInfo> ValuesList = new List<StatInfo>();
    public List<StatInfo> LastValuesList = new List<StatInfo>();
    private bool isInitialized = false;

    /// <summary>
    /// 
    /// </summary>
    IEnumerator Start()
    {
        while (!bl_GameData.isDataCached) { yield return null; }
        if (!bl_GameData.Instance.ShowNetworkStats) { Destroy(gameObject); yield break; }

        PhotonNetwork.AddCallbackTarget(this);
        bl_PhotonNetworkStats pns = FindObjectOfType<bl_PhotonNetworkStats>();
        if (pns != null && pns != this)
        {
            Destroy(gameObject);
            yield break;
        }

        DontDestroyOnLoad(gameObject);
        isInitialized = true;
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnApplicationQuit()
    {
        this.CancelInvoke();
    }

    public void LogStats()
    {
        if (this.LogTrafficStats)
        {
            GetValues();
        }
    }

    void GetValues()
    {
        ValuesList.Clear();
        string info = PhotonNetwork.NetworkingClient.LoadBalancingPeer.VitalStatsToString(true);
        string[] variables = info.Split(":"[0]);
        StatInfo stat = new StatInfo();
        for (int i = 0; i < variables.Length; i++)
        {
            string v = variables[i];
            v = v.Remove(0, 1);
            v = v.Split(null)[0];
            if (i != 0)
            {
                string vari = variables[i - 1];
                if (i > 1)
                {
                    vari = vari.Remove(0, ValuesList[ValuesList.Count - 1].Value.Length + 2);
                }

                stat = new StatInfo();
                stat.Variable = vari;
                stat.Value = v;
                ValuesList.Add(stat);
            }
        }
        TrafficStatsGameLevel gls = PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsGameLevel;
        stat = new StatInfo();
        stat.Variable = "Out Calls";
        stat.Value = gls.TotalOutgoingMessageCount.ToString();
        ValuesList.Add(stat);

        stat = new StatInfo();
        stat.Variable = "In Calls";
        stat.Value = gls.TotalIncomingMessageCount.ToString();
        ValuesList.Add(stat);

        TrackInfo();
    }

    void TrackInfo()
    {
        if (LastValuesList.Count > 0)
        {
            int ncos = ValuesList[18].GetIntValue;
            DownCommandsStimated = (ncos - LastValuesList[18].GetIntValue) / UpdateRate;

            int dsc = ValuesList[17].GetIntValue;
            UpCommandsStimated = (dsc - LastValuesList[17].GetIntValue) / UpdateRate;

            DownPackagesStimated = (ValuesList[11].GetIntValue - LastValuesList[11].GetIntValue) / UpdateRate;
            UpPackagesStimated = (ValuesList[15].GetIntValue - LastValuesList[15].GetIntValue) / UpdateRate;
        }
        LastValuesList.Clear();
        LastValuesList.AddRange(ValuesList.ToArray());
    }

    private void OnGUI()
    {
        if (!isInitialized) return;
        if (ValuesList.Count <= 0)
            return;

        if (m_LogType == LogType.Full)
        {
            GUILayout.BeginArea(m_GUIArea);
            GUILayout.BeginVertical();
            for (int i = 0; i < ValuesList.Count; i++)
            {
                GUILayout.Label(ValuesList[i].Variable + ": " + ValuesList[i].Value, m_Style);
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        else
        {
            GUILayout.BeginArea(m_GUIArea);
            GUILayout.Label(string.Format("Ping: {0}", ValuesList[0].Value), m_Style);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(ArrowIcon, GUILayout.Height(15), GUILayout.Width(15));
            GUILayout.BeginVertical();
            GUILayout.Label(string.Format("{0}", ValuesList[9].GetByteSize()), m_Style);
            GUILayout.Label(string.Format("{0}({1}) Packages", ValuesList[11].GetIntValue, DownPackagesStimated), m_Style);
            GUILayout.Label(string.Format("{0}({1}) Calls", ValuesList[18].GetIntValue, DownCommandsStimated), m_Style);
            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.Label(ArrowIconUp, GUILayout.Height(15), GUILayout.Width(15));
            GUILayout.BeginVertical();
            GUILayout.Label(string.Format("{0}", ValuesList[13].GetByteSize()), m_Style);
            GUILayout.Label(string.Format("{0}({1}) Packages", ValuesList[15].GetIntValue, UpPackagesStimated), m_Style);
            GUILayout.Label(string.Format("{0}({1}) Calls", ValuesList[17].GetIntValue, UpCommandsStimated), m_Style);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }

    static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
    static string SizeSuffix(int value, int decimalPlaces = 1)
    {
        if (decimalPlaces < 0) { Debug.LogError("decimalPlaces"); }
        if (value < 0) { return "-" + SizeSuffix(-value); }
        if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

        // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
        int mag = (int)Mathf.Log(value, 1024);

        // 1L << (mag * 10) == 2 ^ (10 * mag) 
        // [i.e. the number of bytes in the unit corresponding to mag]
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        // make adjustment when the value is large enough that
        // it would round up to 1000 or more
        if (System.Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }

        return string.Format("{0:n" + decimalPlaces + "} {1}",
            adjustedSize,
            SizeSuffixes[mag]);
    }

    public void OnConnected()
    {
        if (LogTrafficStats)
        {
            PhotonNetwork.NetworkStatisticsEnabled = true;
            this.InvokeRepeating("LogStats", UpdateRate, UpdateRate);
        }
    }

    public void OnConnectedToMaster()
    {

    }

    public void OnDisconnected(DisconnectCause cause)
    {
        this.CancelInvoke();
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {

    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {

    }

    [System.Serializable]
    public class StatInfo
    {
        public string Variable;
        public string Value;

        public int GetIntValue
        {
            get
            {
                return int.Parse(Value.Replace(".", ""));
            }
        }

        public string GetByteSize()
        {
            return SizeSuffix(GetIntValue, 2);
        }
    }

    [System.Serializable]
    public enum LogType
    {
        Full,
        Resume,
    }
}