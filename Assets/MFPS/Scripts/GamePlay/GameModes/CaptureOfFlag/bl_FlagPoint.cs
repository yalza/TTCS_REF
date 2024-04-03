using UnityEngine;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace MFPS.GameModes.CaptureOfFlag
{
    public class bl_FlagPoint : bl_PhotonHelper
    {
        public Team flagTeam;
        public FlagState State = FlagState.InHome;
        public Texture2D FlagIcon;
        public Transform IconTarget;
        [SerializeField] private Transform homeMark = null;
        public Vector2 IconSize = new Vector2(7, 7);
        public float ReturnTime;

        Vector3 originalPos, originalRot, originalScale;
        bl_FlagPoint oppositeFlag;
        public bl_PlayerSettings carriyingPlayer/* { get; set; } = null*/;
        private Color IconColor;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            originalPos = transform.position;
            originalRot = transform.eulerAngles;
            originalScale = transform.localScale;
            oppositeFlag = bl_CaptureOfFlag.Instance.GetFlag(bl_CaptureOfFlag.GetOppositeTeam(flagTeam));
            IconColor = flagTeam.GetTeamColor();
            if (homeMark != null) homeMark.parent = transform.parent;
            bl_EventHandler.onLocalPlayerDeath += this.OnLocalPlayerDeath;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
            bl_EventHandler.onLocalPlayerDeath -= this.OnLocalPlayerDeath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeFlagState(FlagState newState)
        {
            if (State == newState) return;

            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", 0);
            data.Add("state", newState);
            data.Add("team", flagTeam);
            data.Add("player", bl_PhotonNetwork.LocalPlayer);
            data.Add("viewID", bl_GameManager.LocalPlayerViewID);
            bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.CaptureOfFlagMode, data);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnTriggerEnter(Collider collider)
        {
            if (collider.isLocalPlayerCollider())
            {
                var player = collider.gameObject.GetComponent<bl_PlayerSettings>();
                if (CanBePickedUpBy(player) == true)
                {
                    ChangeFlagState(FlagState.PickUp);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalPlayerDeath()
        {
            if (carriyingPlayer == null)
            {
                return;
            }

            var local = bl_MFPS.LocalPlayerReferences;
            if (local == null) return;

            if (carriyingPlayer.View.ViewID == local.ViewID)
            {
                // Drop the flag
                var data = bl_UtilityHelper.CreatePhotonHashTable();
                data.Add("cmd", 1);
                data.Add("team", flagTeam);
                data.Add("pos", transform.position);
                bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.CaptureOfFlagMode, data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void HandleFlagCapture()
        {
            if (carriyingPlayer == null)
                return;

            if (carriyingPlayer.View.ViewID != bl_MFPS.LocalPlayer.ViewID)
            {
                return;
            }

            if (oppositeFlag.IsHome() == true && bl_UtilityHelper.Distance(transform.position, oppositeFlag.transform.position) <= bl_CaptureOfFlag.Instance.captureAreaRange)
            {
                ChangeFlagState(FlagState.Captured);
                // change the state locally instantly to prevent detect the capture multiple times.
                State = FlagState.Captured;
            }
        }

        /// <summary>
        /// Determines whether this instance is at the home base
        /// </summary>
        /// <returns></returns>
        public bool IsHome()
        {
            return transform.position == originalPos;
        }

        /// <summary>
        /// Called on all clients when a player drop the flag.
        /// </summary>
        /// <param name="data"></param>
        public void DropFlag(Hashtable data)
        {
            var position = (Vector3)data["pos"];

            State = FlagState.Dropped;
            carriyingPlayer = null;
            transform.parent = null;
            transform.position = position;
            transform.eulerAngles = originalRot;
            transform.localScale = originalScale;

            if (bl_PhotonNetwork.IsMasterClient)
            {
                Invoke(nameof(ReturnInvoke), ReturnTime);
            }
        }

        /// <summary>
        /// Called on all clients when a player successfully capture the flag.
        /// </summary>
        public void OnCapture(Player actor)
        {
            carriyingPlayer = null;
            SetFlagToOrigin();

            //Only the player who captures the flag, updates the properties
            if (bl_PhotonNetwork.LocalPlayer.ActorNumber == actor.ActorNumber)
            {
                bl_KillFeedBase.Instance.SendTeamHighlightMessage(bl_PhotonNetwork.LocalPlayer.NickName, bl_GameTexts.CaptureTheFlag, actor.GetPlayerTeam());
                bl_GameManager.Instance.SetPointFromLocalPlayer(1, GameMode.CTF);
                //Add Point for personal score
                bl_PhotonNetwork.LocalPlayer.PostScore(bl_CaptureOfFlag.Instance.scorePerCapture);
                bl_CaptureOfFlag.Instance.onCapture?.Invoke();
            }
        }

        /// <summary>
        /// Called on all clients when a player recover the flag.
        /// </summary>
        public void Recover(Player actor)
        {
            SetFlagToOrigin();

            if (actor.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber)
            {
                bl_PhotonNetwork.LocalPlayer.PostScore(bl_CaptureOfFlag.Instance.scorePerRecover);
                bl_CaptureOfFlag.Instance.onRecover?.Invoke();
            }
        }

        /// <summary>
        /// Called on all clients when a player pick up this flag.
        /// </summary>
        /// <param name="actor"></param>
        public void OnPickup(Player actor, int viewID)
        {
            Transform actorTransform = bl_GameManager.Instance.FindActor(viewID);
            if (actorTransform == null)
            {
                return;
            }

            var logic = actorTransform.GetComponent<bl_PlayerSettings>();
            if (!CanBePickedUpBy(logic))
            {
                return;
            }

            if (logic.PlayerTeam == flagTeam)
            {
                if (IsHome() == false)
                {
                    // the flag is dropped and taken by a team member = recovering the flag.
                    if (bl_PhotonNetwork.IsMasterClient) ChangeFlagState(FlagState.InHome);
                }
            }
            else
            {
                SetFlagToCarrier(logic);
            }

            //show capture notification
            if (bl_PhotonNetwork.LocalPlayer.ActorNumber == actor.ActorNumber)
            {
                Team enemyTeam = bl_CaptureOfFlag.GetOppositeTeam(bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
                string obtainedText = string.Format(bl_GameTexts.ObtainedFlag, enemyTeam.GetTeamName());
                bl_KillFeedBase.Instance.SendTeamHighlightMessage(bl_PhotonNetwork.LocalPlayer.NickName, obtainedText, bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
                bl_CaptureOfFlag.Instance.onPickUp?.Invoke();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="carrier"></param>
        public void SetFlagToCarrier(bl_PlayerSettings carrier)
        {
            carriyingPlayer = carrier;
            transform.parent = carrier.FlagPosition;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            if (bl_CaptureOfFlag.Instance.moveFlagWithCarrierMotion && carrier.carrierPoint != null)
                transform.parent = carrier.carrierPoint;
        }

        /// <summary>
        /// 
        /// </summary>
        void ReturnInvoke()
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", 2);
            data.Add("team", flagTeam);
            bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.CaptureOfFlagMode, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logic"></param>
        /// <returns></returns>
        public bool CanBePickedUpBy(bl_PlayerSettings logic)
        {
            //If the flag is at its home position, only the enemy team can grab it
            if (IsHome() == true)
            {
                return logic.PlayerTeam != flagTeam;
            }

            //If another player is already carrying the flag, no one else can grab it
            if (carriyingPlayer != null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetFlagToOrigin()
        {
            transform.parent = null;
            transform.position = originalPos;
            transform.eulerAngles = originalRot;
            transform.localScale = originalScale;
            State = FlagState.InHome;
        }

        #region GUI
        void OnGUI()
        {
            if (carriyingPlayer != null && carriyingPlayer.View.ViewID == bl_MFPS.LocalPlayer.ViewID) return;

            GUI.color = IconColor;
            if (bl_GameManager.Instance.CameraRendered)
            {
                Vector3 vector = bl_GameManager.Instance.CameraRendered.WorldToScreenPoint(this.IconTarget.position);
                if (vector.z > 0)
                {
                    GUI.DrawTexture(new Rect(vector.x - 5, (Screen.height - vector.y) - 7, 13 + IconSize.x, 13 + IconSize.y), this.FlagIcon);
                }
            }
            GUI.color = Color.white;
        }

        private SphereCollider SpheCollider;
        private void OnDrawGizmos()
        {
            if (SpheCollider != null)
            {
                Vector3 v = SpheCollider.bounds.center;
                v.y = transform.position.y;
                bl_UtilityHelper.DrawWireArc(v, SpheCollider.radius * transform.lossyScale.x, 360, 20, Quaternion.identity);
            }
            else
            {
                SpheCollider = GetComponent<SphereCollider>();
            }
        }
        #endregion

        [System.Serializable]
        public enum FlagState
        {
            InHome = 0,
            PickUp = 1,
            Captured = 2,
            Dropped = 3,
        }
    }
}