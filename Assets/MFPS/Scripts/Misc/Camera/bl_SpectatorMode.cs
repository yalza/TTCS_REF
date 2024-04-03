using UnityEngine;

public class bl_SpectatorMode : bl_SpectatorModeBase
{
    [LovattoToogle] public bool leaveSpectatorWithEscape = false;
    [SerializeField] private GameObject content = null;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (EnterAsSpectator)
        {
            EnterAsSpectator = false;
            bl_GameManager.Instance.FirstSpawnDone = true;
            SetActiveSpectatorMode(true);
        }
        else
        {
            SetActiveUI(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActiveSpectatorMode(bool active)
    {
        
        bl_UtilityHelper.LockCursor(active);
        if (active)
        {
            SetActiveUI(true);
            bl_RoomCameraBase.Instance.SetCameraMode(bl_RoomCameraBase.CameraMode.Spectator);
            bl_RoomCameraBase.Instance.SetActive(active);
            bl_UIReferences.Instance.ShowMenu(false);
            bl_PauseMenuBase.Instance.CloseMenu();
            bl_UIReferences.Instance.SetActiveChangeTeamButton(false);
        }
        else
        {
            SetActiveUI(false);
            if (bl_GameData.Instance.onLeaveSpectatorMode == LeaveSpectatorModeAction.ReturnToLobby)
            {
                bl_UtilityHelper.LockCursor(false);
                bl_UIReferences.Instance.leaveRoomConfirmation.AskConfirmation(bl_GameTexts.LeaveMatchWarning.Localized("areusulega"), () =>
                {
                    bl_RoomMenu.Instance.LeaveRoom(false);
                }, () => { bl_UtilityHelper.LockCursor(true); });
                return;
            }

            bl_UIReferences.Instance.SetActiveChangeTeamButton(true);
            bl_RoomCameraBase.Instance.ResetCamera();

            if (!bl_GameManager.Joined)
            {
                bl_UIReferences.Instance.ShowMenu(true);
                bl_UIReferences.Instance.SetUpJoinButtons(true);
            }
            bl_PauseMenuBase.Instance.OpenMenu();
        }
        isActive = active;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        ListenInput();
    }

    /// <summary>
    /// 
    /// </summary>
    void ListenInput()
    {
        if (!leaveSpectatorWithEscape) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetActiveSpectatorMode(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActiveUI(bool active) => content.SetActive(active);
}