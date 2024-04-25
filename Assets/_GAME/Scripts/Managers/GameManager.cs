using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace _GAME.Scripts.Managers
{
    public class GameManager : bl_PhotonHelper , IInRoomCallbacks , IConnectionCallbacks
    {
        #region Public Members

        public MatchState GameMatchState;

        public Action onAllPlayersRequiredIn;

        public List<Player> connectedPlayerList = new List<Player>();

        #endregion
        
        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            throw new System.NotImplementedException();
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            throw new System.NotImplementedException();
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            throw new System.NotImplementedException();
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            throw new System.NotImplementedException();
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            throw new System.NotImplementedException();
        }

        public void OnConnected()
        {
            throw new System.NotImplementedException();
        }

        public void OnConnectedToMaster()
        {
            throw new System.NotImplementedException();
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            throw new System.NotImplementedException();
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
            throw new System.NotImplementedException();
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            throw new System.NotImplementedException();
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            throw new System.NotImplementedException();
        }
    }
}
