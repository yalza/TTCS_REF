using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;

namespace MFPS.Runtime.FriendList
{
    public class bl_FriendListUI : bl_FriendListUIBase
    {
        [Header("Settings")]
        public bool SortOnline = true;
        public float minifiedWidth = 30;
        public float expandWidth = 200;

        [Header("References")]
        public GameObject FriendUIPrefab = null;
        [SerializeField] private bl_FriendInfoBase emptyRow = null;
        public Transform PanelList = null;
        public TextMeshProUGUI FriendsCountText = null;
        [SerializeField] private Image ArrowImage = null;
        [SerializeField] private Sprite UpArrowSprite = null;
        [SerializeField] private Sprite BottomArrowSprite = null;
        [SerializeField] private TextMeshProUGUI LogText = null;
        [SerializeField] private TextMeshProUGUI ConnectFriendsText = null;
        public GameObject Panel;
        [SerializeField] private RectTransform windowRect = null;
        public GameObject[] expandObjects;
        public AnimationCurve transitionCurve;

        private List<bl_FriendInfoBase> cacheFriendsInfo = new List<bl_FriendInfoBase>();
        private int OnlineCount = 0;
        private bool onlineFirst = false;
        private bool isExpand = false;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            onlineFirst = SortOnline;
            LogText.text = string.Empty;
            ArrowImage.sprite = (onlineFirst == true) ? UpArrowSprite : BottomArrowSprite;
            FriendUIPrefab.SetActive(false);
            UpdateFriendList(true);
            if (LogText != null)
            {
                LogText.canvasRenderer.SetAlpha(0);
            }
            if (!PhotonNetwork.IsConnected)
            {
                Panel.SetActive(false);
            }
            windowRect.sizeDelta = new Vector2(isExpand ? expandWidth : minifiedWidth, windowRect.sizeDelta.y);
            foreach (var item in expandObjects)
            {
                if (item == null) continue;
                item.SetActive(isExpand);
            }
            if (emptyRow != null) emptyRow.Expand(isExpand);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsOpen()
        {
            return isExpand;
        }

        /// <summary>
        /// Instance UI in Friend panel for each friends
        /// </summary>
        /// <param name="friends"></param>
        public void InstanceFriendList(string[] friends)
        {
            for (int i = 0; i < friends.Length; i++)
            {
                GameObject f = Instantiate(FriendUIPrefab) as GameObject;
                f.SetActive(true);
                f.transform.SetParent(PanelList, false);
            }
        }

        /// <summary>
        /// This is called each time that Friend list is update
        /// while updating a friends list, Photon will temporarily set
        // isOnline and isInRoom to false
        // if you update on a timer, you will notice state rapidly
        // switching between offline and online
        // therefore, we will store online state and room in a
        // dictionary and wait until an update is actually received
        // and store the updated value
        /// </summary>
        public void OnUpdatedFriendList()
        {
            if (!PhotonNetwork.IsConnected || PhotonNetwork.InRoom)
            {
                return;
            }

            if (cacheFriendsInfo.Count <= 0 || bl_FriendListBase.Instance.GetStatus() == bl_FriendListBase.Status.Fetching)
            {
                UpdateFriendList(true);
            }
            else
            {
                UpdateFriendList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void UpdateFriendList(bool instance = false)
        {
            if (bl_FriendListBase.Instance.FriendsCount <= 0 || PhotonNetwork.InRoom)
            {
                return;
            }

            FriendInfo[] friends = bl_FriendListBase.Instance.GetFriends();
            if (emptyRow != null) emptyRow.SetActive(friends.Length <= 0);
            if (friends.Length <= 0) return;

            if (instance)
            {
                CleanCacheList();
                List<FriendInfo> OnlineFriends = new List<FriendInfo>();
                List<FriendInfo> OfflineFriends = new List<FriendInfo>();

                for (int i = 0; i < friends.Length; i++)
                {
                    if (friends[i].UserId == "Null") continue;

                    if (onlineFirst)
                    {
                        if (friends[i].IsOnline)
                        {
                            OnlineFriends.Add(friends[i]);
                        }
                        else
                        {
                            OfflineFriends.Add(friends[i]);
                        }
                    }
                    else
                    {
                        GameObject f = Instantiate(FriendUIPrefab) as GameObject;
                        f.SetActive(true);
                        var info = f.GetComponent<bl_FriendInfoBase>();
                        info.SetFriendData(friends[i], this);
                        f.name = friends[i].UserId;
                        cacheFriendsInfo.Add(info);

                        f.transform.SetParent(PanelList, false);
                    }
                }

                if (onlineFirst)
                {
                    if (OnlineFriends.Count > 0)
                    {
                        InstanceFriend(OnlineFriends.ToArray());
                    }
                    if (OfflineFriends.Count > 0)
                    {
                        InstanceFriend(OfflineFriends.ToArray());
                    }
                }
                ConnectFriendsText.text = OnlineFriends.Count.ToString();

            }
            else//Just update list
            {
                for (int i = 0; i < cacheFriendsInfo.Count; i++)
                {
                    if (cacheFriendsInfo[i] != null)
                    {
                        cacheFriendsInfo[i].UpdateFriendInfo(friends);
                    }
                    else
                    {
                        cacheFriendsInfo.RemoveAt(i);
                    }
                }
                if (emptyRow != null) emptyRow.SetActive(cacheFriendsInfo.Count <= 0);
            }
            UpdateCount(friends);
        }

        /// <summary>
        /// 
        /// </summary>
        void InstanceFriend(FriendInfo[] friends)
        {
            for (int i = 0; i < friends.Length; i++)
            {
                GameObject f = Instantiate(FriendUIPrefab) as GameObject;
                f.SetActive(true);
                var info = f.GetComponent<bl_FriendInfoBase>();
                info.SetFriendData(friends[i], this);
                cacheFriendsInfo.Add(info);
                f.name = friends[i].UserId;
                f.transform.SetParent(PanelList, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void UpdateCount(FriendInfo[] friends)
        {
            OnlineCount = 0;
            foreach (FriendInfo info in friends)
            {
                if (info.IsOnline)
                {
                    OnlineCount++;
                }
            }
            if (FriendsCountText != null)
            {
                FriendsCountText.text = OnlineCount + "/" + friends.Length;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CleanCacheList()
        {
            if (cacheFriendsInfo.Count > 0)
            {
                for (int i = 0; i < cacheFriendsInfo.Count; i++)
                {
                    Destroy(cacheFriendsInfo[i].gameObject);
                }
            }
            cacheFriendsInfo.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetOnlineFirst(bool first)
        {
            onlineFirst = first;
            ArrowImage.sprite = (onlineFirst == true) ? UpArrowSprite : BottomArrowSprite;
            UpdateFriendList(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeOnlineFirst()
        {
            onlineFirst = !onlineFirst;
            ArrowImage.sprite = (onlineFirst == true) ? UpArrowSprite : BottomArrowSprite;
            UpdateFriendList(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ToggleView()
        {
            isExpand = !isExpand;
            ExpandView(isExpand);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expand"></param>
        public void ExpandView(bool expand)
        {
            isExpand = expand;
            StopCoroutine(nameof(DoExpand));
            StartCoroutine(nameof(DoExpand));
            foreach (var item in cacheFriendsInfo)
            {
                if (item == null) continue;
                item.Expand(isExpand);
            }
            foreach (var item in expandObjects)
            {
                if (item == null) continue;
                item.SetActive(expand);
            }
            if (emptyRow != null) emptyRow.Expand(isExpand);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoExpand()
        {
            float d = 0;
            var origin = windowRect.sizeDelta;
            var width = isExpand ? expandWidth : minifiedWidth;
            var target = new Vector2(width, origin.y);
            float t = 0;

            while(d < 1)
            {
                d += Time.deltaTime / 0.3f;
                t = transitionCurve.Evaluate(d);
                windowRect.sizeDelta = Vector2.Lerp(origin, target, t);
                yield return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        public override void ShowMessage(string log)
        {
            CancelInvoke(nameof(HideLog));
            if (LogText != null)
            {
                LogText.text = log;
                LogText.CrossFadeAlpha(1, 0.7f, true);
            }
            Invoke(nameof(HideLog), 3);
        }

        /// <summary>
        /// 
        /// </summary>
        void HideLog()
        {
            if (LogText != null)
            {
                LogText.CrossFadeAlpha(0, 1, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActiveList(bool active)
        {
            Panel.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fname"></param>
        /// <returns></returns>
        public override bool IsPlayerListed(string fname)
        {
            for (int i = 0; i < cacheFriendsInfo.Count; i++)
            {
                if (cacheFriendsInfo[i].name == fname)
                {
                    return true;
                }
            }
            return false;

        }
    }
}