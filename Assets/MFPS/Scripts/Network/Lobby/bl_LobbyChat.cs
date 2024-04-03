using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Chat;
using Photon.Pun;
using TMPro;

public class bl_LobbyChat : MonoBehaviour, IChatClientListener
{

    [Header("Settings")]
    public string[] ChannelsToJoinOnConnect; // set in inspector. Demo channels to join automatically.

    public int HistoryLengthToFetch; // set in inspector. Up to a certain degree, previously sent messages can be fetched for context
    private string selectedChannelName = ""; // mainly used for GUI/input
    public bool ShowState = true;

    [Header("References")]
    public GameObject ChatRoot;
    public GameObject ChatButton;
    public RectTransform ChatPanel;     // set in inspector (to enable/disable panel)
    public TMP_InputField InputFieldChat;   // set in inspector
    public TextMeshProUGUI CurrentChannelText;     // set in inspector
    [SerializeField] private TextMeshProUGUI ChannelText = null;
    [SerializeField] private Animator ChatAnimator;
    private readonly Dictionary<string, Toggle> channelToggles = new Dictionary<string, Toggle>();
    public GameObject Title;
    public TextMeshProUGUI StateText; // set in inspector
    public TextMeshProUGUI UserIdText; // set in inspector
    public ChatClient chatClient = null;
    private bool isShow = false;
    private bool subscribed = false;
    // private static string WelcomeText = "Welcome to chat. Type \\help to list commands.";
    private static string HelpText = "\n    -- HELP --\n" +
        "To subscribe to channel(s):\n" +
            "\t<color=#E07B00>\\subscribe</color> <color=green><list of channelnames></color>\n" +
            "\tor\n" +
            "\t<color=#E07B00>\\s</color> <color=green><list of channelnames></color>\n" +
            "\n" +
            "To leave channel(s):\n" +
            "\t<color=#E07B00>\\unsubscribe</color> <color=green><list of channelnames></color>\n" +
            "\tor\n" +
            "\t<color=#E07B00>\\u</color> <color=green><list of channelnames></color>\n" +
            "\n" +
            "To switch the active channel\n" +
            "\t<color=#E07B00>\\join</color> <color=green><channelname></color>\n" +
            "\tor\n" +
            "\t<color=#E07B00>\\j</color> <color=green><channelname></color>\n" +
            "\n" +
            "To send a private message:\n" +
            "\t\\<color=#E07B00>msg</color> <color=green><username></color> <color=green><message></color>\n" +
            "\n" +
            "To change status:\n" +
            "\t\\<color=#E07B00>state</color> <color=green><stateIndex></color> <color=green><message></color>\n" +
            "<color=green>0</color> = Offline " +
            "<color=green>1</color> = Invisible " +
            "<color=green>2</color> = Online " +
            "<color=green>3</color> = Away \n" +
            "<color=green>4</color> = Do not disturb " +
            "<color=green>5</color> = Looking For Group " +
            "<color=green>6</color> = Playing" +
            "\n\n" +
            "To clear the current chat tab (private chats get closed):\n" +
            "\t<color=#E07B00>\\clear</color>";

    IEnumerator Start()
    {
        UserIdText.text = "";
        StateText.text = "";
        StateText.gameObject.SetActive(true);
        UserIdText.gameObject.SetActive(true);
        Title.SetActive(true);
        ChatPanel.gameObject.SetActive(false);
        ChatButton.SetActive(false);
        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat))
        {
            ChatRoot.SetActive(false);
            this.enabled = false;
            yield break;
        }
        while (!bl_GameData.isDataCached) { yield return null; }
        if (!bl_GameData.Instance.UseLobbyChat)
        {
            ChatRoot.SetActive(false);
            this.enabled = false;
            yield break;
        }
        PhotonNetwork.AddCallbackTarget(this);
        subscribed = true;
    }


    public void Connect(string version)
    {
        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat)) return;
        if (chatClient != null && chatClient.State != ChatState.Disconnected) return;

        this.chatClient = new ChatClient(this);
#if !UNITY_WEBGL
        this.chatClient.UseBackgroundWorkerForSending = false;
#endif
        this.chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, version, new Photon.Chat.AuthenticationValues(PhotonNetwork.NickName));
    }

    /// <summary>
    /// 
    /// </summary>
    public void Disconnect()
    {
        if (this.chatClient != null)
        {
            if (chatClient.State == ChatState.Disconnected || chatClient.State == ChatState.Disconnecting) return;

            this.chatClient.Disconnect();
        }
        if (!enabled || !subscribed) return;
        PhotonNetwork.RemoveCallbackTarget(this);
        subscribed = false;
    }

    public void SetShow()
    {
        isShow = !isShow;
        ChatAnimator.SetBool("show", isShow);
    }

    public void OnDestroy()
    {
        if (this.chatClient != null)
        {
            this.chatClient.Disconnect();
        }
        if (!enabled || !subscribed) return;
        PhotonNetwork.RemoveCallbackTarget(this);
        subscribed = false;
    }

    /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
    public void OnApplicationQuit()
    {
        if (this.chatClient != null)
        {
            this.chatClient.Disconnect();
        }
        if (!enabled || !subscribed) return;
        PhotonNetwork.RemoveCallbackTarget(this);
        subscribed = false;
    }

    public void Update()
    {
        if (this.chatClient != null)
        {
            this.chatClient.Service(); // make sure to call this regularly! it limits effort internally, so calling often is ok!
        }

        this.StateText.gameObject.SetActive(ShowState); // this could be handled more elegantly, but for the demo it's ok.
    }


    public void OnEnterSend()
    {
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            SendChatMessage(this.InputFieldChat.text);
            this.InputFieldChat.text = "";
        }
    }

    public void OnClickSend()
    {
        if (this.InputFieldChat != null)
        {
            SendChatMessage(this.InputFieldChat.text);
            this.InputFieldChat.text = "";
        }
    }

    public int TestLength = 2048;
    private byte[] testBytes = new byte[2048];

    private void SendChatMessage(string inputLine)
    {
        if (string.IsNullOrEmpty(inputLine))
        {
            return;
        }
        if ("test".Equals(inputLine))
        {
            if (this.TestLength != this.testBytes.Length)
            {
                this.testBytes = new byte[this.TestLength];
            }

            this.chatClient.SendPrivateMessage(this.chatClient.AuthValues.UserId, testBytes, true);
        }


        bool doingPrivateChat = this.chatClient.PrivateChannels.ContainsKey(this.selectedChannelName);
        string privateChatTarget = string.Empty;
        if (doingPrivateChat)
        {
            // the channel name for a private conversation is (on the client!!) always composed of both user's IDs: "this:remote"
            // so the remote ID is simple to figure out

            string[] splitNames = this.selectedChannelName.Split(new char[] { ':' });
            privateChatTarget = splitNames[1];
        }
        //UnityEngine.Debug.Log("selectedChannelName: " + selectedChannelName + " doingPrivateChat: " + doingPrivateChat + " privateChatTarget: " + privateChatTarget);


        if (inputLine[0].Equals('\\'))
        {
            string[] tokens = inputLine.Split(new char[] { ' ' }, 2);
            if (tokens[0].Equals("\\help"))
            {
                PostHelpToCurrentChannel();
            }
            if (tokens[0].Equals("\\state"))
            {
                int newState = 0;


                List<string> messages = new List<string>();
                messages.Add("i am state " + newState);
                string[] subtokens = tokens[1].Split(new char[] { ' ', ',' });

                if (subtokens.Length > 0)
                {
                    newState = int.Parse(subtokens[0]);
                }

                if (subtokens.Length > 1)
                {
                    messages.Add(subtokens[1]);
                }

                this.chatClient.SetOnlineStatus(newState, messages.ToArray()); // this is how you set your own state and (any) message
            }
            else if ((tokens[0].Equals("\\subscribe") || tokens[0].Equals("\\s")) && !string.IsNullOrEmpty(tokens[1]))
            {
                this.chatClient.Subscribe(tokens[1].Split(new char[] { ' ', ',' }));
            }
            else if ((tokens[0].Equals("\\unsubscribe") || tokens[0].Equals("\\u")) && !string.IsNullOrEmpty(tokens[1]))
            {
                this.chatClient.Unsubscribe(tokens[1].Split(new char[] { ' ', ',' }));
            }
            else if (tokens[0].Equals("\\clear"))
            {
                if (doingPrivateChat)
                {
                    this.chatClient.PrivateChannels.Remove(this.selectedChannelName);
                }
                else
                {
                    ChatChannel channel;
                    if (this.chatClient.TryGetChannel(this.selectedChannelName, doingPrivateChat, out channel))
                    {
                        channel.ClearMessages();
                    }
                }
            }
            else if (tokens[0].Equals("\\msg") && !string.IsNullOrEmpty(tokens[1]))
            {
                string[] subtokens = tokens[1].Split(new char[] { ' ', ',' }, 2);
                if (subtokens.Length < 2) return;

                string targetUser = subtokens[0];
                string message = subtokens[1];
                this.chatClient.SendPrivateMessage(targetUser, message);
            }
            else if ((tokens[0].Equals("\\join") || tokens[0].Equals("\\j")) && !string.IsNullOrEmpty(tokens[1]))
            {
                string[] subtokens = tokens[1].Split(new char[] { ' ', ',' }, 2);

                // If we are already subscribed to the channel we directly switch to it, otherwise we subscribe to it first and then switch to it implicitly
                if (channelToggles.ContainsKey(subtokens[0]))
                {
                    ShowChannel(subtokens[0]);
                }
                else
                {
                    this.chatClient.Subscribe(new string[] { subtokens[0] });
                }
            }
            else
            {
                Debug.Log("The command '" + tokens[0] + "' is invalid.");
            }
        }
        else
        {
            if (doingPrivateChat)
            {
                this.chatClient.SendPrivateMessage(privateChatTarget, inputLine);
            }
            else
            {
                this.chatClient.PublishMessage(this.selectedChannelName, inputLine);
            }
        }
    }

    public void PostHelpToCurrentChannel()
    {
        this.CurrentChannelText.text += HelpText;
    }

    public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
    {
        if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
        {
            UnityEngine.Debug.LogError(message);
        }
        else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        else
        {
            UnityEngine.Debug.Log(message);
        }
    }

    public void OnConnected()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat)) return;
        if (this.ChannelsToJoinOnConnect != null && this.ChannelsToJoinOnConnect.Length > 0)
        {
            this.chatClient.Subscribe(this.ChannelsToJoinOnConnect, this.HistoryLengthToFetch);
        }

        UserIdText.text = "Connected as " + PhotonNetwork.NickName;
        this.ChatPanel.gameObject.SetActive(true);
        this.chatClient.SetOnlineStatus(ChatUserStatus.Online); // You can set your online state (without a mesage).
        ChannelText.text = selectedChannelName.ToUpper();
        ChatButton.SetActive(bl_GameData.Instance.UseLobbyChat);
        this.chatClient.SetOnlineStatus(ChatUserStatus.Online);
        Debug.Log("Lobby Chat connected");
    }

    public void OnDisconnected()
    {
        Debug.Log("Lobby chat disconnected");
    }

    public void OnChatStateChange(ChatState state)
    {
        // use OnConnected() and OnDisconnected()
        // this method might become more useful in the future, when more complex states are being used.

        this.StateText.text = state.ToString();
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        // in this demo, we simply send a message into each channel. This is NOT a must have!
        foreach (string channel in channels)
        {
            this.chatClient.PublishMessage(channel, "says 'hi'."); // you don't HAVE to send a msg on join but you could.
        }

        // Switch to the first newly created channel
        ShowChannel(channels[0]);
    }

    private void InstantiateChannelButton(string channelName)
    {
        if (this.channelToggles.ContainsKey(channelName))
        {
            Debug.Log("Skipping creation for an existing channel toggle.");
            return;
        }
    }

    public void OnUnsubscribed(string[] channels)
    {
        foreach (string channelName in channels)
        {
            if (this.channelToggles.ContainsKey(channelName))
            {
                Toggle t = this.channelToggles[channelName];
                Destroy(t.gameObject);

                this.channelToggles.Remove(channelName);

                Debug.Log("Unsubscribed from channel '" + channelName + "'.");

                // Showing another channel if the active channel is the one we unsubscribed from before
                if (channelName == selectedChannelName && channelToggles.Count > 0)
                {
                    IEnumerator<KeyValuePair<string, Toggle>> firstEntry = channelToggles.GetEnumerator();
                    firstEntry.MoveNext();

                    ShowChannel(firstEntry.Current.Key);

                    firstEntry.Current.Value.isOn = true;
                }
            }
            else
            {
                Debug.Log("Can't unsubscribe from channel '" + channelName + "' because you are currently not subscribed to it.");
            }
        }
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (channelName.Equals(this.selectedChannelName))
        {
            // update text
            ShowChannel(this.selectedChannelName);
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        // as the ChatClient is buffering the messages for you, this GUI doesn't need to do anything here
        // you also get messages that you sent yourself. in that case, the channelName is determinded by the target of your msg
        this.InstantiateChannelButton(channelName);

        byte[] msgBytes = message as byte[];
        if (msgBytes != null)
        {
            Debug.Log("Message with byte[].Length: " + msgBytes.Length);
        }
        if (this.selectedChannelName.Equals(channelName))
        {
            ShowChannel(channelName);
        }
    }

    /// <summary>
    /// New status of another user (you get updates for users set in your friends list).
    /// </summary>
    /// <param name="user">Name of the user.</param>
    /// <param name="status">New status of that user.</param>
    /// <param name="gotMessage">True if the status contains a message you should cache locally. False: This status update does not include a
    /// message (keep any you have).</param>
    /// <param name="message">Message that user set.</param>
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {

        Debug.LogWarning("status: " + string.Format("{0} is {1}. Msg:{2}", user, status, message));
    }

    public void AddMessageToSelectedChannel(string msg)
    {
        ChatChannel channel = null;
        bool found = this.chatClient.TryGetChannel(this.selectedChannelName, out channel);
        if (!found)
        {
            Debug.Log("AddMessageToSelectedChannel failed to find channel: " + this.selectedChannelName);
            return;
        }

        if (channel != null)
        {
            channel.Add("Bot", msg, 0);
        }
    }

    public void ShowChannel(string channelName)
    {
        if (string.IsNullOrEmpty(channelName))
        {
            return;
        }

        ChatChannel channel = null;
        bool found = this.chatClient.TryGetChannel(channelName, out channel);
        if (!found)
        {
            Debug.Log("ShowChannel failed to find channel: " + channelName);
            return;
        }

        this.selectedChannelName = channelName;
        ChannelText.text = selectedChannelName.ToUpper();
        this.CurrentChannelText.text = channel.ToStringMessages();

        foreach (KeyValuePair<string, Toggle> pair in channelToggles)
        {
            pair.Value.isOn = pair.Key == channelName ? true : false;
        }
    }

    public void OnUserSubscribed(string channel, string user)
    {     
    }

    public void OnUserUnsubscribed(string channel, string user)
    {     
    }

    public bool isConnected() { if (chatClient == null) { return false; } return chatClient.State == ChatState.ConnectedToFrontEnd; }

    private static bl_LobbyChat _instance;
    public static bl_LobbyChat Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<bl_LobbyChat>();
            return _instance;
        }
    }
}