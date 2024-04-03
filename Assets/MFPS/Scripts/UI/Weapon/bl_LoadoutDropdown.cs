using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Runtime.UI
{
    public class bl_LoadoutDropdown : MonoBehaviour
    {
        private TMP_Dropdown dropdown;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            if (dropdown == null)
            {
                dropdown = GetComponent<TMP_Dropdown>();
                int lid = (int)PlayerClass.Assault.GetSavePlayerClass();
                dropdown.value = lid;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void OnChanged(int value)
        {
            var loadout = (PlayerClass)value;
            loadout.SavePlayerClass();
#if CLASS_CUSTOMIZER
            bl_ClassManager.Instance.CurrentPlayerClass = loadout;
#endif
            bl_EventHandler.DispatchPlayerClassChange(loadout);
        }
    }
}