using UnityEngine;

namespace MFPS.Runtime.Level
{
    /// <summary>
    /// MFPS door base class
    /// Inherited from this class your custom door system.
    /// </summary>
    public abstract class bl_DoorBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField]private State _doorState = State.Close;
        public State DoorState
        {
            get => _doorState;
            set => _doorState = value;
        }

        /// <summary>
        /// 
        /// </summary>
        private int _doorId = -1;
        public int DoorID
        {
            get => _doorId;
            set => _doorId = value;
        }

        /// <summary>
        /// In this function you should handle the given door state
        /// You should apply the state rotation/position with the animation/tween
        /// </summary>
        public abstract void SetDoorState(State newState);

        /// <summary>
        /// In this function you should handle the given door state
        /// it will be called from the master when a new player enter in room in order to sync the door state.
        /// You should apply the state rotation/position instantly
        /// </summary>
        public abstract void SetDoorStateInstantly(State newState);

        /// <summary>
        /// See the usage implementation in bl_BasicDoor.cs
        /// </summary>
        public abstract void OnUpdateDoor();

        /// <summary>
        /// 
        /// </summary>
        public enum State
        {
            Close,
            OpenToOutside,
            OpenToInside        
        }
    }
}