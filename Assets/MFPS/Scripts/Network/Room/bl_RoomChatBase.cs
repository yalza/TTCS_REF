public abstract class bl_RoomChatBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    private bool _isChatAvailable = true;
    public bool IsChatAvailable
    {
        get => _isChatAvailable;
        set => _isChatAvailable = value;
    }

    /// <summary>
    /// Local player sent text to all
    /// </summary>
    /// <param name="text"></param>
    public abstract void SetChat(string text);

    /// <summary>
    /// Local client only
    /// </summary>
    /// <param name="text"></param>
    public abstract void SetChatLocally(string text);

    /// <summary>
    /// Should take the text from the input field 
    /// Then do the same as in SetChat(...);
    /// </summary>
    public abstract void OnSubmit();

    /// <summary>
    /// Active / Deactivate the chat UI.
    /// </summary>
    public abstract void SetActiveUI(bool active);

    /// <summary>
    /// 
    /// </summary>
    private static bl_RoomChatBase _instance;
    public static bl_RoomChatBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_RoomChatBase>(); }
            return _instance;
        }
    }
}