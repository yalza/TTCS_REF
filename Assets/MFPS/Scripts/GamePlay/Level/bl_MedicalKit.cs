using UnityEngine;

namespace MFPS.Runtime.Level
{
    public class bl_MedicalKit : bl_NetworkItem
    {
        [LovattoToogle] public bool autoRespawn = false;
        [Range(0, 100)]
        public int health = 25;

        /// <summary>
        /// 
        /// </summary>
        void OnTriggerEnter(Collider m_other)
        {
            if (!m_other.isLocalPlayerCollider()) return;

            var pdm = m_other.transform.root.GetComponent<bl_PlayerHealthManagerBase>();
            if (pdm == null) { return; }

            //don't pickup if the player has max health
            if (pdm.GetHealth() >= pdm.GetMaxHealth()) return;

            bl_EventHandler.DispatchPickUpHealth(health);

            //should this health kit respawn after certain time?
            if (autoRespawn)
            {
                bl_ItemManagerBase.Instance.RespawnAfter(this);
            }
            else
            {
                DestroySync();
            }
        }
    }
}