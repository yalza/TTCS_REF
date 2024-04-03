using UnityEngine;
using MFPS.Runtime.Settings;
using System.Linq;
using TMPro;

public class bl_LobbyRoomCreatorUI : MonoBehaviour
{
    [Header("References")]
    public bl_SingleSettingsBinding MapSettingsSelector;
    public bl_SingleSettingsBinding MaxPlayersSelector;
    public bl_SingleSettingsBinding GameGoalSelector;
    public bl_SingleSettingsBinding TimeLimitSelector;
    public bl_SingleSettingsBinding GameModeSelector;
    public bl_SingleSettingsBinding MaxPingSelector;
    public bl_SingleSettingsBinding BotsActiveSelector;
    public bl_SingleSettingsBinding FriendlyFireSelector;
    public bl_SingleSettingsBinding PerRoundSelector;
    public bl_SingleSettingsBinding TeamSelectionSelector;
    public TMP_InputField roomInputField;
    public TMP_InputField roomPasswordInputField;
    public bl_MFPSRoomPreview roomPreview;

    private GameModeSettings[] allModes;
    private string defaultRoomName = "Server";
    private string[] endPoints = new string[]
    {
        "Players","Minute"
    };

    /// <summary>
    /// 
    /// </summary>
    public void SetupSelectors()
    {
        endPoints[0] = endPoints[0].Localized("player", true);
        endPoints[1] = endPoints[1].Localized("minute", true);
        defaultRoomName = $"{bl_LobbyRoomCreator.Instance.defaultRoomPrefix} {Random.Range(10, 9999)}";
        roomInputField.text = defaultRoomName;

        MapSettingsSelector.SetOptions(bl_LobbyRoomCreator.Instance.MapList.Select(x => x.ShowName).ToArray());
        var allModes = GetModesForCurrentMap();
        GameModeSelector.SetOptions(allModes.Select(x => x.ModeName.Localized(x.gameMode.ToString().ToLower())).ToArray());
        MaxPingSelector.SetOptions(bl_Lobby.Instance.MaxPing.AsStringArray(" ms"));

        SetupGameModeDependentsSelectors();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetupGameModeDependentsSelectors()
    {
        allModes = GetModesForCurrentMap();
        var gameMode = allModes[GameMode];

        MaxPlayersSelector.SetOptions(gameMode.maxPlayers.AsStringArray($" {endPoints[0]}"));
        TimeLimitSelector.SetOptions(gameMode.timeLimits.Select(x => $"{(x / 60)} {endPoints[1]}").ToArray());

        if (gameMode.GameGoalsOptions.Length > 0)
            GameGoalSelector.SetOptions(gameMode.GameGoalsOptions.AsStringArray($" {gameMode.GoalName}"));
        else GameGoalSelector.SetOptions(new string[1] { gameMode.GoalName });

        BotsActiveSelector.gameObject.SetActive(gameMode.supportBots);
        TeamSelectionSelector.SetActive(!gameMode.AutoTeamSelection);
        PerRoundSelector.SetActive(gameMode.RoundModeAllowed == GameModeSettings.RoundModeAllowedOptions.AllowBoth);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnChangeGameMode(int id)
    {
        GameGoalSelector.currentOption = 0;
        MaxPlayersSelector.currentOption = 0;
        TimeLimitSelector.currentOption = 0;
        SetupGameModeDependentsSelectors();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void OnChangeMap(int id)
    {
        roomPreview?.Show(bl_LobbyRoomCreator.Instance.BuildRoomInfo());
        var allModes = GetModesForCurrentMap();
        GameModeSelector.SetOptions(allModes.Select(x => x.ModeName.Localized(x.gameMode.ToString().ToLower())).ToArray());
        SetupGameModeDependentsSelectors();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private GameModeSettings[] GetModesForCurrentMap()
    {
        var currentMap = bl_LobbyRoomCreator.Instance.MapList[MapSettingsSelector.currentOption];
        return currentMap.GetAllowedGameModes(bl_Lobby.Instance.GameModes);
    }

    public void OnChangeRoomPrivate(bool toPrivate) { if (!toPrivate) roomPasswordInputField.text = ""; }
    #region Getters
    public int Map => MapSettingsSelector.currentOption;
    public int GameMode => GameModeSelector.currentOption;
    public GameModeSettings CurrentGameMode
    {
        get
        {
            if(allModes == null)
            {
                allModes = GetModesForCurrentMap();
            }
            return allModes[GameModeSelector.currentOption];
        }
    }
    public int MaxPlayer => MaxPlayersSelector.currentOption;
    public int Goal => GameGoalSelector.currentOption;
    public int TimeLimit => TimeLimitSelector.currentOption;
    public int MaxPing => MaxPingSelector.currentOption;
    public string RoomName => roomInputField.text;
    public string RoomPassword => roomPasswordInputField.text;
    public bool IsPrivate => !string.IsNullOrEmpty(roomPasswordInputField.text);
    public bool FriendlyFire => FriendlyFireSelector.CurrentSelectionAsBool();
    public bool BotsActive => BotsActiveSelector.CurrentSelectionAsBool();
    public bool AutoTeamSelection => TeamSelectionSelector.CurrentSelectionAsBool();
    public RoundStyle GamePerRound => PerRoundSelector.CurrentSelectionAsBool() == true ? RoundStyle.Rounds : RoundStyle.OneMacht;
    #endregion

    private static bl_LobbyRoomCreatorUI _instance;
    public static bl_LobbyRoomCreatorUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_LobbyRoomCreatorUI>();
            }
            if (_instance == null && bl_LobbyUI.Instance != null)
            {
                _instance = bl_LobbyUI.Instance.GetComponentInChildren<bl_LobbyRoomCreatorUI>(true);
            }
            return _instance;
        }set => _instance = value;
    }
}