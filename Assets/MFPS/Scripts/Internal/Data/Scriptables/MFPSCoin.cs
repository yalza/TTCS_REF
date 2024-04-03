using System;
using System.Text;
using UnityEngine;
using MFPSEditor;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.Storage;
#endif

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(menuName = "MFPS/Shop/Coin", fileName = "Coin")]
    public class MFPSCoin : ScriptableObject
    {
        public string CoinName;
        public string Acronym;
        [Tooltip("The value of this coin with respect to 1, e.g items priced 100 can be purchase with 1000 coins with value of 0.1")]
        public float CoinValue = 1;
        public int InitialCoins = 0;
        [TextArea(2, 3)] public string Description;
        [SpritePreview] public Sprite CoinIcon;

        /// <summary>
        /// Add and save coins
        /// </summary>
        /// <param name="coins">Coins to add</param>
        /// <returns></returns>
        public MFPSCoin Add(int coins, string forUser = "")
        {
#if ULSP
            if (!bl_DataBase.IsUserLogged) return this;

            bl_DataBase.Instance.SaveNewCoins(coins, this);
#else
            int savedCoins = GetCoins(forUser);
            savedCoins += coins;

            PlayerPrefs.SetInt(Key(forUser), savedCoins);
#endif
            return this;
        }

        /// <summary>
        /// Add and save coins
        /// </summary>
        /// <param name="coins">Coins to add</param>
        /// <returns></returns>
        public MFPSCoin Deduct(int coins, string forUser = "")
        {
#if ULSP
            if (!bl_DataBase.IsUserLogged) return this;

            bl_DataBase.Instance.SubtractCoins(coins, this);
#else
            int savedCoins = GetCoins(forUser);
            savedCoins -= coins;

            if(savedCoins < 0)
            {
                Debug.LogWarning("Something weird happen, funds where not verified before execute a transaction");
                savedCoins = 0;
            }

#if !ACTK_IS_HERE
PlayerPrefs.SetInt(Key(forUser), savedCoins);
#else
 ObscuredPrefs.Set<int>(Key(forUser), savedCoins);
#endif                     
#endif
            return this;
        }

        /// <summary>
        /// Get the locally saved coins
        /// </summary>
        /// <returns></returns>
        public int GetCoins(string endPoint = "")
        {
#if ULSP
            if (!bl_DataBase.IsUserLogged)
            {
                // Debug.Log($"You need an account to accesses to the coins.");
                return 0;
            }
            int indexOfCoin = bl_MFPS.Coins.GetIndexOfCoin(this);
            var userCoins = bl_DataBase.LocalUserInstance.Coins;
            if(indexOfCoin >= userCoins.Length)
            {
                Debug.LogWarning($"Local user doesn't have data for this coin '{CoinName} with ID {indexOfCoin}'.");
                return 0;
            }
            return userCoins[indexOfCoin];
#else         
#if !ACTK_IS_HERE
            int savedCoins = PlayerPrefs.GetInt(Key(endPoint), InitialCoins);
#else
            int savedCoins = ObscuredPrefs.Get<int>(Key(endPoint), InitialCoins);
#endif
            return savedCoins;
#endif
        }

        /// <summary>
        /// Return the conversion of this coin to the reference price (value of 1)
        /// </summary>
        /// <param name="realPrice"></param>
        /// <returns></returns>
        public int DoConversion(int realPrice)
        {
            return Mathf.FloorToInt(realPrice / CoinValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string Key(string endPoint = "")
        {
            var k = $"{Application.productName}.coin.{CoinName}.{endPoint}";
            //Most basic obfuscation possible, is not recommended store coins locally.
            //For serious game, store the coins in a external server.
            k = Convert.ToBase64String(Encoding.UTF8.GetBytes(k));
            return k;
        }

        public static implicit operator int(MFPSCoin coin) => bl_MFPS.Coins.GetIndexOfCoin(coin);
        public static explicit operator MFPSCoin(int coinID) => bl_MFPS.Coins.GetAllCoins()[coinID];
    }
}