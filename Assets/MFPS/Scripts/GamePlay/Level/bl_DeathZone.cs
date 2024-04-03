using UnityEngine;
using TMPro;

namespace MFPS.Runtime.Level
{
    public class bl_DeathZone : bl_PhotonHelper
    {
        [LovattoToogle] public bool instaKill = false;
        public int countDown = 5;
        [TextArea(2, 4)]
        public string CustomMessage = "you're in a zone prohibited \n returns to the playing area or die at \n";

        private bool mOn = false;
        private GameObject m_killZoneUI = null;
        private int CountDown;
        private Collider m_Collider = null;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            CountDown = countDown;
            m_Collider = transform.GetComponent<Collider>();

            if (m_Collider == null)
            {
                Debug.LogWarning($"Kill Zone object doesn't have a collider attached!", gameObject);
            }

            m_killZoneUI = bl_UIReferences.Instance.PlayerUI.KillZoneUI;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mCol"></param>
        void OnTriggerEnter(Collider mCol)
        {
            if (mCol.isLocalPlayerCollider())//when is player local enter
            {
                DoPlayerDamage(mCol.transform.root);
            }
            else if (mCol.CompareTag("Metal"))
            {
#if MFPS_VEHICLE
                var vehicle = mCol.GetComponentInParent<Vehicles.bl_VehicleManager>();
                if (vehicle != null && vehicle.IsLocalPlayerInside())
                {
                    DoPlayerDamage(vehicle.transform.root);
                }
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void DoPlayerDamage(Transform root)
        {
            var pdm = root.GetComponentInChildren<bl_PlayerHealthManagerBase>(true);// get the component damage
            if (pdm != null && pdm.isMine && pdm.GetHealth() > 0 && !mOn)
            {
                if (instaKill)
                {
                    bl_MFPS.LocalPlayer.Suicide();
                    return;
                }
                InvokeRepeating(nameof(DoCountDown), 1, 1);
                UpdateUI();
                mOn = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mCol"></param>
        void OnTriggerExit(Collider mCol)
        {
            if (mCol.isLocalPlayerCollider())// if player exit of zone then cancel countdown
            {
                CancelInvoke(nameof(DoCountDown));
                CountDown = countDown; // restart time
                if (m_killZoneUI != null)
                {
                    m_killZoneUI.SetActive(false);
                }
                mOn = false;
            }
        }

        /// <summary>
        /// Start CountDown when player is on Trigger
        /// </summary>
        void DoCountDown()
        {
            CountDown--;
            UpdateUI();
            if (CountDown <= 0)
            {
                GameObject player = FindPlayerRoot(bl_GameManager.LocalPlayerViewID);
                if (player != null)
                {
                    player.GetComponent<bl_PlayerHealthManagerBase>().Suicide();
                }
                CancelInvoke(nameof(DoCountDown));
                CountDown = countDown;
                if (m_killZoneUI != null) m_killZoneUI.SetActive(false);
                mOn = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateUI()
        {
            if (m_killZoneUI == null) return;

            m_killZoneUI.SetActive(true);
            var text = m_killZoneUI.GetComponentInChildren<TextMeshProUGUI>();
            var message = string.IsNullOrEmpty(CustomMessage) ? string.Format(bl_GameTexts.KillZoneMessage, CountDown) : string.Format(CustomMessage, CountDown);
            text.text = message;

        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            var c = GetComponent<BoxCollider>();
            if (c == null) return;

            Gizmos.matrix = transform.localToWorldMatrix;
            if (gameObject == UnityEditor.Selection.activeGameObject)
            {
                // If we are directly selected (and not just our parent is selected)
                // draw with negative size to get an 'inside out' cube we can see from the inside
                Gizmos.color = new Color(1.0f, 1.0f, 0.5f, 0.8f);
                Gizmos.DrawCube(c.center, -c.size);
            }
            Gizmos.color = new Color(1.0f, 0.5f, 0.5f, 0.3f);
            Gizmos.DrawCube(c.center, c.size);

            UnityEditor.Handles.Label(transform.position, "KILL ZONE");
        }
#endif
    }
}