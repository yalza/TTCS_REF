using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using MFPS.Audio;
using MFPS.Runtime.UI;
using MFPS.Runtime.FriendList;
using TMPro;
using UnityEditor;
using MFPS.Runtime.Settings;

public class bl_LobbyUI : MonoBehaviour
{
    public string MainWindowName = "room list";
    public List<WindowUI> windows = new List<WindowUI>();
    public List<PopUpWindows> popUpWindows = new List<PopUpWindows>();

    [Header("References")]
    public CanvasGroup FadeAlpha;
    public GameObject PhotonStaticticsUI;
    public GameObject PhotonGamePrefab;
    public GameObject EnterPasswordUI;
    public GameObject SeekingMatchUI;
    public TextMeshProUGUI PlayerNameText = null;
    [SerializeField] private TextMeshProUGUI PasswordLogText = null;

    public Image LevelIcon;
    public TMP_InputField PlayerNameField = null;
    [SerializeField] private TMP_Dropdown ServersDropdown = null;
    public bl_FriendListUI FriendUI;
    public GameObject[] AddonsButtons;
    public LevelUI m_LevelUI;
    public GameObject overAllEmptyLoading;
    public bl_ConfirmationWindow confirmationWindow;
    public bl_ConfirmationWindow messageWindow;
    public bl_ConfirmationWindow connectionPopupMessage;
    public bl_CanvasGroupFader blackScreenFader;

    #region Private members
    private string currentWindow = "";
    private int requestedRegionID = -1;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public void InitialSetup()
    {
        bl_EventHandler.onGameSettingsChange += ApplyRuntimeSettings;
#if LOCALIZATION
        bl_Localization.Instance.OnLanguageChange += OnLanguageChange;
#endif
        windows.ForEach(x => { if (x.UIRoot != null) { x.UIRoot.SetActive(false); } });//disable all windows
        if (PhotonStaticticsUI != null) { PhotonStaticticsUI.SetActive(bl_Lobby.Instance.ShowPhotonStatistics); }
        blackScreenFader.SetAlpha(1);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
#if ULSP
        if (bl_DataBase.IsUserLogged) bl_DataBase.OnUpdateData += OnUpdateDataBaseInfo;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.onGameSettingsChange -= ApplyRuntimeSettings;
#if LOCALIZATION
        bl_Localization.Instance.OnLanguageChange -= OnLanguageChange;
#endif
#if ULSP
        if (bl_DataBase.IsUserLogged) bl_DataBase.OnUpdateData -= OnUpdateDataBaseInfo;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeWindow(string window)
    {
        if (window == currentWindow) return;//return if we are trying to open the opened window
        WindowUI w = windows.Find(x => x.Name == window);
        if (w == null) return;//the window with that windowName doesn't exist

        StopCoroutine("DoChangeWindow");
        StartCoroutine("DoChangeWindow", w);
        currentWindow = window;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetEnableWindow(string windowName, bool active)
    {
        WindowUI w = windows.Find(x => x.Name == windowName);
        if (w == null || w.UIRoot == null) return;//the window with that windowName doesn't exist

        w.UIRoot.SetActive(active);
        if (w.MenuButton != null) w.MenuButton.interactable = !active;
    }

    public void Home() { ChangeWindow(MainWindowName); bl_Lobby.Instance.onShowMenu?.Invoke(); }
    public void HideAll() { currentWindow = ""; windows.ForEach(x => { if (x.UIRoot != null) { x.UIRoot.SetActive(false); } }); }//disable all windows 

    /// <summary>
    /// 
    /// </summary>
    IEnumerator DoChangeWindow(WindowUI window)
    {
        //now change the windows
        for (int i = 0; i < windows.Count; i++)
        {
            WindowUI w = windows[i];
            if (w.UIRoot == null) continue;

            if (w.isPersistent)
            {
                w.UIRoot.SetActive(!window.hidePersistents);
            }
            else
            {
                if (w.MenuButton != null) w.MenuButton.interactable = true;
                w.UIRoot.SetActive(false);
            }
            window.UIRoot.SetActive(true);
            if (window.MenuButton != null) window.MenuButton.interactable = false;
            if (window.showTopMenu)
            {
                SetEnableWindow("top menu", true);
            }
        }

        if (window.playFadeInOut)
        {
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime * 2;
                FadeAlpha.alpha = d;
                yield return null;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowPopUpWindow(string popUpName)
    {
        PopUpWindows w = popUpWindows.Find(x => x.Name == popUpName);
        if (w == null || w.Window == null) return;

        w.Window.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {
        if (bl_PhotonNetwork.IsConnected && bl_GameData.isDataCached)
        {
            PlayerNameText.text = bl_MFPS.LocalPlayer.FullNickName();
        }
    }

    public void SignOut()
    {
        bl_Lobby.Instance.SignOut();
    }

    #region Settings

    /// <summary>
    /// 
    /// </summary>
    public void Setup()
    {
        LoadSettings();
        FullSetUp();
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadSettings()
    {
        ApplyRuntimeSettings();

        bool km = PlayerPrefs.GetInt(PropertiesKeys.KickKey, 0) == 1;
        if (km) { ShowPopUpWindow("kicked"); }
        if (bl_PhotonNetwork.Instance.hasPingKick) { ShowPopUpWindow("ping kick"); bl_PhotonNetwork.Instance.hasPingKick = false; }
        PlayerPrefs.SetInt(PropertiesKeys.KickKey, 0);
        bl_Lobby.Instance.rememberMe = !string.IsNullOrEmpty(PlayerPrefs.GetString(PropertiesKeys.GetUniqueKey("remembernick"), string.Empty));
        if (bl_PhotonNetwork.Instance.hasAFKKick) { ShowPopUpWindow("afk kick"); bl_PhotonNetwork.Instance.hasAFKKick = false; }

        bl_Input.Initialize();
        bl_Input.CheckGamePadRequired();

#if LM
        bl_LevelManager.Instance.Initialize();
        if (bl_LevelManager.Instance.isNewLevel)
        {
            var info = bl_LevelManager.Instance.GetLevel();
            m_LevelUI.Icon.sprite = info.Icon;
            m_LevelUI.LevelNameText.text = info.Name;
            m_LevelUI.Root.SetActive(true);
            bl_LevelManager.Instance.Refresh();
        }
        bl_LevelManager.Instance.GetInfo();
#endif
#if CUSTOMIZER
        SetActiveAddonObject(2, true);
#endif
        if (bl_GameData.Instance.MasterCanKickPlayers)
        {
            PhotonNetwork.EnableCloseConnection = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ApplyRuntimeSettings()
    {
        if (bl_MFPS.Settings != null)
        {
            var apm = bl_ResolutionSettings.ResolutionHandler == null ? bl_RuntimeSettingsProfile.ResolutionApplication.ApplyCurrent : bl_RuntimeSettingsProfile.ResolutionApplication.NoApply;
            bl_MFPS.Settings.ApplySettings(apm, false);
            Application.targetFrameRate = bl_MFPS.Settings.RefreshRates[(int)bl_MFPS.Settings.GetSettingOf("Frame Rate")];
            bl_MFPS.MusicVolume = (float)bl_MFPS.Settings.GetSettingOf("Music Volume");
            bl_AudioController.Instance.ForceStopAllFades();
            bl_AudioController.Instance.BackgroundVolume = bl_MFPS.MusicVolume;
            bl_AudioController.Instance.MaxBackgroundVolume = bl_MFPS.MusicVolume;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void FullSetUp()
    {
#if LM
        if (LevelIcon != null)
        {
            LevelIcon.gameObject.SetActive(true);
            var pli = bl_LevelManager.Instance.GetLevel();
            LevelIcon.sprite = pli.Icon;
            var plt = LevelIcon.GetComponentInChildren<TextMeshProUGUI>();
            if (plt != null) plt.text = pli.LevelID.ToString();
        }
#else
        if (LevelIcon != null) LevelIcon.gameObject.SetActive(false);
#endif

#if SHOP
        SetActiveAddonObject(9, true);
#endif
#if PSELECTOR
        SetActiveAddonObject(10, !bl_PlayerSelector.InMatch);
#endif
        SetActiveAddonObject(0, false);
#if CLASS_CUSTOMIZER
        SetActiveAddonObject(0, true);
#endif
        SetRegionDropdown();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLanguageChange(Dictionary<string, string> lang)
    {
#if LOCALIZATION
        bl_LobbyRoomCreatorUI.Instance.SetupSelectors();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetSettings()
    {
        LoadSettings();
        FullSetUp();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetRegionDropdown()
    {
        //when Photon Server is used
        if (!PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
        {
            //disable the dropdown server selection since it doesn't work with Photon Server.
            ServersDropdown.gameObject.SetActive(false);
            return;
        }
        string key = PlayerPrefs.GetString(PropertiesKeys.GetUniqueKey("preferredregion"), bl_Lobby.Instance.DefaultServer.ToString());
        string[] Regions = Enum.GetNames(typeof(SeverRegionCode));
        for (int i = 0; i < Regions.Length; i++)
        {
            if (key == Regions[i])
            {
                int id = i;
                if (id > 4) { id--; } // ids jumps from 3 to 5
                ServersDropdown.value = id;
                bl_Lobby.Instance.ServerRegionID = id;
                break;
            }
        }
        ServersDropdown.RefreshShownValue();

        // Add the callback after setup the dropdown to avoid call the listener unintentional.
        ServersDropdown.onValueChanged.AddListener(ChangeServerCloud);
    }
    #endregion

    #region UI Callbacks
    /// <summary>
    /// 
    /// </summary>
    public void EnterName(TMP_InputField field = null)
    {
        if (field == null || string.IsNullOrEmpty(field.text))
            return;

        int check = bl_GameData.Instance.CheckPlayerName(field.text);
        if (check == 1)
        {
            bl_Lobby.Instance.CachePlayerName = field.text;
            SetEnableWindow("user password", true);
            return;
        }
        else if (check == 2)
        {
            field.text = string.Empty;
            return;
        }
        bl_Lobby.Instance.SetPlayerName(field.text);
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnterPassword(TMP_InputField field = null)
    {
        if (field == null || string.IsNullOrEmpty(field.text))
            return;

        string pass = field.text;
        if (!bl_Lobby.Instance.EnterPassword(pass))
        {
            field.text = string.Empty;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckRoomPassword(RoomInfo room)
    {
        EnterPasswordUI.SetActive(true);
        bl_Lobby.Instance.CheckRoomPassword(room);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnEnterPassworld(TMP_InputField pass)
    {
        if (!bl_Lobby.Instance.SetRoomPassworld(pass.text))
        {
            PasswordLogText.text = "Wrong room password";
        }
    }

    public void ChangeServerCloud(int id)
    {
        requestedRegionID = id;
        confirmationWindow.AskConfirmation(bl_GameTexts.ChangeRegionAsk.Localized(200), () =>
         {
             bl_Lobby.Instance.ConnectToServerRegion(id);
         });
    }

    public void LoadLocalLevel(string level)
    {
        bl_Lobby.Instance.LoadLocalLevel(level);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowBuyCoins()
    {
#if SHOP
        MFPS.Shop.bl_CoinsWindow.Instance.SetActive(true);
#else
        Debug.Log("Require shop addon.");
#endif
    }

#if ULSP
    void OnUpdateDataBaseInfo(MFPS.ULogin.LoginUserInfo info)
    {
        bl_EventHandler.DispatchCoinUpdate(null);
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void Quit()
    {
        confirmationWindow.AskConfirmation(bl_GameTexts.QuitGameConfirmation, () =>
         {
#if UNITY_EDITOR
             EditorApplication.ExecuteMenuItem("Edit/Play");
#else
             Application.Quit();
#endif
         });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="active"></param>
    public void SetActiveAddonObject(int index, bool active)
    {
        if (index < 0 || index > AddonsButtons.Length - 1) return;
        if (AddonsButtons[index] == null) return;

        AddonsButtons[index].SetActive(active);
    }

    public static void ShowEmptyLoading(bool active) => Instance.overAllEmptyLoading.SetActive(active);
    public static void ShowOverAllMessage(string text) => Instance.messageWindow?.ShowMessage(text);
    public static void ShowConfirmationWindow(string text, Action onAccept, Action onCancel = null) => Instance.confirmationWindow?.AskConfirmation(text, onAccept, onCancel);

    public void AutoMatch() { bl_Lobby.Instance.AutoMatch(); }
    public void CreateRoom() { bl_Lobby.Instance.CreateRoom(); }
    public void SetRememberMe(bool value) { bl_Lobby.Instance.SetRememberMe(value); }
    #endregion

    #region Classes
    [System.Serializable]
    public class WindowUI
    {
        public string Name;
        public GameObject UIRoot;
        public Button MenuButton;

        public bool isPersistent = false;//this window will stay show up when change window?
        public bool hidePersistents = false;//force hide persistent windows
        public bool playFadeInOut = false;
        public bool showTopMenu = true;
    }

    [System.Serializable]
    public class PopUpWindows
    {
        public string Name;
        public GameObject Window;
    }

    [System.Serializable]
    public class LevelUI
    {
        public GameObject Root;
        public Image Icon;
        public TextMeshProUGUI LevelNameText;
    }

    private static bl_LobbyUI _instance;
    public static bl_LobbyUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_LobbyUI>();
            }
            return _instance;
        }
    }
    #endregion
}