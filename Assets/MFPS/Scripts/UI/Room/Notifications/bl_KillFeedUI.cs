using UnityEngine;
using MFPS.Internal.Structures;
using MFPS.Runtime.UI.Bindings;

namespace MFPS.Runtime.UI
{
    public class bl_KillFeedUI : bl_KillFeedUIBase
    {
        public int numberOfPooledPrefabs = 6;
        public Transform KillfeedPanel;
        public GameObject KillfeedPrefab;

        private bl_KillFeedUIBindingBase[] pool;
        private int currentPooled = 0;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            PreparePool();
        }

        /// <summary>
        /// 
        /// </summary>
        void PreparePool()
        {
            if (pool != null) return;

            pool = new bl_KillFeedUIBindingBase[numberOfPooledPrefabs];
            for (int i = 0; i < numberOfPooledPrefabs; i++)
            {
                var obj = Instantiate(KillfeedPrefab) as GameObject;
                obj.transform.SetParent(KillfeedPanel, false);
                pool[i] = obj.GetComponent<bl_KillFeedUIBindingBase>();
                obj.SetActive(false);
            }
            KillfeedPrefab.SetActive(false);
        }

        /// <summary>
        /// Global notification (on right corner) when kill someone
        /// </summary>
        public override void SetKillFeed(KillFeed feed)
        {
            if (!bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.KillFeed)) return;

            if (pool == null) PreparePool();

            var newkillfeed = pool[currentPooled];
            newkillfeed.Init(feed);
            newkillfeed.transform.SetParent(KillfeedPanel, false);
            newkillfeed.transform.SetAsFirstSibling();

            currentPooled = (currentPooled + 1) % numberOfPooledPrefabs;
        }
    }
}