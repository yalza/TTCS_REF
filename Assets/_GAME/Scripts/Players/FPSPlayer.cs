using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace _GAME.Scripts.Players
{
    public class FPSPlayer
    {
        public string Name;
        public Transform Actor;
        public bool isRealPlayer = true;
        public bool isAlive = true;
        public Team Team = Team.None;

        public Transform AimPosition;

        public PhotonView _actorView;

        public PhotonView ActorView
        {
            get => _actorView;
            set
            {
                _actorView = value;
                if (_actorView != null) ActorViewID = _actorView.ViewID;
            }
        }

        private int _viewID;

        public int ActorViewID
        {
            get => _viewID;
            set => _viewID = value;
        }
        
        public int ActorNumber
        {
            get
            {
                if (ActorView == null || ActorView.Owner == null) return -1;
                return ActorView.Owner.ActorNumber;
            }
        }

        public Player GetNetworkPlayer()
        {
            if (_actorView != null) return ActorView.Owner;
            else
            {
                var playerList = bl_PhotonNetwork.PlayerList;
                foreach (var player in playerList)
                {
                    if (player.NickName == Name) return player;
                }

                return null;
            }
        }

        public object GetPlayerPropertie(string key, object defaultValue = null)
        {
            switch (key)
            {
                case PropertiesKeys.KillsKey:
                    return GetNetworkPlayer().GetKills();
                case PropertiesKeys.DeathsKey:
                    return GetNetworkPlayer().GetDeaths();
                case PropertiesKeys.ScoreKey:
                    return GetNetworkPlayer().GetPlayerScore();
                default:
                    Debug.LogWarning($"Property {key} has not been setup yet.");
                    return defaultValue;
                   
            }
        }

        #region Constructors

        public FPSPlayer(){}

        public FPSPlayer(PhotonView view, bool realPlayer = true,bool alive = true)
        {
            BuildFromeView(view,realPlayer,alive);
        }

        public FPSPlayer BuildFromeView(PhotonView view, bool realPlayer = true, bool alive = true)
        {
            isRealPlayer = realPlayer;
            isAlive = alive;
            if (view == null) return this;
            Actor = view.transform;
            ActorView = view;
            AimPosition = Actor;
            if (view.InstantiationData != null)
            {
                if (isRealPlayer)
                {
                    Name = (string)view.InstantiationData[0];
                    Team = (Team)view.InstantiationData[1];
                }
                else
                {
                    Name = view.Owner.NickName;
                    Team = (Team)view.InstantiationData[0];
                }
            }

            return this;
        }

        #endregion
    }
    
    
}