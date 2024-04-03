using System;
using System.Collections.Generic;
using MFPS.Internal.Scriptables;
using UnityEngine;
using MFPSEditor;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Internal.Structures
{
    [Serializable]
    public class MFPSItemUnlockability
    {
        public UnlockabilityMethod UnlockMethod = UnlockabilityMethod.UnlockedByDefault;
        public ItemTypeEnum ItemType = ItemTypeEnum.Weapon;
        public int Price = 0;       
        public int UnlockAtLevel = 0;
        [Tooltip("Coins with which can't purchase this item; leave empty if all coins are available.")]
        [MFPSCoinID] public int[] NoAllowedCoins;

        /// <summary>
        /// Is this item unlocked for the local player?
        /// </summary>
        public bool IsUnlocked(int itemID)
        {
            if (UnlockMethod == UnlockabilityMethod.UnlockedByDefault) return true;
            return GetLockReason(itemID) == LockReason.None;
        }

        /// <summary>
        /// If locked return the reason, otherwise return: LockReason.None
        /// </summary>
        public LockReason GetLockReason(int itemID)
        {
            var reason = LockReason.None;
            if (UnlockMethod == UnlockabilityMethod.UnlockedByDefault) return reason;
            if (UnlockMethod == UnlockabilityMethod.Hidden) return LockReason.Hidden;

            bool isPurchased = false;
#if SHOP && ULSP
            if (CanBePurchased())
            {
                isPurchased = bl_DataBase.IsItemPurchased((int)ItemType, itemID);
                if (!isPurchased)
                {
                    reason = LockReason.NoPurchased;
                }
            }
#endif
#if LM
            if (CanBeUnlockedByLevel())
            {
                // if the local player level doesn't meets the required level to unlock.
                if (bl_LevelManager.Instance.GetLevelID() < UnlockAtLevel)
                {
                    if (reason == LockReason.NoPurchased) reason = LockReason.NoPurchasedAndLevel;
                    else
                    {
                        // if this item is purchased and both ways are allowed to unlock the item (purchase or level up) then the item is unlock no matter the player level.
                        if(isPurchased && UnlockMethod == UnlockabilityMethod.PurchasedOrLevelUp) reason = LockReason.None;
                        else reason = LockReason.Level;
                    }
                }// if the player have the required level to unlock.
                else
                {
                    // if the item is not purchased, but the player meets the required level to unlock the item.
                    if(reason == LockReason.NoPurchased && UnlockMethod == UnlockabilityMethod.PurchasedOrLevelUp)
                    {
                        reason = LockReason.None;
                    }
                }
            }
#endif
            if (isPurchased) { }
            return reason;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsFree() => Price <= 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CanBePurchased()
        {
            if (UnlockMethod == UnlockabilityMethod.UnlockedByDefault || UnlockMethod == UnlockabilityMethod.LevelUpOnly) return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CanBeUnlockedByLevel()
        {
            if (UnlockMethod == UnlockabilityMethod.UnlockedByDefault || UnlockMethod == UnlockabilityMethod.PurchasedOnly) return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool AllowUnlockByLevel()
        {
            if (!CanBeUnlockedByLevel() || UnlockAtLevel <= 0) return false;
            return true;
        }

        /// <summary>
        /// Get the coins that can be used with this item
        /// Compare from all the available coins in the game (assigned in GameData -> Game Coins)
        /// </summary>
        /// <param name="originalCoins"></param>
        /// <returns></returns>
        public List<MFPSCoin> GetAllowedCoins()
        {
            var all = bl_MFPS.Coins.GetAllCoins();
            var copy = new List<MFPSCoin>();
            foreach (var c in all)
            {
                copy.Add(c);
            }

            if (NoAllowedCoins == null || NoAllowedCoins.Length <= 0) return copy;

            return copy.Except(GetNoAllowedCoins()).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<MFPSCoin> GetNoAllowedCoins()
        {
            var copy = new List<MFPSCoin>();
            for (int i = 0; i < NoAllowedCoins.Length; i++)
            {
                copy.Add((MFPSCoin)NoAllowedCoins[i]);
            }
            return copy;
        }

        [Serializable]
        public enum UnlockabilityMethod
        {
            UnlockedByDefault = 0,
            PurchasedOnly = 1,
            LevelUpOnly,
            PurchasedOrLevelUp,
            Hidden, // for items that are unlocked only by special events
        }

        [System.Serializable]
        public enum ItemTypeEnum
        {
            Weapon = 0,
            WeaponCamo = 1,
            PlayerSkin = 2,
            PlayerAccesory = 3,
            Emblem = 4,
            CallingCard = 5,
            Emote = 6,
            SeasonPass = 7,
            LootBox = 8,
            Bundle = 9,
            CoinPack = 10,
            None = 99,
        }

        public enum LockReason
        {
            None,
            NoPurchased,
            Level,
            NoPurchasedAndLevel,
            Hidden,
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MFPSItemUnlockability))]
    public class MFPSItemUnlockabilityDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.DrawRect(position, new Color(0, 0, 0, 0.1f));
            var r = position;

            r.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            if (property.isExpanded)
            {
                r.y += EditorGUIUtility.singleLineHeight;
                 var unlockMethod = property.FindPropertyRelative("UnlockMethod");
                   EditorGUI.PropertyField(r, unlockMethod);

                r.y += EditorGUIUtility.singleLineHeight;
                var itemType = property.FindPropertyRelative("ItemType");
                EditorGUI.PropertyField(r, itemType);

                var um = (MFPSItemUnlockability.UnlockabilityMethod)unlockMethod.enumValueIndex;
                GUI.enabled = um != MFPSItemUnlockability.UnlockabilityMethod.UnlockedByDefault;
                if(um != MFPSItemUnlockability.UnlockabilityMethod.LevelUpOnly)
                {
                    r.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(r, property.FindPropertyRelative("Price"));
                }

                if (um != MFPSItemUnlockability.UnlockabilityMethod.PurchasedOnly)
                {
                    r.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(r, property.FindPropertyRelative("UnlockAtLevel"));
                }

                if (um != MFPSItemUnlockability.UnlockabilityMethod.LevelUpOnly)
                {
                    r.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(r, property.FindPropertyRelative("NoAllowedCoins"), true);
                }
                GUI.enabled = true;
            }
            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return base.GetPropertyHeight(property, label);
            else
            {
                float less = 0;
                var unlockMethod = property.FindPropertyRelative("UnlockMethod").enumValueIndex;
                if (unlockMethod == 2) less = EditorGUIUtility.singleLineHeight * 2;
                else if(unlockMethod == 1) less = EditorGUIUtility.singleLineHeight;

                return EditorGUI.GetPropertyHeight(property, label, true) - less;
            }
        }
    }
#endif
}