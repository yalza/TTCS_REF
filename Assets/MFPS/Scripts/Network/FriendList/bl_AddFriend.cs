using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Runtime.FriendList
{
    public class bl_AddFriend : MonoBehaviour
    {
        public TMP_InputField nameInput;
        public TextMeshProUGUI logText;
        public Button addButton;

        /// <summary>
        /// 
        /// </summary>
        public void AddFriend()
        {
#if !ULSP
            bl_FriendListBase.Instance?.AddFriend(nameInput);
            gameObject.SetActive(false);
#else
            var name = nameInput.text;
            if (string.IsNullOrEmpty(name)) return;

#if UNITY_EDITOR
            if (!bl_DataBase.IsUserLogged)
            {
                Debug.Log("Player is not logged, can't be checked if user exist in database.");
                Add(name);
                return;
            }
#endif

            addButton.interactable = false;
            bl_DataBase.CheckIfUserExist(this, "nick", name, (exist) =>
               {
                   if (exist)
                   {
                       Add(name);
                   }
                   else
                   {
                       logText.text = $"Player '{name}' not exist.";
                   }
                   nameInput.text = string.Empty;
                   addButton.interactable = true;
               });
#endif
        }

        private void Add(string friendName)
        {
            logText.text = string.Empty;
            bl_FriendListBase.Instance?.AddFriend(friendName);
            gameObject.SetActive(false);
        }
    }
}