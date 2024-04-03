using Photon.Pun;

public class bl_GameModeObject : bl_PhotonHelper
{
    public GameMode m_GameMode = GameMode.FFA;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(GetGameMode == m_GameMode);
    }
}